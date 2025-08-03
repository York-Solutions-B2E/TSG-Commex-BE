namespace TSG_Commex_BE.DTOs.Request;

public class GetCommunicationRequest
{
    public string? StatusFilter { get; set; }
    public string? TypeFilter { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

}