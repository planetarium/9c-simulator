namespace NineSimulator.Simulator;

using System;
using Microsoft.Data.Sqlite;

public class Store
{
    private const string CreateTableSql =
        "CREATE TABLE IF NOT EXISTS result (id INTEGER PRIMARY KEY AUTOINCREMENT, avatar1_address VARCHAR(42), avatar1_name INT, avatar1_cp INT, avatar1_win BOOL, avatar2_address VARCHAR(42), avatar2_name INT, avatar2_cp INT)";
    private const string CreateIndexSql = "CREATE INDEX IF NOT EXISTS idx_avatar_address ON result (avatar1_address);";
    private const string AddTxQuotaSql =
        "INSERT OR IGNORE INTO result (avatar1_address, avatar1_name, avatar1_cp, avatar1_win, avatar2_address, avatar2_name, avatar2_cp) VALUES (@Avatar1Address, @Avatar1Name, @Avatar1CP, @Avatar1Win, @Avatar2Address, @Avatar2Name, @Avatar2CP)";

    protected readonly string _connectionString;

    public Store(string connectionString)
    {
        _connectionString = connectionString;
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            ExecuteNonQuery(connection, CreateTableSql);
            ExecuteNonQuery(connection, CreateIndexSql);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void ExecuteNonQuery(SqliteConnection connection, string commandText)
    {
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }

    public void AddArenaResult(bool avatar1_win, string avatar1_address, string avatar1_name, int avatar1_cp, string avatar2_address, string avatar2_name, int avatar2_cp)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = AddTxQuotaSql;
        command.Parameters.AddWithValue("@Avatar1Address", avatar1_address);
        command.Parameters.AddWithValue("@Avatar1Name", avatar1_name);
        command.Parameters.AddWithValue("@Avatar1CP", avatar1_cp);
        command.Parameters.AddWithValue("@Avatar1Win", avatar1_win);
        command.Parameters.AddWithValue("@Avatar2Address", avatar2_address);
        command.Parameters.AddWithValue("@Avatar2Name", avatar2_name);
        command.Parameters.AddWithValue("@Avatar2CP", avatar2_cp);
        command.ExecuteNonQuery();
    }
}
