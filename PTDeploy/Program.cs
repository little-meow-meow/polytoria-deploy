// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Http.Headers;
using Polytoria.Creator.Utils;
using Polytoria.Formats;
using Polytoria.Shared;

const string usageString = "Usage: <token> <projectPath> <placeId>";

if (args.Length < 3)
{
    Console.Error.WriteLine(usageString);
    Environment.Exit(1);
}

var token = args[0];
var projectPath = args[1];
if (!int.TryParse(args[2], out var placeId))
{
    Console.Error.WriteLine("Failed to parse placeId argument");
    Console.Error.WriteLine(usageString);
    Environment.Exit(1);
}

PolyCreatorAPI.SetToken(token);

try
{
    var property = typeof(PolyCreatorAPI).GetProperty("IsUserAuthenticated");
    var setter = property!.SetMethod;
    setter!.Invoke(null, [true]);
}
catch (Exception)
{
    Console.Error.WriteLine("Failed to connect creator token. This tool probably needs to be updated!");
    throw;
}

var metadata = PackedFormat.ReadProjectMetadata(File.ReadAllText(Path.Combine(projectPath,
    Globals.ProjectMetaFileName)));
var packed = await PackedFormat.PackProject(projectPath);

// We can't `await PolyCreatorAPI.UploadWorld` because PTHttpClient is dependent on Godot runtime
// USE_NATIVE_HTTP environment variable would work but PT does not currently compile with it 

// See https://github.com/polytoria/polytoria-game/blob/main/Polytoria/scripts/creator/utils/PolyCreatorAPI.cs

var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", $"Polytoria Client {Globals.AppVersion}");
using MultipartFormDataContent form = new();
form.Add(new StringContent(placeId.ToString()), "id");
form.Add(new StringContent(token), "token");
form.Add(new StringContent(metadata.MainWorld), "mainPlacePath");
form.Add(new StringContent(Globals.MajorAppVersion), "majorVersion");

ByteArrayContent fileContent = new(packed);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
form.Add(fileContent, "file", "level.ptpacked");

var response = await client.PostAsync(Path.Join(Globals.ApiEndpoint, "v1/creator/upload-place"), form);

if (response.IsSuccessStatusCode)
{
    Console.WriteLine("Done");
    Environment.Exit(0);
}

var responseBody = await response.Content.ReadAsStringAsync();

Console.Error.WriteLine($"Failed to upload: {(int)response.StatusCode} {response.ReasonPhrase}");
Console.Error.WriteLine($"Server said: {responseBody}");

switch (response.StatusCode)
{
    case HttpStatusCode.Forbidden:
        Console.Error.WriteLine("The creator token may be expired!");
        break;
    case HttpStatusCode.BadRequest:
        Console.Error.WriteLine("The place ID may be incorrect!");
        break;
}

Environment.Exit(1);
