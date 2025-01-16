using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using boost_api.dto.chatDto;
using boost_api.dto.pythonServerDto;
using boost_api.mapper;
using boost_api.model;
using boost_api.repository;
using boost_api.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;

namespace boost_api.controller;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
	private readonly UserManager<AppUser> _userManager;
	private readonly IChatRepository _chatRepository;
	private readonly HttpClient _httpClient;
	private readonly PdfService _pdfService;
	private readonly string _pythonServerUrl = "http://localhost:7563/";

	public ChatController(UserManager<AppUser> userManager, IChatRepository chatRepository, IHttpClientFactory httpClientFactory, PdfService pdfService)
	{
		_userManager = userManager;
		_chatRepository = chatRepository;
		_httpClient = httpClientFactory.CreateClient("PythonClient");
		_pdfService = pdfService;
	}
	
	[HttpGet]
	[Authorize]
	public async Task<IActionResult> GetChats()
	{
		var userId = HttpContext.User.FindFirst("userId")?.Value;
		if (userId == null)
		{
			return Unauthorized();
		}
		
		var chats = await _chatRepository.GetAllAsync(userId);
		var shortEventDtos = chats.Select(c => new ChatShortDto
		{
			Id = c.Id,
			Title = c.Title,
			Type = c.Type switch
			{
				ChatType.PaperAnalysis => "paperAnalysis",
				ChatType.Chat => "chat",
				_ => "unknown"
			},
			Timestamp = c.LastMessageTimestamp
		}).ToList();
		
		return Ok(shortEventDtos);
	}
	
	[HttpGet("{chatId:int}")]
	[Authorize]
	public async Task<IActionResult> GetChat(int chatId)
	{
		var userId = HttpContext.User.FindFirst("userId")?.Value;
		if (userId == null)
		{
			return Unauthorized();
		}
		
		var chat = await _chatRepository.GetByIdAsync(chatId);
		if (chat == null)
		{
			return NotFound();
		}

		if (chat.AppUserId != userId)
		{
			return Unauthorized();
		}
		
		return Ok(chat.ToChatDto());
	}
	
	[HttpPost("chat")]
	[Authorize]
	public async Task<IActionResult> CreateChat([FromBody] NewChatDto chatCreateDto)
	{
	    if (!ModelState.IsValid)
	    {
	        return BadRequest(ModelState);
	    }

	    var userId = HttpContext.User.FindFirst("userId")?.Value;
	    if (userId == null)
	    {
	        return Unauthorized();
	    }

	    var firstMessage = new Message
	    {
	        Body = chatCreateDto.Text,
	        Timestamp = DateTime.Now,
	        IsSentByUser = true
	    };

	    var data = new PythonRequest
	    {
	        Text = "<" + chatCreateDto.Style + "> " + chatCreateDto.Text,
	        Purpose = "answer",
	        CorrectGrammar = true,
	        MaxTokens = 200
	    };
	    var requestUrl = _pythonServerUrl + "generate-response";

	    try
	    {
	        // Create a CancellationToken with a timeout of 1 minute
	        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
	        var response = await _httpClient.PostAsJsonAsync(requestUrl, data, cts.Token);

	        if (response.IsSuccessStatusCode)
	        {
	            var responseBody = await response.Content.ReadAsStringAsync();
	            var pythonResponse = JsonSerializer.Deserialize<PythonResponse>(responseBody);
	            if (pythonResponse == null)
	            {
	                return StatusCode(500, "Python server error");
	            }

	            var secondMessage = new Message
	            {
	                Body = pythonResponse.Response,
	                Timestamp = DateTime.Now,
	                IsSentByUser = false,
	                Analysis = new Analysis
	                {
	                    Academic = pythonResponse.StyleAnalysis.Academic,
	                    Formal = pythonResponse.StyleAnalysis.Formal,
	                    Humorous = pythonResponse.StyleAnalysis.Humorous,
	                    Informal = pythonResponse.StyleAnalysis.Informal
	                }
	            };

	            var chat = new Chat
	            {
	                Title = chatCreateDto.Text.Length > 30
		                ? string.Concat(chatCreateDto.Text.AsSpan(0, 30), "...")
                        : chatCreateDto.Text,
	                Type = ChatType.Chat,
	                Style = chatCreateDto.Style switch
	                {
	                    "academic" => ChatStyle.Academic,
	                    "formal" => ChatStyle.Formal,
	                    "humorous" => ChatStyle.Humorous,
	                    "informal" => ChatStyle.Informal,
	                    _ => ChatStyle.Informal
	                },
	                Messages = [firstMessage, secondMessage],
	                LastMessageTimestamp = secondMessage.Timestamp,
	                AppUserId = userId
	            };

	            var createdChat = await _chatRepository.CreateAsync(chat);
	            if (createdChat == null)
	            {
	                return StatusCode(500, "Database error");
	            }

	            return Ok(createdChat.ToChatDto());
	        }
	        else
	        {
	            return StatusCode(500, "Python server error");
	        }
	    }
	    catch (TaskCanceledException)
	    {
	        // Handle the timeout by generating a generic response
	        var secondMessage = new Message
	        {
	            Body = "The server is currently unavailable. Please try again later.",
	            Timestamp = DateTime.Now,
	            IsSentByUser = false,
	            Analysis = new Analysis
	            {
	                Academic = 0,
	                Formal = 0,
	                Humorous = 0,
	                Informal = 0
	            }
	        };

	        var chat = new Chat
	        {
	            Title = "Chat: " + chatCreateDto.Text[..Math.Min(20, chatCreateDto.Text.Length)],
	            Type = ChatType.Chat,
	            Style = chatCreateDto.Style switch
	            {
	                "academic" => ChatStyle.Academic,
	                "formal" => ChatStyle.Formal,
	                "humorous" => ChatStyle.Humorous,
	                "informal" => ChatStyle.Informal,
	                _ => ChatStyle.Informal
	            },
	            Messages = [firstMessage, secondMessage],
	            LastMessageTimestamp = secondMessage.Timestamp,
	            AppUserId = userId
	        };

	        var createdChat = await _chatRepository.CreateAsync(chat);
	        if (createdChat == null)
	        {
	            return StatusCode(500, "Database error");
	        }

	        return Ok(createdChat.ToChatDto());
	    }
	    catch (Exception e)
	    {
	        return StatusCode(500, e);
	    }
	}

	[HttpPost("paper-analysis")]
	[Authorize]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> CreatePaperAnalysis([FromForm] NewPaperAnalysisDto paperAnalysisDto)
	{
	    if (!ModelState.IsValid)
	    {
	        return BadRequest(ModelState);
	    }

	    var userId = HttpContext.User.FindFirst("userId")?.Value;
	    if (userId == null)
	    {
	        return Unauthorized();
	    }

	    var file = paperAnalysisDto.File;
	    if (file != null && file.ContentType != "application/pdf" || file == null)
	    {
	        return BadRequest("File must be a PDF");
	    }

	    var savedPdfPath = await _pdfService.SaveFileAsync(file);
	    var fileText = _pdfService.ReadPdfText(savedPdfPath);
	    var fileName = file.FileName;

	    var data = new PythonRequest
	    {
	        Text = "<|" + paperAnalysisDto.Style + "|> " + fileText,
	        Purpose = "feedback",
	        CorrectGrammar = true,
	        MaxTokens = 200
	    };
	    var requestUrl = _pythonServerUrl + "generate-response";

	    Console.WriteLine("Sending request to Python server... with data: " + data);

	    try
	    {
	        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(20));
	        var response = await _httpClient.PostAsJsonAsync(requestUrl, data, cts.Token);

	        if (response.IsSuccessStatusCode)
	        {
	            var responseBody = await response.Content.ReadAsStringAsync();
	            Console.WriteLine("Received response from Python server: " + responseBody);
	            var pythonResponse = JsonSerializer.Deserialize<PythonResponse>(responseBody);
	            Console.WriteLine("Deserialized response: " + pythonResponse);
	            if (pythonResponse == null)
	            {
	                return StatusCode(500, "Python server error");
	            }

	            var answer = new Message
	            {
	                Body = pythonResponse.Response,
	                Timestamp = DateTime.Now,
	                IsSentByUser = false,
	                Analysis = new Analysis
	                {
	                    Academic = pythonResponse.StyleAnalysis.Academic,
	                    Formal = pythonResponse.StyleAnalysis.Formal,
	                    Humorous = pythonResponse.StyleAnalysis.Humorous,
	                    Informal = pythonResponse.StyleAnalysis.Informal
	                }
	            };

	            var chat = new Chat
	            {
	                Title = "Analysis on " + fileName,
	                Type = ChatType.PaperAnalysis,
	                Style = paperAnalysisDto.Style switch
	                {
	                    "academic" => ChatStyle.Academic,
	                    "formal" => ChatStyle.Formal,
	                    "humorous" => ChatStyle.Humorous,
	                    "informal" => ChatStyle.Informal,
	                    _ => ChatStyle.Informal
	                },
	                Messages = [answer],
	                LastMessageTimestamp = answer.Timestamp,
	                PaperPath = savedPdfPath,
	                PaperTitle = fileName,
	                AppUserId = userId
	            };

	            var createdChat = await _chatRepository.CreateAsync(chat);
	            if (createdChat == null)
	            {
	                return StatusCode(500, "Database error");
	            }

	            return Ok(createdChat.ToChatDto());
	        }
	        else
	        {
	            return StatusCode(500, "Python server error");
	        }
	    }
	    catch (TaskCanceledException)
	    {
	        // Handle the timeout by generating a generic response
	        var answer = new Message
	        {
	            Body = "The server is currently unavailable. Please try again later.",
	            Timestamp = DateTime.Now,
	            IsSentByUser = false,
	            Analysis = new Analysis
	            {
	                Academic = 0,
	                Formal = 0,
	                Humorous = 0,
	                Informal = 0
	            }
	        };

	        var chat = new Chat
	        {
	            Title = "Paper analysis: " + fileName,
	            Type = ChatType.PaperAnalysis,
	            Style = paperAnalysisDto.Style switch
	            {
	                "academic" => ChatStyle.Academic,
	                "formal" => ChatStyle.Formal,
	                "humorous" => ChatStyle.Humorous,
	                "informal" => ChatStyle.Informal,
	                _ => ChatStyle.Informal
	            },
	            Messages = [answer],
	            LastMessageTimestamp = answer.Timestamp,
	            PaperPath = savedPdfPath,
	            PaperTitle = fileName,
	            AppUserId = userId
	        };

	        var createdChat = await _chatRepository.CreateAsync(chat);
	        if (createdChat == null)
	        {
	            return StatusCode(500, "Database error");
	        }

	        return Ok(createdChat.ToChatDto());
	    }
	    catch (Exception e)
	    {
	        return StatusCode(500, e);
	    }
	}

	[HttpPost("{chatId:int}")]
	[Authorize]
	public async Task<IActionResult> AddMessage(int chatId, [FromBody] MessageNewDto messageDto)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var userId = HttpContext.User.FindFirst("userId")?.Value;
		if (userId == null)
		{
			return Unauthorized();
		}

		var chat = await _chatRepository.GetByIdAsync(chatId);
		if (chat == null)
		{
			return NotFound();
		}

		if (chat.AppUserId != userId)
		{
			return Unauthorized();
		}

		var message = new Message
		{
			Body = messageDto.Body,
			Timestamp = DateTime.Now,
			IsSentByUser = true
		};

		var data = new PythonRequest
		{
			Text = "<|" + chat.Style + "|> " + messageDto.Body,
			Purpose = "answer",
			CorrectGrammar = true,
			MaxTokens = 200
		};
		var requestUrl = _pythonServerUrl + "generate-response";

		try
		{
			var response = await _httpClient.PostAsJsonAsync(requestUrl, data);

			if (response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStringAsync();
				var pythonResponse = JsonSerializer.Deserialize<PythonResponse>(responseBody);
				if (pythonResponse == null)
				{
					return StatusCode(500, "Python server error");
				}

				var answer = new Message
				{
					Body = pythonResponse.Response,
					Timestamp = DateTime.Now,
					IsSentByUser = false,
					Analysis = new Analysis
					{
						Academic = pythonResponse.StyleAnalysis.Academic,
						Formal = pythonResponse.StyleAnalysis.Formal,
						Humorous = pythonResponse.StyleAnalysis.Humorous,
						Informal = pythonResponse.StyleAnalysis.Informal
					}
				};
				
				var updatedChat = await _chatRepository.AddMessagesAsync(chatId, [message, answer]);
				if (updatedChat == null)
				{
					return StatusCode(500, "Database error");
				}

				return Ok(updatedChat.ToChatDto());
			}
			else
			{
				return StatusCode(500, "Python server error");
			}
		}
		catch (Exception e)
		{
			return StatusCode(500, e);
		}
	}
	
	[HttpDelete("{chatId:int}")]
	[Authorize]
	public async Task<IActionResult> DeleteChat(int chatId)
	{
		var userId = HttpContext.User.FindFirst("userId")?.Value;
		if (userId == null)
		{
			return Unauthorized();
		}
		
		var chat = await _chatRepository.GetByIdAsync(chatId);
		if (chat == null)
		{
			return NotFound();
		}

		if (chat.AppUserId != userId)
		{
			return Unauthorized();
		}
		
		await _chatRepository.DeleteAsync(chatId);
		return Ok();
	}
}