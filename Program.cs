using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// ��������� ������� CORS
builder.Services.AddCors(); 

var app = builder.Build();

List<string> responseData = new List<string>();
string responseStr = "";
Props appProps = new();
string initFile = Environment.CurrentDirectory + "\\setup.ini";
await ReadInitFile();
string curDir = appProps.rootDir;

// ����������� CORS. ����� ������������ ��� ������, ���������, ���������.
app.UseCors(builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    await getResponse(request, response);
});

app.Run();

async Task getResponse(HttpRequest request, HttpResponse response)
{
    //��� ������� � ������� ������ ������������ � Body
    //�������� � ���� ������, ����� ������������� � JSON
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    Dir? dir = new(); 
    if (bodyAsString == "")
    {
        //���� ������ ���, ������ ����� �������� �����
        await handleResponse("/", response);
    }
    else
    {
        //���� ������ ����, ��������� �� JSON.
        //�������� ?? ���������� ����� �������, ���� ���� ������� �� ����� null.
        //����� ������������ ������ �������. ��� ���� ����� ������� ������ ��������� null.
        dir = JsonSerializer.Deserialize<Dir>(bodyAsString ?? "/");
        //? - ���� dir �� ����� null, �� ���������� ��������� � ��� �������� Name
        await handleResponse(dir?.Name ?? "/", response);
    }
}
//��������� ���������� �������
async Task handleResponse(string selectedDir, HttpResponse response)
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
            await sendResponse(response, curDir);
            break;
        //��� ������� ������ �����
        case "back":
            var len = curDir.LastIndexOf('/');
            if (len != -1) curDir = curDir.Substring(0, len);
            await sendResponse(response, curDir);
            break;
        //�� ���� ��������� ������� �������� �������� ��������� �����
        default:
            curDir = string.Concat(curDir, "/", selectedDir);
            await sendResponse(response, curDir);
            break;
    }
}

async Task sendResponse(HttpResponse response, string path)
{
    responseData = new List<string>();
    await addDirsListToResponse(path);
    //await addParamsToResponse(path);
    await response.WriteAsJsonAsync(responseData);
};

async Task addDirsListToResponse(string path)
{
    var dir = new DirectoryInfo(path + "\\");
    DirectoryInfo[] dirs = dir.GetDirectories();
    for (int n = 0; n < dirs.Length; n++)
    {
        responseData.Add(dirs[n].Name);
    }
    responseStr = JsonSerializer.Serialize(responseData);
}

async Task addParamsToResponse(string path)
{
    //������� 1 - ��� ������. ���������� ��� ������ ����� ��� ��� ������ �������� ������
    responseData.Add(checkImageDir(path).Result.ToString() ?? "0");
    responseStr = JsonSerializer.Serialize<Props>(appProps);
}

async Task ReadInitFile()
{
    using (StreamReader reader = new StreamReader(initFile))
    {
        string? propsStr="";
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            propsStr=string.Concat(propsStr,"\"", line.Split("=")[0], "\":\"",line.Split("=")[1], "\",");
        }
        propsStr = propsStr.TrimEnd([',']);
        propsStr = string.Concat("{",propsStr,"}");       
        appProps = JsonSerializer.Deserialize<Props>(propsStr);
    };
    
}

async Task<int> checkImageDir(string path)
{
    FileInfo file = new FileInfo(path + "\\" + appProps.imageDirFile);
    if (file.Exists) return 1;
    return 0;
}

public class Props
{
    public string rootDir { get; set; } = "";
    public string picDir { get; set; } = "";
    public string imageDirFile { get; set; } = "";
    public string imagePic { get; set; } = "";
    public Props() { }
    public Props(string rootDir, string picDir, string imageDirFile, string imagePic) { 
        this.rootDir = rootDir;
        this.picDir = picDir;
        this.imageDirFile = imageDirFile;
        this.imagePic = imagePic;
    }
}

public class Dir
{
    public string Name { get; set; } = "";
    public string Test { get; set; } = "";
    public Dir() { }
    public Dir (string name, string test) {  Name = name; Test = test; }
}