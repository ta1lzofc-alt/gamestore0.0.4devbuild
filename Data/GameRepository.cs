using GameStore.Desktop.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace GameStore.Desktop.Data;

public class GameRepository
{
    private readonly string _connectionString;

    public GameRepository()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Строка подключения не найдена.");
    }

    public async Task<List<Game>> GetGamesAsync(string search = "")
    {
        var games = new List<Game>();

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT
                g.game_id,
                g.title,
                g.description,
                g.price,
                COALESCE(
                    STRING_AGG(gen.name, ', ' ORDER BY gen.name),
                    'Без жанра'
                ) AS genres,
                COALESCE(g.cover_url, '') AS cover_url
            FROM games g
            LEFT JOIN game_genres gg
                ON gg.game_id = g.game_id
            LEFT JOIN genres gen
                ON gen.genre_id = gg.genre_id
            WHERE g.is_active = TRUE
              AND (@search = ''
                   OR g.title ILIKE '%' || @search || '%')
            GROUP BY
                g.game_id,
                g.title,
                g.description,
                g.price,
                g.cover_url
            ORDER BY g.created_at DESC;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("search", search.Trim());

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            games.Add(new Game
            {
                GameId = reader.GetInt64(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2)
                    ? ""
                    : reader.GetString(2),
                Price = reader.GetDecimal(3),
                Genres = reader.GetString(4),
                CoverUrl = reader.GetString(5)
            });
        }

        return games;
    }
}