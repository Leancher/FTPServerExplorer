using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(); // добавляем сервисы CORS

var app = builder.Build();

string[] dirsList;
string curDir= "d:";
string initFile = Environment.CurrentDirectory + "\\setup.ini";

// настраиваем CORS
app.UseCors(builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    await ReadInitFile();
    await getSelectedDir(request, response);
});

app.Run();

async Task getSelectedDir(HttpRequest request, HttpResponse response)
{
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    Dir? dir = new("");
    string json = JsonSerializer.Serialize(bodyAsString);
    if (bodyAsString == "") await handleSelectedDir("/", response);
    if (bodyAsString != null & bodyAsString != "")
    {
        dir = JsonSerializer.Deserialize<Dir>(bodyAsString);
        await handleSelectedDir(dir?.Name, response);
    }
}

async Task handleSelectedDir(string selectedDir, HttpResponse response)
{
    switch (selectedDir)
    {
        case "/":
            response.ContentType = "text/html; charset=utf-8";
            await response.SendFileAsync("index.html");
            break;
        case "root":
            await sendDirsList(response, curDir);
            break;
        case "back":
            var len = curDir.LastIndexOf('/');
            if (len != -1) curDir = curDir.Substring(0, len);
            await sendDirsList(response, curDir);
            break;
        default:
            curDir = string.Concat(curDir, "/", selectedDir);
            await sendDirsList(response, curDir);
            break;
    }
}

async Task sendDirsList(HttpResponse response, string path)
{
    var dir = new DirectoryInfo(path + "\\");
    DirectoryInfo[] dirs = dir.GetDirectories();
    dirsList = new string[dirs.Length];
    for (int n = 0; n < dirs.Length; n++)
    {
        dirsList[n] = dirs[n].Name;
    }
    await response.WriteAsJsonAsync(dirsList);
};

async Task ReadInitFile()
{
    using (StreamReader reader = new StreamReader(initFile))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            curDir = line.Substring(line.IndexOf("=") + 1);
        }
    };
}


public class Dir
{
    public string Name { get; set; } = "";
    public Dir (string name) {  Name = name; }
}