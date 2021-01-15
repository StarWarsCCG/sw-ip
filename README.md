## Star Wars CCG Information Pool (sw-ip)

This repo exposes the card data in a way that more people are able to contribute.

The card data is now JSON, which makes it trivial for people to submit corrections and additions.

This repo also provides tools for converting the JSON into SQL compatible with the SQLite 2 format.

## Requirements

These tools are written in C#. Install the [.NET 5 SDK](https://dotnet.microsoft.com/download).

For working with SQLite, install SQLite 2. For Windows, [download SQLite 2](https://www.sqlite.org/sqlite-2_8_17.zip) and put `sqlite.exe` into the folder `SwIpSqlGenerator`.

### Dumping Card Data

This has already been done. If you just want to edit card data and/or produce a new `.sdb` file, skip to the next section.

If you find yourself needing to re-export the sw-ip data to JSON, use the `SwIpDumper` tool. You will need to install [SQLite 3](https://www.sqlite.org/download.html). Place the `swccg_db.sdb` file into the dumper tool's folder.

    sh dump.sh

The script `dump.sh` moves the data into a SQLite 3 database and runs `SwIpDumper`, which reads the new database and creates the JSON files in `/CardData`.

### Editing Card Data

Modify the JSON files in `/CardData`. When adding new cards, make sure each new card receives a unique `id`. Ensure that the JSON structure is always well-formed.

Be sure to use a text editor that supports UTF-8 (such as [Notepad++](https://notepad-plus-plus.org/)) and always save the JSON files as UTF-8. The SQL generator tool takes care of the conversion to ISO 8859-1, which is what the sw-ip application expects when reading the `.sdb` file.

### Creating the SDB File

The tool `SwIpSqlGenerator` converts the JSON files into SQL that can be consumed by SQLite 2, which is what sw-ip requires. Simply run the export script to handle all this for you.

For Windows, run `export.cmd`. Otherwise, run `export.sh`.

The script will generate the SQL (`cards.sql`) and use that to generate `swccg_db.sdb`. Place `swccg_db.sdb` into your sw-ip application folder, and you're done!
