using Microsoft.Extensions.Configuration;
using Npgsql;
using GameStore.Desktop.Models;

namespace GameStore.Desktop.Data;

public class OrderRepository
{
    private readonly string _connectionString;

    public OrderRepository()
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

    public async Task<long> CreateOrderAsync(long userId, List<Game> games)
    {
        if (games.Count == 0)
            throw new InvalidOperationException("Корзина пуста.");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            decimal total = games.Sum(g => g.Price);

            const string orderSql = """
                INSERT INTO orders (user_id, total, created_at)
                VALUES (@userId, @total, CURRENT_TIMESTAMP)
                RETURNING order_id;
                """;

            long orderId;

            await using (var command = new NpgsqlCommand(
                orderSql, connection, transaction))
            {
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("total", total);

                orderId = (long)(await command.ExecuteScalarAsync())!;
            }

            const string itemSql = """
                INSERT INTO order_items (order_id, game_id, price)
                VALUES (@orderId, @gameId, @price);
                """;

            const string librarySql = """
                INSERT INTO user_library (user_id, game_id, purchased_at)
                VALUES (@userId, @gameId, CURRENT_TIMESTAMP)
                ON CONFLICT (user_id, game_id) DO NOTHING;
                """;

            foreach (var game in games)
            {
                await using (var itemCommand = new NpgsqlCommand(
                    itemSql, connection, transaction))
                {
                    itemCommand.Parameters.AddWithValue("orderId", orderId);
                    itemCommand.Parameters.AddWithValue("gameId", game.GameId);
                    itemCommand.Parameters.AddWithValue("price", game.Price);

                    await itemCommand.ExecuteNonQueryAsync();
                }

                await using (var libraryCommand = new NpgsqlCommand(
                    librarySql, connection, transaction))
                {
                    libraryCommand.Parameters.AddWithValue("userId", userId);
                    libraryCommand.Parameters.AddWithValue("gameId", game.GameId);

                    await libraryCommand.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();

            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}