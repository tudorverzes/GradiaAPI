using System.Text.Json.Serialization;

namespace boost_api.dto.pythonServerDto;

public class PythonRequest
{
	[JsonPropertyName("text")]
	public string Text { get; set; } = string.Empty;
	
	[JsonPropertyName("purpose")]
	public string Purpose { get; set; } = string.Empty;
	
	[JsonPropertyName("correct_grammar")]
	public bool CorrectGrammar { get; set; }
	
	[JsonPropertyName("max_tokens")]
	public int MaxTokens { get; set; }
	
	public override string ToString()
	{
		return $"Text: {Text}, Purpose: {Purpose}, CorrectGrammar: {CorrectGrammar}, MaxTokens: {MaxTokens}";
	}
}

public class PythonAnalysisResponse
{
	[JsonPropertyName("academic")]
	public double Academic { get; set; }
	
	[JsonPropertyName("formal")]
	public double Formal { get; set; }
	
	[JsonPropertyName("humorous")]
	public double Humorous { get; set; }
	
	[JsonPropertyName("informal")]
	public double Informal { get; set; }
	
	public override string ToString()
	{
		return $"Academic: {Academic}, Formal: {Formal}, Humorous: {Humorous}, Informal: {Informal}";
	}
}

public class PythonResponse
{
	[JsonPropertyName("style_analysis")]
	public PythonAnalysisResponse StyleAnalysis { get; set; } = new PythonAnalysisResponse();
	
	[JsonPropertyName("response")]
	public string Response { get; set; } = string.Empty;

	public override string ToString()
	{
		return $"StyleAnalysis: {StyleAnalysis}, Response: {Response}";
	}
}

