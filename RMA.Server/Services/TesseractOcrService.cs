using Tesseract;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RMA.Server.Services;

public class TesseractOcrService : IOcrService
{
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(ILogger<TesseractOcrService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ScanSerialNumberAsync(string base64Image)
    {
        try
        {
            _logger.LogInformation("Starting local Tesseract OCR scan...");

            // 1. Resolve tessdata folder
            string? tessdataPath = ResolveTessdataPath();
            if (string.IsNullOrEmpty(tessdataPath))
            {
                _logger.LogError("tessdata directory or 'eng.traineddata' file is missing.");
                throw new FileNotFoundException("Không tìm thấy tệp 'eng.traineddata' trong thư mục 'tessdata'. Vui lòng tải về và đặt vào dự án.");
            }

            _logger.LogInformation("Using tessdata path: {path}", tessdataPath);

            // 2. Decode image bytes
            string cleanBase64 = base64Image;
            if (cleanBase64.Contains(","))
            {
                cleanBase64 = cleanBase64.Split(',')[1];
            }
            byte[] imageBytes = Convert.FromBase64String(cleanBase64);

            // 3. Run Tesseract engine
            using var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);

            string rawText = page.GetText();
            if (string.IsNullOrWhiteSpace(rawText))
            {
                _logger.LogWarning("Tesseract OCR returned empty text.");
                return string.Empty;
            }

            _logger.LogInformation("Tesseract Raw Text: \n{text}", rawText);

            // 4. Extract Serial Number using Regex
            // Pattern 1: Regular S/N parsing
            var match = Regex.Match(rawText, @"(?:S\/N|SN|Serial(?: Number)?)\s*[:\-]?\s*([A-Za-z0-9\-]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Pattern 2: Specific Regex requested by user: @"S\/N[:\s]+([A-Z0-9]+)"
            var userMatch = Regex.Match(rawText, @"S\/N[:\s]+([A-Z0-9]+)", RegexOptions.IgnoreCase);
            if (userMatch.Success)
            {
                return userMatch.Groups[1].Value.Trim();
            }

            // Fallback: Find a string of numbers/letters 6 to 20 chars long, typical for S/Ns
            var fallbackMatch = Regex.Match(rawText, @"\b[A-Z0-9\-]{6,20}\b");
            if (fallbackMatch.Success)
            {
                 return fallbackMatch.Value;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Tesseract OCR scan.");
            throw;
        }
    }

    private string? ResolveTessdataPath()
    {
        var pathsToTry = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
            Path.Combine(Directory.GetCurrentDirectory(), "tessdata"),
            Path.Combine(Directory.GetCurrentDirectory(), "RMA.Server", "tessdata")
        };

        foreach (var path in pathsToTry)
        {
            if (Directory.Exists(path) && File.Exists(Path.Combine(path, "eng.traineddata")))
            {
                return path;
            }
        }

        // Fallback to first path if directories exist but empty, to let Tesseract's own engine throw a clear error
        foreach (var path in pathsToTry)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
