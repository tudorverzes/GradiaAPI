using System.ComponentModel.DataAnnotations.Schema;

namespace boost_api.model;

[Table("Analyses")]
public class Analysis
{
	public int Id { get; set; }
	
	public double Academic { get; set; }
	public double Formal { get; set; }
	public double Humorous { get; set; }
	public double Informal { get; set; }
}