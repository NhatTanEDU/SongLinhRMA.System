using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMA.Server.Services;
using RMA.Shared.DTOs;

namespace RMA.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;

    public OcrController(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] OcrRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Base64Image))
        {
            return BadRequest(new { message = "Image is required." });
        }

        try
        {
            var sn = await _ocrService.ScanSerialNumberAsync(request.Base64Image);
            return Ok(new OcrResponseDto { SerialNumber = sn, RawText = "" });
        }
        catch (Grpc.Core.RpcException rpcEx) when (rpcEx.Status.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
        {
            return StatusCode(403, new { message = "Google Cloud Vision API chưa được kích hoạt cho Project này. Vui lòng truy cập đường dẫn sau để bật và thử lại: " + rpcEx.Status.Detail });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi OCR: {ex.Message}" });
        }
    }
}
