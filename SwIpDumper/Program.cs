using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwIpDumper
{
    class Program
    {
        private static readonly Encoding theEncoding = Encoding.GetEncoding("ISO-8859-1");
        static async Task Main(string[] args)
        {
            var buffer = new byte[8192];
            try
            {
                using (var connection = new SQLiteConnection("Data Source=swccg_db.sqlite;Version=3"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM SWD ORDER BY `id` ASC";
                        command.Prepare();

                        using (var reader = command.ExecuteReader())
                        {
                            var cardData = new Dictionary<string, List<Dictionary<string, object>>>();
                            var skipColumns = ImmutableHashSet.Create(
                                reader.GetOrdinal("Inventory"),
                                reader.GetOrdinal("Needs"));

                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; ++i)
                                {
                                    if (!skipColumns.Contains(i))
                                    {
                                        var content = reader[i].ToString() ?? string.Empty;
                                        
                                        if (!string.IsNullOrWhiteSpace(content))
                                        {
                                            if (double.TryParse(content, out var asDouble))
                                                row.Add(reader.GetName(i), asDouble);
                                            else
                                                row.Add(reader.GetName(i), content);
                                        }
                                    }
                                }

                                var expansion = row["Expansion"].ToString() ?? throw new NullReferenceException("Expansion must have a value!");

                                if (!cardData.TryGetValue(expansion, out var cardsInExpansion))
                                {
                                    cardsInExpansion = new List<Dictionary<string, object>>();
                                    cardData.Add(expansion, cardsInExpansion);
                                }

                                cardsInExpansion.Add(row);
                            }

                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };

                            var folder = args[0];
                            Directory.CreateDirectory(folder);

                            foreach (var pair in cardData)
                            {
                                var cleanExpansion = pair.Key
                                    .ToLowerInvariant()
                                    .Replace("  \n", " ")
                                    .Replace(' ', '-')
                                    .Replace("'", "")
                                    .Replace("#", "")
                                    .Replace("\r", "");
                                
                                var fileName = Path.Combine(folder, cleanExpansion + ".json");
                                
                                using (var stream = File.Create(fileName))
                                    await JsonSerializer.SerializeAsync(stream, pair.Value, options);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
