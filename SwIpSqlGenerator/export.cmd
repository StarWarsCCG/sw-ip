if exist swccg_db.sdb del /f /q /s swccg_db.sdb
if exist cards.sql del /f /q /s cards.sql
dotnet run ..\CardData cards.sql
type cards.sql | sqlite swccg_db.sdb
