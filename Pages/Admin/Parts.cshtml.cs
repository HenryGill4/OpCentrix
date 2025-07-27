// Add this temporary debug method to identify the problematic column
private async Task DebugDatabaseSchema()
{
    using (var command = _context.Database.GetDbConnection().CreateCommand())
    {
        command.CommandText = "PRAGMA table_info(Parts)";
        await _context.Database.OpenConnectionAsync();
        
        using (var reader = await command.ExecuteReaderAsync())
        {
            int ordinal = 0;
            while (await reader.ReadAsync())
            {
                _logger.LogInformation($"Ordinal {ordinal}: Column {reader["name"]} - Type: {reader["type"]}, NotNull: {reader["notnull"]}, Default: {reader["dflt_value"]}");
                ordinal++;
            }
        }
    }
}