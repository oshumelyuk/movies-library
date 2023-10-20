using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetBySlugAsync(string slug);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<bool> DeleteByIdAsync(Guid id);
    Task<Movie?> UpdateAsync(Movie movie);
}