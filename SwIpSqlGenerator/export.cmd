dotnet run ..\CardData cards.sql
del /F swccg_db.sdb
type cards.sql | sqlite swccg_db.sdb
