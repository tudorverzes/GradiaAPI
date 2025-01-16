using boost_api.model;

namespace boost_api.repository;

public interface IChatRepository
{
	Task<List<Chat>> GetAllAsync(string appUserId);
	Task<Chat?> GetByIdAsync(int id);
	Task<Chat?> CreateAsync(Chat chat);
	Task<Chat?> UpdateAsync(Chat chat);
	Task<Chat?> AddMessageAsync(int chatId, Message message);
	Task<Chat?> AddMessagesAsync(int chatId, List<Message> messages);
	Task<Chat?> DeleteAsync(int id);
}