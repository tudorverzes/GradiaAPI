using System.ComponentModel.DataAnnotations.Schema;

namespace boost_api.model;

[Table("Messages")]
public class Message
{
	public int Id { get; set; }

	public string Body { get; set; } = "";
	public DateTime Timestamp { get; set; }
	public bool IsSentByUser { get; set; }
	
	public int? ChatId { get; set; }
	public int? AnalysisId { get; set; }
	public Analysis? Analysis { get; set; }
}