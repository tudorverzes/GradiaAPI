namespace boost_api.dto.chatDto;

public class ChatShortDto
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public DateTime Timestamp { get; set; }
}