
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace GameStore.Desktop.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository()
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

    public async Task<UserProfile?> GetUserAsync(long userId)
    {
        const string sql = """
            SELECT user_id, username, email, created_at
            FROM users
            WHERE user_id = @userId;
            """;

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("userId", userId);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new UserProfile
        {
            UserId = reader.GetInt64(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            CreatedAt = reader.GetDateTime(3)
        };
    }
}

public class UserProfile
{
    public long UserId { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
