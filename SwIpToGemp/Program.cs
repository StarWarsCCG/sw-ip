using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwIpToGemp;

static class Program
{
    const string ApplicationFolderName = "SwIpToGemp";
    const string MappingFileName = "swccg-id-mapping.json";
    const string DarkUri = "https://raw.githubusercontent.com/swccgpc/swccg-card-json/main/Dark.json";
    const string LightUri = "https://raw.githubusercontent.com/swccgpc/swccg-card-json/main/Light.json";
    const string FinalTag = "[/Matchups]";
    const string Crlf = "\r\n";

    static readonly char[] s_separators = new char[] { '\r', '\n', '\t', ' '};

    static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    static async Task<CardDatabase> DownloadDatabaseAsync(HttpClient httpClient, string uri)
    {
        Console.WriteLine("Downloading card database -- " + uri);
        using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        using var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<CardDatabase>(stream, s_jsonSerializerOptions) ?? new CardDatabase();
        return result;
    }

    static async Task<ImmutableDictionary<int, CardMapping>> LoadMappingAsync(HttpClient httpClient)
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataFolder, ApplicationFolderName);
        _ = Directory.CreateDirectory(appFolder);

        var mappingFile = Path.Combine(appFolder, MappingFileName);
        List<CardMapping> cardMappings;

        if (File.Exists(mappingFile))
        {
            using var fileStream = File.OpenRead(mappingFile);
            cardMappings = await JsonSerializer.DeserializeAsync<List<CardMapping>>(fileStream, s_jsonSerializerOptions) ?? new List<CardMapping>();
        }
        else
        {
            cardMappings = new List<CardMapping>();

            var darkDatabase = await DownloadDatabaseAsync(httpClient, DarkUri);
            var lightDatabase = await DownloadDatabaseAsync(httpClient, LightUri);

            cardMappings.AddRange(darkDatabase.Cards.Select(card => card.ToCardMapping()));
            cardMappings.AddRange(lightDatabase.Cards.Select(card => card.ToCardMapping()));

            using var fileStream = File.Create(mappingFile);
            await JsonSerializer.SerializeAsync(fileStream, cardMappings, s_jsonSerializerOptions);
        }

        var result = cardMappings.ToImmutableDictionary(cardMapping => cardMapping.SwIpId);
        return result;
    }

    static async Task ConvertAsync(string path, ImmutableDictionary<int, CardMapping> mapping)
    {
        Console.WriteLine("Converting sw-ip deck file: " + path);
        var text = await File.ReadAllTextAsync(path);

        var indexOfFinalTag = text.IndexOf(FinalTag);

        if (indexOfFinalTag == -1)
        {
            Console.WriteLine("Deck file is malformed. :(");
            return;
        }

        var relevantText = text.Substring(indexOfFinalTag + FinalTag.Length);
        var numbers = relevantText.Split(s_separators, StringSplitOptions.RemoveEmptyEntries);
        var builder = new StringBuilder()
            .Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>")
            .Append(Crlf)
            .Append("<deck>")
            .Append(Crlf);

        for (int i = 3; i < numbers.Length; i += 4)
        {
            if (!int.TryParse(numbers[i - 3], out var swIpId))
            {
                Console.WriteLine("Invalid integer found for sw-ip card ID.");
                return;
            }

            if (!mapping.TryGetValue(swIpId, out var cardMapping))
            {
                Console.WriteLine("Cannot find GEMP card ID for sw-ip card ID " + swIpId);
                return;
            }

            builder
                .Append("<card blueprintId=\"")
                .AppendXml(cardMapping.GempId)
                .Append("\" title=\"")
                .AppendXml(cardMapping.Title)
                .Append("\"/>")
                .Append(Crlf);
        }

        builder.Append("</deck>").Append(Crlf);

        var destination = path + ".gemp.txt";
        await File.WriteAllTextAsync(destination, builder.ToString());
    }

    public static async Task Main(string[] args)
    {
        try
        {
            using var httpClient = new HttpClient();
            var mapping = await LoadMappingAsync(httpClient);

            foreach (var arg in args)
                await ConvertAsync(arg, mapping);
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(ex);
            Console.WriteLine();
        }
    }
}
