using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();

        var result = await dbConnection.ExecuteAsync(
            new CommandDefinition("""
                                  insert into movies (id, slug, title, yearofrelease)
                                  values (@Id, @Slug, @Title, @YearOfRelease)
                                  """, movie));
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await dbConnection.ExecuteAsync(
                    new CommandDefinition("""
                                          insert into genres (movieId, name)
                                          values (@MovieId, @Name)
                                          """, new {MovieId = movie.Id, Name = genre}));
            }
        }
        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await dbConnection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT * from movies where id = @id
                                  """, new { id}));
        if (movie is null)
            return null;

        var genres = await dbConnection.QueryAsync<string>(
            new CommandDefinition("""
                                  select name from genres where movieid = @id
                                  """, new {id}));
        movie.Genres.AddRange(genres);
        
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await dbConnection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT * from movies where slug = @slug
                                  """, new {slug}));
        if (movie is null)
            return null;

        var genres = await dbConnection.QueryAsync<string>(
            new CommandDefinition("""
                                  select name from genres where movieid = @id
                                  """, new {id = movie.Id}));
        movie.Genres.AddRange(genres);
        
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        var result = await dbConnection.QueryAsync(
            new CommandDefinition("""
                                  SELECT m.*, string_agg(g.name, ',') as genres 
                                  from movies m left join genres g on m.id = g.movieid 
                                  group by id
                                  """));
        return result.Select(m => new Movie
        {
            Id = m.id,
            Title = m.title,
            YearOfRelease = m.yearofrelease,
            Genres = Enumerable.ToList(m.genres.Split(','))
        });
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();

        await dbConnection.ExecuteAsync(
            new CommandDefinition("""
                                  delete from genres where movieid = @id
                                  """, new {id }));
        
        var itemsDeleted = await dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition("""
                                  delete from movies where id = @id
                                  """, new {id }));
        
        transaction.Commit();
        return itemsDeleted > 0;
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = dbConnection.BeginTransaction();

        await dbConnection.ExecuteAsync(
            new CommandDefinition("""
                                  delete from genres where movieid = @id
                                  """, new {id = movie.Id}));

        foreach (var genre in movie.Genres)
        {
            await dbConnection.ExecuteAsync(
                new CommandDefinition("""
                                      insert into genres(movieid, name) 
                                      values (@movieId, @name)
                                      """, new {movieId = movie.Id, name = genre}));
        }
        
        var result = await dbConnection.ExecuteAsync(
            new CommandDefinition("""
                                  update movies 
                                  set slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                                  where id = @Id
                                  """, movie));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var dbConnection = await _dbConnectionFactory.CreateConnectionAsync();
        return await dbConnection.ExecuteScalarAsync<bool>(
            new CommandDefinition("""
                                  SELECT count(1) from movies where id = @id
                                  """, new {id}));
    }
}