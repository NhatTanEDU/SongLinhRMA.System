namespace RMA.Shared.DTOs;

public class OcrRequestDto
{
    public string Base64Image { get; set; } = string.Empty;
}

public class OcrResponseDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
}
