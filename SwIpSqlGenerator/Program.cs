using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwIpSqlGenerator
{
class Program
    {
        static string GetValue(JsonProperty property)
        {
            var value = property.Value;

            if (value.ValueKind == JsonValueKind.Null)
            {
                return "NULL";
            }
            else if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble().ToString();
            }
            else
            {
                var text = value.GetString().Replace("'", "''");
                return $"'{text}'";
            }
        }

        static async Task AppendAsync(StreamWriter writer, JsonDocument document)
        {
            foreach (var row in document.RootElement.EnumerateArray())
            {
                await writer.WriteAsync("INSERT INTO SWD (");
                await writer.WriteAsync(string.Join(",", row.EnumerateObject().Select(p => p.Name)));
                await writer.WriteAsync(") VALUES (");
                await writer.WriteAsync(string.Join(",", row.EnumerateObject().Select(GetValue)));
                await writer.WriteLineAsync(");");
            }
        }

        static async Task<JsonDocument> LoadDocumentAsync(string file)
        {
            await using (var stream = File.OpenRead(file))
                return await JsonDocument.ParseAsync(stream);
        }

        static async Task GenerateSqlAsync(string folder, string sqlFile)
        {
            var files = Directory.GetFiles(folder);
            
            using (var writer = File.CreateText(sqlFile))
            {
                await writer.WriteLineAsync("BEGIN TRANSACTION;");
                await writer.WriteLineAsync("CREATE TABLE Deck(ID INTEGER NOT NULL,Uniqueness char(6) ,ReserveDeck INTEGER ,DeckName char(100) ,Destiny char(4) ,SubType char(40) ,CardName char(80) NOT NULL,CardType char(18) NOT NULL,Expansion char(33) ,StartingCards INTEGER ,SideDeck INTEGER ,Rarity char(15) NOT NULL,Inventory int );");
                await writer.WriteLineAsync("CREATE TABLE SQLITEADMIN_QUERIES(ID INTEGER PRIMARY KEY,NAME VARCHAR(100),SQL TEXT);");
                await writer.WriteLineAsync("INSERT INTO SQLITEADMIN_QUERIES VALUES(1,'grr','select Cardname from swd where id = ''1999''; insert into SWDTEMP (CardName)');");
                await writer.WriteLineAsync("CREATE TABLE SWD (id int, CardName char(80), Grouping char(6), CardType char(18), Subtype char(40), ModelType char(40), Expansion char(33), Rarity char(15), Uniqueness char(6), Characteristics char(60), Destiny char(4), Power char(4), Ferocity char(4), CreatureDefenseValue char(4), CreatureDefenseValueName char(20), ObjectiveFront text, ObjectiveBack text, ObjectiveFrontName char(80), ObjectiveBackName char(80), Deploy char(4), Forfeit char(4), Armor char(5), Ability char(4), Hyperspeed char(4), Landspeed char(4), Politics char(4), Maneuver char(4), ForceAptitude char(20), Lore text, Gametext text, JediTestNumber char(4), LightSideIcons char(4), DarkSideIcons char(4), LightSideText text, DarkSideText text, Parsec char(4), Icons char(60), Planet char(4), Space char(4), Mobile char(4), Interior char(4), Exterior char(4), Underground char(4), Creature char(4), Vehicle char(4), Starship char(4), Underwater char(4), Pilot char(4), Warrior char(4), Astromech char(4), PermanentWeapon char(4), SelectiveCreature char(4), Independent char(4), ScompLink char(4), Droid char(4), TradeFederation char(4), Republic char(4), Episode1 char(4), Information text, Abbreviation char(50), Pulls text, IsPulled text, Counterpart char(50), Combo text, Matching char(50), MatchingWeapon char(50), Rules text, Cancels text, IsCanceledBy text, Inventory int, Needs int, ExpansionV VARCHAR(40), Influence char(4), Grabber char(4), Errata char(4), CardNameV char(80), UniquenessV char(6));");
                await writer.WriteLineAsync("CREATE TABLE SWDTEMP(id int not NULL,CardName char(80) not NULL,Grouping char(6) not NULL,CardType char(18) not NULL,Subtype char(40) ,ModelType char(40) ,Expansion char(33) ,Rarity char(15) not NULL,Uniqueness char(6) ,Characteristics char(60) ,Destiny char(4) ,Power char(4) ,Ferocity char(4) ,CreatureDefenseValue char(4) ,CreatureDefenseValueName char(20) ,ObjectiveFront text ,ObjectiveBack text ,ObjectiveFrontName char(80) ,ObjectiveBackName char(80) ,Deploy char(4) ,Forfeit char(4) ,Armor char(5) ,Ability char(4) ,Hyperspeed char(4) ,Landspeed char(4) ,Politics char(4) ,Maneuver char(4) ,ForceAptitude char(20) ,Lore text ,Gametext text ,JediTestNumber char(4) ,LightSideIcons char(4) ,DarkSideIcons char(4) ,LightSideText text ,DarkSideText text ,Parsec char(4) ,Icons char (60),Planet char(4) ,Space char(4) ,Mobile char(4) ,Interior char(4) ,Exterior char(4) ,Underground char(4) ,Creature char(4) ,Vehicle char(4) ,Starship char(4) ,Underwater char(4) ,Pilot char(4) ,Warrior char(4) ,Astromech char(4) ,PermanentWeapon char(4) ,SelectiveCreature char(4) ,Independent char(4) ,ScompLink char(4) ,Droid char(4) ,TradeFederation char(4) ,Republic char(4) ,Episode1 char(4) ,Information text ,Abbreviation char(50) ,Pulls text ,IsPulled text ,Counterpart char(50) ,Combo text ,Matching char(50) ,MatchingWeapon char(50) ,Rules text ,Cancels text ,IsCanceledBy text ,Inventory int ,Needs int ,ExpansionV VARCHAR(40) ,Influence char(4) ,Grabber char(4) ,Errata char(4) ,CardNameV char(80) ,UniquenessV char(6) );");
                await writer.WriteLineAsync("CREATE TABLE VERSION(Version VARCHAR(10) );");
                await writer.WriteLineAsync("INSERT INTO VERSION VALUES(1.7);");

                foreach (var file in files)
                {   
                    Console.WriteLine("Generating SQL for " + file);
                    using (var document = await LoadDocumentAsync(file))
                    {
                        foreach (var row in document.RootElement.EnumerateArray())
                        {
                            await writer.WriteAsync("INSERT INTO SWD (");
                            await writer.WriteAsync(string.Join(",", row.EnumerateObject().Select(p => p.Name)));
                            await writer.WriteAsync(") VALUES (");
                            await writer.WriteAsync(string.Join(",", row.EnumerateObject().Select(GetValue)));
                            await writer.WriteLineAsync(");");
                        }
                    }
                }
                await writer.WriteLineAsync("COMMIT;");
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                await GenerateSqlAsync(args[0], args[1]);
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
