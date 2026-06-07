using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.RegularExpressions;
using ZXing;
using ZXing.ImageSharp;
using Microsoft.Extensions.Configuration;

namespace RMA.Server.Services;

public class BarcodeAndOcrService : IOcrService
{
    private readonly TesseractOcrService _tesseractOcr;
    private readonly GoogleVisionOcrService _googleVisionOcr;
    private readonly ILogger<BarcodeAndOcrService> _logger;
    private readonly IConfiguration _configuration;

    public BarcodeAndOcrService(
        TesseractOcrService tesseractOcr,
        GoogleVisionOcrService googleVisionOcr,
        ILogger<BarcodeAndOcrService> logger,
        IConfiguration configuration)
    {
        _tesseractOcr = tesseractOcr;
        _googleVisionOcr = googleVisionOcr;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> ScanSerialNumberAsync(string base64Image)
    {
        try
        {
            _logger.LogInformation("Starting 3-Tier Fallback Barcode & OCR Scan...");

            // 1. Clean base64 header if present
            string cleanBase64 = base64Image;
            if (cleanBase64.Contains(","))
            {
                cleanBase64 = cleanBase64.Split(',')[1];
            }

            byte[] imageBytes = Convert.FromBase64String(cleanBase64);

            // ==========================================
            // TIER 1: Quét mã vạch (Barcode/QR) cục bộ
            // ==========================================
            _logger.LogInformation("[TIER 1] Scanning for barcodes locally...");
            string? barcodeResult = TryScanBarcode(imageBytes);
            if (!string.IsNullOrEmpty(barcodeResult))
            {
                _logger.LogInformation("[TIER 1 SUCCESS] Barcode/QR code detected: {sn}", barcodeResult);
                return barcodeResult;
            }
            _logger.LogInformation("[TIER 1 FAILED] No barcode/QR found.");

            // =========================================================================
            // GHI CHÚ BẢO TRÌ / THAY ĐỔI THỨ TỰ ƯU TIÊN:
            // - Mặc định hiện tại: TIER 2 (Tesseract local) -> TIER 3 (Google Vision API)
            // - Để đưa Google Vision lên ưu tiên trước Tesseract:
            //   Chỉ cần hoán đổi vị trí chạy code của TIER 2 và TIER 3 dưới đây.
            // =========================================================================

            // ==========================================
            // TIER 2: Quét chữ bằng Tesseract cục bộ
            // ==========================================
            _logger.LogInformation("[TIER 2] Scanning text via local Tesseract OCR...");
            string tesseractResult = string.Empty;
            try
            {
                tesseractResult = await _tesseractOcr.ScanSerialNumberAsync(base64Image);
                if (!string.IsNullOrEmpty(tesseractResult))
                {
                    _logger.LogInformation("[TIER 2 SUCCESS] Serial Number extracted via Tesseract: {sn}", tesseractResult);
                    return tesseractResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TIER 2 FAILED] Tesseract OCR failed (possibly due to missing eng.traineddata file or native library). Fallback to next tier...");
            }

            // ==========================================
            // TIER 3: Quét chữ bằng Google Cloud Vision API
            // ==========================================
            bool enableGoogleVision = _configuration.GetValue<bool>("Ocr:EnableGoogleVision", true);
            if (enableGoogleVision)
            {
                _logger.LogInformation("[TIER 3] Scanning text via Google Cloud Vision API...");
                try
                {
                    string googleOcrResult = await _googleVisionOcr.ScanSerialNumberAsync(base64Image);
                    if (!string.IsNullOrEmpty(googleOcrResult))
                    {
                        _logger.LogInformation("[TIER 3 SUCCESS] Serial Number extracted via Google Vision: {sn}", googleOcrResult);
                        return googleOcrResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TIER 3 FAILED] Google Cloud Vision API failed.");
                    throw;
                }
            }
            else
            {
                _logger.LogInformation("[TIER 3 SKIPPED] Google Cloud Vision API is disabled by configuration.");
            }

            _logger.LogWarning("All 3-Tier scanning methods failed to detect a serial number.");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error occurred in BarcodeAndOcrService.");
            throw;
        }
    }

    private string? TryScanBarcode(byte[] imageBytes)
    {
        try
        {
            using var ms = new MemoryStream(imageBytes);
            using var image = Image.Load<Rgb24>(ms);

            // Cấu hình trình đọc ZXing sử dụng ImageSharp binding
            var reader = new ZXing.ImageSharp.BarcodeReader<Rgb24>
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E,
                        BarcodeFormat.ITF
                    }
                }
            };

            var result = reader.Decode(image);
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                string text = result.Text.Trim();
                
                // Trích xuất S/N từ nội dung mã vạch nếu có định dạng S/N:...
                var match = Regex.Match(text, @"(?:S\/N|SN|Serial(?: Number)?)\s*[:\-]?\s*([A-Za-z0-9\-]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                
                var userMatch = Regex.Match(text, @"S\/N[:\s]+([A-Z0-9]+)", RegexOptions.IgnoreCase);
                if (userMatch.Success)
                {
                    return userMatch.Groups[1].Value;
                }

                // Nếu là mã vạch chứa trực tiếp giá trị S/N thô, trả về trực tiếp
                return text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local barcode scan failed or image format was unreadable.");
        }

        return null;
    }
}
