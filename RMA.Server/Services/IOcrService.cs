namespace RMA.Server.Services;

public interface IOcrService
{
    Task<string> ScanSerialNumberAsync(string base64Image);
}
