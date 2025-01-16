using System.ComponentModel.DataAnnotations;

namespace boost_api.dto.chatDto;

public class MessageNewDto
{
	[Required]
	public string Body { get; set; } = string.Empty;
}