using System.ComponentModel.DataAnnotations.Schema;

namespace boost_api.model;

public enum ChatType
{
	PaperAnalysis,
	Chat,
}

public enum ChatStyle
{
	Academic,
	Formal,
	Humorous,
	Informal,
}

[Table("Chats")]
public class Chat
{
	public int Id { get; set; }
	
	public string Title { get; set; } = "";
	public ChatType Type { get; set; }
	public ChatStyle Style { get; set; }
	public DateTime LastMessageTimestamp { get; set; }
	
	public string? PaperPath { get; set; }
	public string? PaperTitle { get; set; }
	
	public List<Message> Messages { get; set; } = [];
	
	public string AppUserId { get; set; }
}