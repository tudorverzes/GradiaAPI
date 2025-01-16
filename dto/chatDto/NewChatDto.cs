using System.ComponentModel.DataAnnotations;

namespace boost_api.dto.chatDto;

public class NewChatDto
{
	[Required]
	[MinLength(1)]
	public string Text { get; set; } = string.Empty;

	[Required]
	[RegularExpression("academic|formal|humorous|informal")]
	public string Style { get; set; } = string.Empty;
}

public class NewPaperAnalysisDto
{
	[Required]
	public IFormFile? File { get; set; }
	
	[Required]
	[RegularExpression("academic|formal|humorous|informal")]
	public string Style { get; set; } = string.Empty;
}