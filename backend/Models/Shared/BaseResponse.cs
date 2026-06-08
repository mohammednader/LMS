namespace TrainingService.Models.Shared;

public class BaseResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string? ArMessage { get; set; }
    public long Id { get; set; }
}
