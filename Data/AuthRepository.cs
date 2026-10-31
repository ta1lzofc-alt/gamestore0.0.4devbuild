
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace GameStore.Desktop.Data;

public class AuthRepository
{
    private readonly string _connectionString;

    public AuthRepository()
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

    public async Task<bool> RegisterAsync(
        string username,
        string email,
        string password)
    {
        username = username.Trim();
        email = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("Введите логин.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Введите email.");

        if (password.Length < 8)
            throw new InvalidOperationException(
                "Пароль должен содержать минимум 8 символов.");

        string passwordHash =
            BCrypt.Net.BCrypt.HashPassword(password);

        const string sql = """
            INSERT INTO users
                (username, email, password_hash, created_at)
            VALUES
                (@username, @email, @passwordHash, CURRENT_TIMESTAMP)
            RETURNING user_id;
            """;

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("email", email);
        command.Parameters.AddWithValue("passwordHash", passwordHash);

        try
        {
            await command.ExecuteScalarAsync();
            return true;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new InvalidOperationException(
                "Такой логин или email уже занят.");
        }
    }

    public async Task<long?> LoginAsync(
        string login,
        string password)
    {
        login = login.Trim();

        const string sql = """
            SELECT user_id, password_hash
            FROM users
            WHERE LOWER(username) = LOWER(@login)
               OR LOWER(email) = LOWER(@login)
            LIMIT 1;
            """;

        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("login", login);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        long userId = reader.GetInt64(0);
        string passwordHash = reader.GetString(1);

        bool passwordValid;

        try
        {
            passwordValid =
                BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            passwordValid = false;
        }

        return passwordValid ? userId : null;
    }
}
