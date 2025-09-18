using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// ��������� ������� CORS
builder.Services.AddCors(); 

var app = builder.Build();

string[] dirsList;
string curDir= "d:";
string initFile = Environment.CurrentDirectory + "\\setup.ini";

await ReadInitFile();

// ����������� CORS. ����� ������������ ��� ������, ���������, ���������.
app.UseCors(builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    await getSelectedDir(request, response);
});

app.Run();

async Task getSelectedDir(HttpRequest request, HttpResponse response)
{
    //��� ������� � ������� ������ ������������ � Body
    //�������� � ���� ������, ����� ������������� � JSON
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    Dir? dir = new("");
    string json = JsonSerializer.Serialize(bodyAsString);    
    if (bodyAsString == "")
    {
        //���� ������ ���, ������ ����� �������� �����
        await handleSelectedDir("/", response);
    }
    else
    {
        //���� ������ ����, ��������� �� JSON.
        //�������� ?? ���������� ����� �������, ���� ���� ������� �� ����� null.
        //����� ������������ ������ �������. ��� ���� ����� ������� ������ ��������� null.
        dir = JsonSerializer.Deserialize<Dir>(bodyAsString ?? "/");
        //? - ���� dir �� ����� null, �� ���������� ��������� � ��� �������� Name
        await handleSelectedDir(dir?.Name ?? "/", response);
    }
}
//��������� ���������� �������
async Task handleSelectedDir(string selectedDir, HttpResponse response)
{
    switch (selectedDir)
    {
        //���������� ��� ������ �������, ���������� ��������
        case "/":
            response.ContentType = "text/html; charset=utf-8";
            await response.SendFileAsync("index.html");
            break;
        //����� �������� ��������, ��� ����������� �������� �����
        case "root":
            await sendDirsList(response, curDir);
            break;
        //��� ������� ������ �����
        case "back":
            var len = curDir.LastIndexOf('/');
            if (len != -1) curDir = curDir.Substring(0, len);
            await sendDirsList(response, curDir);
            break;
        //�� ���� ��������� ������� �������� �������� ��������� �����
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