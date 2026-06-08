namespace TrainingService.Services;

public interface ICertificateService
{
    byte[] GenerateCertificate(int orgId, string studentName, string courseName, DateTime date, bool isValid = true);
}
