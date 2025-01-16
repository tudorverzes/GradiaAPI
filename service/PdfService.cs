namespace boost_api.service;

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using UglyToad.PdfPig;

public class PdfService
{
	private readonly string _storagePath;

	public PdfService(string storagePath)
	{
		_storagePath = storagePath;
		Directory.CreateDirectory(_storagePath); // Ensure directory exists
	}

	public async Task<string> SaveFileAsync(IFormFile pdfFile)
	{
		if (pdfFile == null || pdfFile.Length == 0)
			throw new ArgumentException("Invalid PDF file");

		// Generate a unique file name
		var fileName = $"{Path.GetRandomFileName()}.pdf";
		var filePath = Path.Combine(_storagePath, fileName);

		// Save the file permanently
		await using var stream = new FileStream(filePath, FileMode.Create);
		await pdfFile.CopyToAsync(stream);

		return filePath;
	}

	public string ReadPdfText(string filePath)
	{
		if (!File.Exists(filePath))
			throw new FileNotFoundException("File not found", filePath);

		string text = string.Empty;

		// Read text from the PDF
		using (var pdf = PdfDocument.Open(filePath))
		{
			foreach (var page in pdf.GetPages())
			{
				text += page.Text;
			}
		}

		return text;
	}
}
