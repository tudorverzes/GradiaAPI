using System.Diagnostics;
using boost_api.dto.chatDto;
using boost_api.model;

namespace boost_api.mapper;

public static class ChatMapper
{
	public static ChatDto ToChatDto(this Chat chat)
	{
		List<MessageDto> messageDtos = [];
		foreach (var message in chat.Messages)
		{
			var messageDto = new MessageDto
			{
				Id = message.Id,
				Body = message.Body,
				Timestamp = message.Timestamp,
				IsSentByUser = message.IsSentByUser
			};
			
			if (message.Analysis != null)
			{
				var analysisDto = new AnalysisDto
				{
					Academic = message.Analysis.Academic,
					Formal = message.Analysis.Formal,
					Humorous = message.Analysis.Humorous,
					Informal = message.Analysis.Informal
				};
				messageDto.Analysis = analysisDto;
			}
			
			messageDtos.Add(messageDto);
		}
		
		var chatDto = new ChatDto
		{
			Id = chat.Id,
			Title = chat.Title,
			Type = chat.Type switch
			{
				ChatType.PaperAnalysis => "paperAnalysis",
				ChatType.Chat => "chat",
				_ => "chat"
			},
			Style = chat.Style switch
			{
				ChatStyle.Academic => "academic",
				ChatStyle.Formal => "formal",
				ChatStyle.Humorous => "humorous",
				ChatStyle.Informal => "informal",
				_ => "informal"
			},
			PaperTitle = chat.PaperTitle,
			Messages = messageDtos
		};
		
		return chatDto;
	}
	
}