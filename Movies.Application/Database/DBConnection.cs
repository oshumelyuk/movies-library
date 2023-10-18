using System.Data;
using Npgsql;

namespace Movies.Application.Database;


public interface IDbConnectionFactory
{
    public Task<IDbConnection> CreateConnectionAsync();
}

public class NpgSqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection =  new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}