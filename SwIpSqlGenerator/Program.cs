using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SwIpSqlGenerator
{
    static class Extensions
    {
        public static string TrimEnd(this string text, string suffix)
        {
            if (text.EndsWith(suffix))
                return text.Substring(0, text.Length - suffix.Length);
            else
                return text;
        }
    }

    class Program
    {
        const string Uuid = "Uuid";
        const string GempId = "GempId";

        static readonly ImmutableDictionary<string, string> GempExpansions = new Dictionary<string, string>
        {
            ["A New Hope"] = "ANewHope",
            ["Cloud City"] = "CloudCity",
            ["Coruscant"] = "Coruscant",
            ["Dagobah"] = "Dagobah",
            ["Death Star II"] = "DeathStarII",
            ["Demonstration Deck Premium Card Set"] = "Demonstration Deck Premium Card Set",
            ["Endor"] = "Endor",
            ["Enhanced Cloud City"] = "EnhancedCloudCity",
            ["Enhanced Jabba's Palace"] = "EnhancedJabbasPalace",
            ["Enhanced Premiere Pack"] = "EnhancedPremiere",
            ["Hoth"] = "Hoth",
            ["Hoth 2 Player"] = "EmpireStrikesBackIntroductoryTwoPlayerGame",
            ["Jabba's Palace"] = "JabbasPalace",
            ["Jabba's Palace Sealed Deck"] = "JabbasPalaceSealedDeck",
            ["Jedi Pack"] = "JediPack",
            ["Official Tournament Sealed Deck"] = "OfficialTournamentSealedDeck",
            ["Premiere"] = "Premiere",
            ["Premiere 2 Player"] = "PremiereIntroductoryTwoPlayerGame",
            ["Rebel Leader Cards"] = "RebelLeader",
            ["Reflections II"] = "ReflectionsII",
            ["Reflections III"] = "ReflectionsIII",
            ["Special Edition"] = "SpecialEdition",
            ["Tatooine"] = "Tatooine",
            ["Theed Palace"] = "TheedPalace",
            ["Third Anthology"] = "ThirdAnthology",
            ["Virtual Card Set #0"] = "Virtual0",
            ["Virtual Card Set #1"] = "Virtual1",
            ["Virtual Card Set #2"] = "Virtual2",
            ["Virtual Card Set #3"] = "Virtual3",
            ["Virtual Card Set #4"] = "Virtual4",
            ["Virtual Card Set #5"] = "Virtual5",
            ["Virtual Card Set #6"] = "Virtual6",
            ["Virtual Card Set #7"] = "Virtual7",
            ["Virtual Card Set #8"] = "Virtual8",
            ["Virtual Card Set #9"] = "Virtual9",
            ["Virtual Card Set #10"] = "Virtual10",
            ["Virtual Card Set #11"] = "Virtual11",
            ["Virtual Card Set #12"] = "Virtual12",
            ["Virtual Defensive Shields"] = "Virtual Defensive Shields",
            ["Virtual Premium Set"] = "VirtualPremium"
        }.ToImmutableDictionary();

        static string GetValue(JsonElement value)
        {
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
                var text = value
                    .GetString()
                    .Replace("'", "''");
                
                return $"'{text}'";
            }
        }

        static async Task<JsonDocument> LoadDocumentAsync(string file)
        {
            await using (var stream = File.OpenRead(file))
                return await JsonDocument.ParseAsync(stream);
        }

        static async Task<T> DeserializeFileAsync<T>(
            string file,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            T result;

            await using (var fileStream = File.OpenRead(file))
            {
                result = await JsonSerializer.DeserializeAsync<T>(
                    fileStream,
                    options,
                    cancellationToken);
            }

            return result;
        }

        static string CleanCardName(string cardName)
        {
            if (cardName.EndsWith(')'))
            {
                return cardName.Substring(0, cardName.LastIndexOf('('));
            }
            else
            {
                return cardName;
            }
            // return cardName
            //     .TrimEnd(" (V)")
            //     .TrimEnd(" (EP1)")
            //     .TrimEnd(" (Frozen)")
            //     .TrimEnd(" (1st Marker)")
            //     .TrimEnd(" (2nd Marker)")
            //     .TrimEnd(" (3rd Marker)")
            //     .TrimEnd(" (4th Marker)")
            //     .TrimEnd(" (5th Marker)")
            //     .TrimEnd(" (6th Marker)")
            //     .TrimEnd(" (7th Marker)")
            //     .TrimEnd(" (8th Marker)");
        }

        static async Task GenerateSqlAsync(string folder, string sqlFile)
        {
            var files = Directory.GetFiles(folder);
            var ids = new HashSet<int>();
            var duplicateIds = new HashSet<int>();
            int minId = int.MaxValue;
            int maxId = int.MinValue;

            var uuids = await DeserializeFileAsync<List<Guid>>("uuid.json");
            var rawGempData = await DeserializeFileAsync<Dictionary<string, string>>("gemp.json");
            var gempData = rawGempData.ToDictionary(p => p.Value, p => p.Key);

            // https://codingrigour.wordpress.com/2011/02/17/the-case-of-the-mysterious-characters/
            
            using (var stream = File.Create(sqlFile))
            using (var writer = new StreamWriter(stream, Encoding.GetEncoding("ISO-8859-1")))
            {
                await writer.WriteLineAsync("BEGIN TRANSACTION;");
                await writer.WriteLineAsync("CREATE TABLE Deck(ID INTEGER NOT NULL,Uniqueness char(6) ,ReserveDeck INTEGER ,DeckName char(100) ,Destiny char(4) ,SubType char(40) ,CardName char(80) NOT NULL,CardType char(18) NOT NULL,Expansion char(33) ,StartingCards INTEGER ,SideDeck INTEGER ,Rarity char(15) NOT NULL,Inventory int );");
                await writer.WriteLineAsync("CREATE TABLE SQLITEADMIN_QUERIES(ID INTEGER PRIMARY KEY,NAME VARCHAR(100),SQL TEXT);");
                await writer.WriteLineAsync("INSERT INTO SQLITEADMIN_QUERIES VALUES(1,'grr','select Cardname from swd where id = ''1999''; insert into SWDTEMP (CardName)');");
                await writer.WriteLineAsync("CREATE TABLE SWD (id int, CardName char(80), Grouping char(6), CardType char(18), Subtype char(40), ModelType char(40), Expansion char(33), Rarity char(15), Uniqueness char(6), Characteristics char(60), Destiny char(4), Power char(4), Ferocity char(4), CreatureDefenseValue char(4), CreatureDefenseValueName char(20), ObjectiveFront text, ObjectiveBack text, ObjectiveFrontName char(80), ObjectiveBackName char(80), Deploy char(4), Forfeit char(4), Armor char(5), Ability char(4), Hyperspeed char(4), Landspeed char(4), Politics char(4), Maneuver char(4), ForceAptitude char(20), Lore text, Gametext text, JediTestNumber char(4), LightSideIcons char(4), DarkSideIcons char(4), LightSideText text, DarkSideText text, Parsec char(4), Icons char(60), Planet char(4), Space char(4), Mobile char(4), Interior char(4), Exterior char(4), Underground char(4), Creature char(4), Vehicle char(4), Starship char(4), Underwater char(4), Pilot char(4), Warrior char(4), Astromech char(4), PermanentWeapon char(4), SelectiveCreature char(4), Independent char(4), ScompLink char(4), Droid char(4), TradeFederation char(4), Republic char(4), Episode1 char(4), Information text, Abbreviation char(50), Pulls text, IsPulled text, Counterpart char(50), Combo text, Matching char(50), MatchingWeapon char(50), Rules text, Cancels text, IsCanceledBy text, Inventory int, Needs int, ExpansionV VARCHAR(40), Influence char(4), Grabber char(4), Errata char(4), CardNameV char(80), UniquenessV char(6), Uuid char(36), GempId char(32));");
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
                            var properties = row
                                .EnumerateObject()
                                .Select(p => KeyValuePair.Create(p.Name, GetValue(p.Value)))
                                .ToList();
                            
                            var swIpId = row.GetProperty("id").GetInt32();
                            
                            if (!row.TryGetProperty(Uuid, out _))
                            {
                                var value = $"'{uuids[swIpId]}'";
                                properties.Add(KeyValuePair.Create(Uuid, value));
                            }

                            if (!row.TryGetProperty(GempId, out _))
                            {
                                var grouping = row.GetProperty("Grouping").GetString();
                                var expansion = row.GetProperty("Expansion").GetString();
                                var gempExpansion = GempExpansions[expansion];
                                var cardName = row.GetProperty("CardName").GetString();
                                var gempName = new string(CleanCardName(cardName)
                                    .Select(c => char.ToLowerInvariant(c))
                                    .Where(c => char.IsLetterOrDigit(c) || c == '&').ToArray());
                                var key = $"/gemp-swccg/images/cards/{gempExpansion}-{grouping}/{gempName}.gif";
                                
                                if (gempData.TryGetValue(key, out var gempId))
                                {
                                    properties.Add(KeyValuePair.Create(GempId, gempId));
                                }
                                else
                                {
                                    Console.WriteLine($"No Gemp ID for [{swIpId}] {cardName}.");
                                }
                            }
                            
                            await writer.WriteAsync("INSERT INTO SWD (");
                            await writer.WriteAsync(string.Join(",", properties.Select(p => p.Key)));
                            await writer.WriteAsync(") VALUES (");
                            await writer.WriteAsync(string.Join(",", properties.Select(p => p.Value)));
                            await writer.WriteLineAsync(");");

                            var id = row.GetProperty("id").GetInt32();
                            minId = Math.Min(minId, id);
                            maxId = Math.Max(maxId, id);

                            if (!ids.Add(id))
                                duplicateIds.Add(id);
                        }
                    }
                }

                await writer.WriteLineAsync("COMMIT;");
            }

            Console.WriteLine("Unique IDs: " + ids.Count);
            Console.WriteLine($"ID range: {minId} to {maxId}");
            
            if (0 < duplicateIds.Count)
            {
                Console.WriteLine("Duplicate IDs: " + string.Join(", ", duplicateIds));
                var gaps = Enumerable.Range(minId, maxId).Where(n => !ids.Contains(n));
                var list = string.Join(", ", gaps);
                
                if (!string.IsNullOrWhiteSpace(list))
                    Console.WriteLine("Gaps: " + list);
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
