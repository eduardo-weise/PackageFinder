using System.Text.Json;
using System.Text.Json.Serialization;

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

        var packages = await File.ReadAllLinesAsync(filePath);

        foreach (string package in packages)
        {
            var packageInfo = await GetPackageInfo(package);
            var vulnerabilities = await GetVulnerabilities(package);

            Console.WriteLine($"Package: {package}");
            
            foreach (var property in typeof(CatalogEntry).GetProperties())
            {
                var value = property.GetValue(packageInfo);
                Console.WriteLine($"{property.Name}: {value}");
            }

            Console.WriteLine($"Vulnerabilities: {vulnerabilities?.Count ?? 0}");

            if (vulnerabilities is not null)
            {
                foreach (Vulnerability vulnerability in vulnerabilities)
                {
                    Console.WriteLine($"- {vulnerability.Title}");
                    Console.WriteLine($"  Description: {vulnerability.Description}");
                    Console.WriteLine($"  Reference: {vulnerability.Reference}");
                }
            }
            Console.WriteLine();
        }
    }

    static async Task<CatalogEntry?> GetPackageInfo(string package)
    {
        var packageId = package.Split('@').First();
        var packageVersion = package.Split('@').Last();

        var url = $"https://api.nuget.org/v3/registration5-semver1/{packageId.ToLower()}/{packageVersion}.json";
        var responseApi = await httpClient.GetAsync(url);
        var packageInfoResponse = await responseApi.Content.ReadAsStringAsync();
        var packageInfo = JsonSerializer.Deserialize<PackageInfo>(packageInfoResponse, jsonOptions);

        var responseCatalogEntry = await httpClient.GetAsync(packageInfo!.CatalogEntryUrl);
        var catalogEntryString = await responseCatalogEntry.Content.ReadAsStringAsync();
        var root = JsonSerializer.Deserialize<CatalogEntry>(catalogEntryString, jsonOptions);

        return root;
    }

    static async Task<List<Vulnerability>?> GetVulnerabilities(string package)
    {
        var url = $"https://ossindex.sonatype.org/api/v3/component-report/{package}";
        HttpResponseMessage response = await httpClient.GetAsync(url);

        var json = await response.Content.ReadAsStringAsync();
        ComponentReport? report = JsonSerializer.Deserialize<ComponentReport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return report?.Vulnerabilities;
    }
}

public class PackageInfo
{
    [JsonPropertyName("catalogEntry")]
    public string CatalogEntryUrl { get; set; }
}

public class ComponentReport
{
    [JsonPropertyName("vulnerabilities")]
    public List<Vulnerability>? Vulnerabilities { get; set; }
}

public class Vulnerability
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    public override string ToString()
    {
        return $"Title: {Title ?? "N/A"}\n" +
           $"Description: {Description ?? "N/A"}\n" +
           $"Reference: {Reference ?? "N/A"}\n";
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
        return $":\n    Id: {Id}" +
           $"\n Message: {Message ?? "N/A"}" +
           $"\n Reasons: {(Reasons is not null ? string.Join(", ", Reasons) : "N/A")}";
    }
}

public class CatalogEntry
{
    [JsonPropertyName("@id")]
    public string? Id { get; set; }
    public string? Authors { get; set; }
    public string? Copyright { get; set; }
    public DateTime? Created { get; set; }
    public Deprecation? Deprecation { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool? IsPrerelease { get; set; }
    public DateTime? LastEdited { get; set; }
    public string? LicenseUrl { get; set; }
    public bool? Listed { get; set; }
    public string? PackageHash { get; set; }
    public string? PackageHashAlgorithm { get; set; }
    public int? PackageSize { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? Published { get; set; }
    public string? Repository { get; set; }
    public bool? RequireLicenseAcceptance { get; set; }
    public string? Serviceable { get; set; }
    public string? VerbatimVersion { get; set; }
    public string? Version { get; set; }

    public override string ToString()
    {
        return $"\n{nameof(CatalogEntry)}:" +
               $"\n  {nameof(Id)}: {Id}" +
               $"\n  {nameof(Authors)}: {Authors}" +
               $"\n  {nameof(Copyright)}: {Copyright}" +
               $"\n  {nameof(Created)}: {Created}" +
               $"\n  {nameof(Deprecation)}: {Deprecation}" +
               $"\n  {nameof(Description)}: {Description}" +
               $"\n  {nameof(IconUrl)}: {IconUrl}" +
               $"\n  {nameof(IsPrerelease)}: {IsPrerelease}" +
               $"\n  {nameof(LastEdited)}: {LastEdited}" +
               $"\n  {nameof(LicenseUrl)}: {LicenseUrl}" +
               $"\n  {nameof(Listed)}: {Listed}" +
               $"\n  {nameof(PackageHash)}: {PackageHash}" +
               $"\n  {nameof(PackageHashAlgorithm)}: {PackageHashAlgorithm}" +
               $"\n  {nameof(PackageSize)}: {PackageSize}" +
               $"\n  {nameof(ProjectUrl)}: {ProjectUrl}" +
               $"\n  {nameof(Published)}: {Published}" +
               $"\n  {nameof(Repository)}: {Repository}" +
               $"\n  {nameof(RequireLicenseAcceptance)}: {RequireLicenseAcceptance}" +
               $"\n  {nameof(Serviceable)}: {Serviceable}" +
               $"\n  {nameof(VerbatimVersion)}: {VerbatimVersion}" +
               $"\n  {nameof(Version)}: {Version}";
    }
}