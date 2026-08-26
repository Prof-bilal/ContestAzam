using QRCoder;

namespace EventSphere.Api.Services;

public class QrCodeService : IQrCodeService
{
    public string GenerateQrCodeBase64(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var pngByteQrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = pngByteQrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
