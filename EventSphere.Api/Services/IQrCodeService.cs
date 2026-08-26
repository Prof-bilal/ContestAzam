namespace EventSphere.Api.Services;

public interface IQrCodeService
{
    string GenerateQrCodeBase64(string content);
}
