del sa.server.db
dotnet ef migrations remove --context SimpleAuth.Server.DbContext --verbose
dotnet ef migrations add InitialCreate --context SimpleAuth.Server.DbContext --verbose
dotnet ef database update --context SimpleAuth.Server.DbContext --verbose
