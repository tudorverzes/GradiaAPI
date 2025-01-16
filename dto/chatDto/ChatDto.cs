using boost_api.model;

namespace boost_api.dto.chatDto;

public class ChatDto
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Style { get; set; } = string.Empty;
	public string? PaperTitle { get; set; }
	public List<MessageDto> Messages { get; set; } = [];
}

public class MessageDto
{
	public int Id { get; set; }
	public string Body { get; set; } = string.Empty;
	public DateTime Timestamp { get; set; }
	public bool IsSentByUser { get; set; }
	public AnalysisDto? Analysis { get; set; }
}

public class AnalysisDto
{
	public double Academic { get; set; }
	public double Formal { get; set; }
	public double Humorous { get; set; }
	public double Informal { get; set; } 
}