using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PackageFinder;

class Program
{
    private static readonly HttpClient httpClient = new();
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    static async Task Main(string[] args)
    {
        string filePath = "c:\\packages.txt";

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"O arquivo {filePath} não foi encontrado.");
            return;
        }

        string[] packages = await File.ReadAllLinesAsync(filePath);

        foreach (string package in packages)
        {
            CatalogEntry? packageInfo = await GetPackageInfo(package);
            Console.WriteLine($"Package: {package}");

            foreach (PropertyInfo property in typeof(CatalogEntry).GetProperties())
            {
                object? value = property.GetValue(packageInfo);
                Console.WriteLine($"{property.Name}: {value ?? "N/A"}");
            }

            List<Vulnerability>? vulnerabilities = await GetVulnerabilities(package);
            Console.WriteLine($"Vulnerabilities: {vulnerabilities?.Count ?? 0}");

            if (vulnerabilities is not null)
            {
                foreach (Vulnerability vulnerability in vulnerabilities)
                {
                    Console.WriteLine(vulnerability.ToString());
                }
            }
            Console.WriteLine();
        }
    }

    private static async Task<CatalogEntry?> GetPackageInfo(string package)
    {
        string packageId = package.Split('@').First();
        string packageVersion = package.Split('@').Last();

        string url = $"https://api.nuget.org/v3/registration5-semver1/{packageId.ToLower()}/{packageVersion}.json";

        try
        {
            HttpResponseMessage responseApi = await httpClient.GetAsync(url);
            responseApi.EnsureSuccessStatusCode();
            PackageInfo? packageInfo = await DeserializeJsonAsync<PackageInfo>(responseApi);

            if (packageInfo is not null)
            {
                HttpResponseMessage catalogEntryApi = await httpClient.GetAsync(packageInfo!.CatalogEntryUrl);
                catalogEntryApi.EnsureSuccessStatusCode();
                CatalogEntry? catalogEntry = await DeserializeJsonAsync<CatalogEntry>(catalogEntryApi);

                return catalogEntry;
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ocorreu um erro ao fazer a solicitação HTTP: {ex.Message}");
            return null;
        }
    }

    private static async Task<List<Vulnerability>?> GetVulnerabilities(string package)
    {
        string url = $"https://ossindex.sonatype.org/api/v3/component-report/{package}";
        HttpResponseMessage response = await httpClient.GetAsync(url);

        string json = await response.Content.ReadAsStringAsync();
        ComponentReport? report = JsonSerializer.Deserialize<ComponentReport>(json, jsonOptions);

        return report?.Vulnerabilities;
    }

    private static async Task<T?> DeserializeJsonAsync<T>(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();

        if (json is null)
            return default;

        return JsonSerializer.Deserialize<T>(json, jsonOptions);
    }
}

public class PackageInfo
{
    [JsonPropertyName("catalogEntry")]
    public string? CatalogEntryUrl { get; set; }
}

public class ComponentReport
{
    public List<Vulnerability>? Vulnerabilities { get; set; }
}

public class Vulnerability
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Reference { get; set; }

    public override string ToString()
    {
        return $":\n - Title: {Title ?? "N/A"}" +
           $"\n - Description: {Description ?? "N/A"}" +
           $"\n - Reference: {Reference ?? "N/A"}";
    }
}

public class Deprecation
{
    [JsonPropertyName("@id")]
    public string? Id { get; set; }
    public string? Message { get; set; }
    public List<string>? Reasons { get; set; }

    public override string ToString()
    {
        return $":\n - Id: {Id}" +
           $"\n - Message: {Message ?? "N/A"}" +
           $"\n - Reasons: {(Reasons is not null ? string.Join(", ", Reasons) : "N/A")}";
    }
}

public class CatalogEntry
{
    [JsonPropertyName("@id")]
    public string? Id { get; set; }
    public string? Authors { get; set; }
    public string? Copyright { get; set; }
    public DateTime? Created { get; set; }
    public string? Version { get; set; }
    public DateTime? LastEdited { get; set; }
    public string? LicenseUrl { get; set; }
    public Deprecation? Deprecation { get; set; }
}