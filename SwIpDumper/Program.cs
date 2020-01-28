using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwIpDumper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using (var connection = new SQLiteConnection("Data Source=swccg_db.sqlite;Version=3"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM SWD";
                        command.Prepare();

                        using (var reader = command.ExecuteReader())
                        {
                            var cardData = new List<Dictionary<string, object>>();

                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; ++i)
                                {
                                    var content = reader[i].ToString();

                                    if (!string.IsNullOrWhiteSpace(content))
                                    {
                                        if (double.TryParse(content, out var asDouble))
                                            row.Add(reader.GetName(i), asDouble);
                                        else
                                            row.Add(reader.GetName(i), content);
                                    }
                                }

                                cardData.Add(row);
                            }

                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };

                            using (var stream = File.Create("sw-ip.json"))
                                await JsonSerializer.SerializeAsync(stream, cardData, options);
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
