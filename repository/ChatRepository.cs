using boost_api.data;
using boost_api.model;
using Microsoft.EntityFrameworkCore;

namespace boost_api.repository;

public class ChatRepository : IChatRepository
{
	private readonly ApplicationDbContext _context;

	public ChatRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<List<Chat>> GetAllAsync(string appUserId)
	{
		return await _context.Chats
			.Where(c => c.AppUserId == appUserId)
			.ToListAsync();
	}

	public async Task<Chat?> GetByIdAsync(int id)
	{
		return await _context.Chats
			.Include(c => c.Messages)
				.ThenInclude(m => m.Analysis)
			.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<Chat?> CreateAsync(Chat chat)
	{
		await _context.Chats.AddAsync(chat);
		await _context.SaveChangesAsync();
		return chat;
	}

	public async Task<Chat?> UpdateAsync(Chat chat)
	{
		var existingChat = await _context.Chats.FindAsync(chat.Id);
		if (existingChat == null)
		{
			return null;
		}

		existingChat.Title = chat.Title;
		await _context.SaveChangesAsync();
		return existingChat;
	}

	public async Task<Chat?> AddMessageAsync(int chatId, Message message)
	{
		var chat = await _context.Chats
			.Include(c => c.Messages)
			.FirstOrDefaultAsync(c => c.Id == chatId);
		if (chat == null)
		{
			return null;
		}

		chat.Messages.Add(message);
		await _context.SaveChangesAsync();
		return chat;
	}

	public async Task<Chat?> AddMessagesAsync(int chatId, List<Message> messages)
	{
		var chat = await _context.Chats
			.Include(c => c.Messages)
			.FirstOrDefaultAsync(c => c.Id == chatId);
		if (chat == null)
		{
			return null;
		}

		chat.Messages.AddRange(messages);
		chat.LastMessageTimestamp = messages.Last().Timestamp;
		await _context.SaveChangesAsync();
		return chat;
	}

	public async Task<Chat?> DeleteAsync(int id)
	{
		var chat = await _context.Chats
			.Include(c => c.Messages)
			.FirstOrDefaultAsync(c => c.Id == id);
		if (chat == null)
		{
			return null;
		}

		_context.Chats.Remove(chat);
		await _context.SaveChangesAsync();
		return chat;
	}
}