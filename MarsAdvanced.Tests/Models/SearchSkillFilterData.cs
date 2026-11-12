using MarsAdvanced.Tests.Models;

public class SearchSkillFilterData
{
    public string Keyword { get; set; } = string.Empty;

    public ShareSkillData MatchingListing { get; set; } = new();

    public ShareSkillData NonMatchingListing { get; set; } = new();
}