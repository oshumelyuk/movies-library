using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mappings;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController: ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie);
        return CreatedAtAction(nameof(Get), new {idOrSlug = movie.Id}, movie);
    }
    
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody]UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var updatedMovie =  await _movieService.UpdateAsync(movie);
        if (updatedMovie is null)
            return NotFound();
        var response = movie.MapToResponse();
        return Ok(response);
    }
    
    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetByIdAsync(id)
            : await _movieService.GetBySlugAsync(idOrSlug);
        if (movie is null) return NotFound(idOrSlug);
        
        var movieResponse = movie.MapToResponse();
        return Ok(movieResponse);
    }
    
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieService.GetAllAsync();

        var moviesResponse = movies.MapToResponse();
        return Ok(moviesResponse);
    }
    
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _movieService.DeleteByIdAsync(id);
        if (!deleted) return NotFound(id);
        
        return Ok();
    }
}