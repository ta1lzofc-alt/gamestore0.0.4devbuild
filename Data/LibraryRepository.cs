using Npgsql;
using GameStore.Desktop.Models;
using Microsoft.Extensions.Configuration;

namespace GameStore.Desktop.Data;

public class LibraryRepository
{
    private readonly string _connectionString;

    public LibraryRepository()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _connectionString =
            config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Строка подключения не найдена.");
    }

    public async Task<List<Game>> GetUserGamesAsync(long userId)
    {
        const string sql = """
            SELECT
                g.game_id,
                g.title,
                COALESCE(g.description, ''),
                g.price,
                COALESCE(
                    STRING_AGG(gen.name, ', ' ORDER BY gen.name),
                    'Без жанра'
                ) AS genres,
                COALESCE(g.cover_url, '') AS cover_url
            FROM user_library ul
            JOIN games g ON g.game_id = ul.game_id
            LEFT JOIN game_genres gg ON gg.game_id = g.game_id
            LEFT JOIN genres gen ON gen.genre_id = gg.genre_id
            WHERE ul.user_id = @userId
            GROUP BY
                g.game_id,
                g.title,
                g.description,
                g.price,
                g.cover_url
            ORDER BY g.title;
            """;

        var games = new List<Game>();

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            games.Add(new Game
            {
                GameId = reader.GetInt64(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDecimal(3),
                Genres = reader.GetString(4),
                CoverUrl = reader.GetString(5)
            });
        }

        return games;
    }
}