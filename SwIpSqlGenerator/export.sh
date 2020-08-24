dotnet run ../CardData cards.sql
rm -f swccg_db.sdb
cat cards.sql | sqlite swccg_db.sdb
