namespace MarsAdvanced.Tests.Models;

public class ShareSkillData
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Subcategory { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public string ServiceType { get; set; } = string.Empty;

    public string LocationType { get; set; } = string.Empty;

    public string SkillTrade { get; set; } = string.Empty;

    public List<string> SkillExchangeTags { get; set; } = new();

    public string ActiveStatus { get; set; } = string.Empty;
}