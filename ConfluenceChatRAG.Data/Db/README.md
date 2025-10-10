# Chat History Database Setup

## Local Development (SQL Server LocalDB)

The connection string in `local.settings.json` uses SQL Server LocalDB which comes with Visual Studio.

```json
"ConnectionStrings:ChatHistory": "Server=(localdb)\\mssqllocaldb;Database=ChatHistory;Trusted_Connection=True;"
```

The database will be **automatically created** on first run using `EnsureCreated()`.

## Azure SQL Database (Production)

For production, update the connection string to point to Azure SQL:

```json
"ConnectionStrings:ChatHistory": "Server=tcp:yourserver.database.windows.net,1433;Database=ChatHistory;User ID=yourusername;Password=yourpassword;Encrypt=True;"
```

Or use Azure App Configuration / Key Vault to store the connection string securely.

## Database Schema

**Table: ChatMessages**
- Id (int, primary key)
- SessionId (string, indexed)
- Role (string) - "user" or "assistant"
- Content (string) - message text
- Timestamp (DateTimeOffset, indexed)
- SourcesJson (string, nullable) - JSON array of sources
- SuggestionsJson (string, nullable) - JSON array of suggestions

## Notes

- Messages are automatically trimmed to keep only the last 20 messages per session
- Sources and suggestions are stored as JSON for simplicity
- Uses `IDbContextFactory<T>` for efficient connection pooling in Azure Functions
