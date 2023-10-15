using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public string Slug => GenerateSlug();
    public required int YearOfRelease { get; set; }
    public required List<string> Genres { get; init; } = new();
    
    private string GenerateSlug()
    {
        var sluggedTitle = SlugRegex()
            .Replace(Title, "-")
            .ToLower();
        return $"{sluggedTitle}-{YearOfRelease}";
    }
    
    [GeneratedRegex("[^a-zA-Z0-9_-]", RegexOptions.NonBacktracking, 10)]
    private static partial Regex SlugRegex();
}

