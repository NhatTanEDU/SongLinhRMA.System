using Google.Cloud.Vision.V1;
using System.Text.RegularExpressions;

namespace RMA.Server.Services;

public class GoogleVisionOcrService : IOcrService
{
    private readonly ILogger<GoogleVisionOcrService> _logger;

    public GoogleVisionOcrService(ILogger<GoogleVisionOcrService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ScanSerialNumberAsync(string base64Image)
    {
        try
        {
            // Xóa header data:image/jpeg;base64, nếu có
            if (base64Image.Contains(","))
            {
                base64Image = base64Image.Split(',')[1];
            }

            var client = await ImageAnnotatorClient.CreateAsync();
            var image = Image.FromBytes(Convert.FromBase64String(base64Image));
            
            var response = await client.DetectTextAsync(image);
            if (response == null || !response.Any())
            {
                _logger.LogWarning("Google Vision API returned no text.");
                return string.Empty;
            }

            var rawText = response[0].Description;
            _logger.LogInformation("OCR Raw Text Length: {len}", rawText.Length);

            // Bóc tách S/N
            var match = Regex.Match(rawText, @"(?:S\/N|SN|Serial(?: Number)?)\s*[:\-]?\s*([A-Za-z0-9\-]+)", RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Fallback: Tìm một chuỗi hoa và số dài, thường là Serial Number
            var fallbackMatch = Regex.Match(rawText, @"\b[A-Z0-9\-]{6,20}\b");
            if (fallbackMatch.Success)
            {
                 return fallbackMatch.Value;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Vision API");
            throw;
        }
    }
}
