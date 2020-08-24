sqlite swccg_db.sdb .dump > raw.sql
iconv -f ISO8859-1 -t UTF-8 raw.sql -o utf8.sql
rm swccg_db.sqlite
cat utf8.sql | sqlite3 swccg_db.sqlite
