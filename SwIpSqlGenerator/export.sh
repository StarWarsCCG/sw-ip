dotnet run ../CardData cards.sql ../virtual-cards.md
rm -f swccg_db.sdb
cat cards.sql | sqlite swccg_db.sdb
