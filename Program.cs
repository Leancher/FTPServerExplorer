using MyFTPserverExplorer;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// ��������� ������� CORS
builder.Services.AddCors(); 

var app = builder.Build();

AppProps appProps = new();

await init();
//���� � ������������ �����, ������ ����� ������� ����� ������������
string curDirPath = appProps.CurDirPath;

//������������� ����� ��� ��������� ������ �� ��������
//����� wwwroot
var webRootProvider = new PhysicalFileProvider(builder.Environment.WebRootPath);
//���� �����
var newPathProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, appProps.CurDirPath));
var compositeProvider = new CompositeFileProvider(webRootProvider, newPathProvider);
app.Environment.WebRootFileProvider = compositeProvider;
app.UseStaticFiles();

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
    appProps.WebHost = string.Concat("http://", request.Host.ToString());
    //��� ������� � ������� ������ ������������ � Body
    //�������� � ���� ������, ����� ������������� � JSON
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    ResponseData? data = new();
    if (bodyAsString == "")
    {
        //���� ������ ���, ������ ����� �������� �����
        await handleResponse("/", "init", response);
    }
    else
    {
        //���� ������ ����, ��������� �� JSON.
        //�������� ?? ���������� ����� �������, ���� ���� ������� �� ����� null.
        //����� ������������ ������ �������. ��� ���� ����� ������� ������ ��������� null.
        data = JsonSerializer.Deserialize<ResponseData>(bodyAsString ?? "/");
        //? - ���� dir �� ����� null, �� ���������� ��������� � ��� �������� Name
        await handleResponse(data?.DirName ?? "/", data?.Command ?? "/", response);
    }
}
//��������� ���������� �������
async Task handleResponse(string selectedDir, string command, HttpResponse response)
{
    switch (command)
    {
        //���������� ��� ������ �������, ���������� ��������
        case "init":
            response.ContentType = "text/html";
            await response.SendFileAsync("index.html");
            break;
        //����� �������� ��������, ��� ����������� �������� �����
        case "root":
            await sendDirsLsit(response, curDirPath);
            break;
        //��� ������� ������ �����
        case "back":
            var len = curDirPath.LastIndexOf('/');
            if (len != -1) curDirPath = curDirPath.Substring(0, len);
            await sendDirsLsit(response, curDirPath);
            break;
        case "props":
            await sendProps(response, curDirPath);
            break;

        //�� ���� ��������� ������� �������� �������� ��������� �����
        default:
            curDirPath = Path.Combine(curDirPath, selectedDir);
            await sendDirsLsit(response, curDirPath);
            break;
    }
}

//���������� ������ � �������, � ����� ��������� ���-�� ����� �
//��� ������: ���������� ��� ������ ����� ��� ��� ������ �������� ������
async Task sendDirsLsit(HttpResponse response, string path)
{
    List<string> responseData = [];
    var dir = new DirectoryInfo(path + "\\");
    DirectoryInfo[] dirs = dir.GetDirectories();
    for (int n = 0; n < dirs.Length; n++)
    {
        responseData.Add(dirs[n].Name);
    }
    //responseStr = JsonSerializer.Serialize(responseData);
    await response.WriteAsJsonAsync(responseData);
}

async Task sendProps(HttpResponse response, string curDirPath)
{
    //������ ��������:
    //���� � ������� �����
    //��� ����� � ��������� �����
    //����� �����
    //����� ���-������
    checkImageDir(curDirPath);
    //������� ��� �����
    int num = curDirPath.IndexOf('\\');
    if (num != -1) curDirPath = curDirPath.Substring(num + 1);
    appProps.CurDirPath = curDirPath;
    string responseStr = JsonSerializer.Serialize<AppProps>(appProps);
    await response.WriteAsJsonAsync(responseStr);
}

async Task init()
{
    string initFile = Path.Combine(Environment.CurrentDirectory, "Config.ini");
    using (StreamReader reader = new(initFile))
    {
        string? propsStr="";
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            propsStr=string.Concat(propsStr,"\"", line.Split("=")[0], "\":\"",line.Split("=")[1], "\",");
        }
        propsStr = propsStr.TrimEnd([',']);
        propsStr = string.Concat("{",propsStr,"}");       
        appProps = JsonSerializer.Deserialize<AppProps>(propsStr);
    };
}

void checkImageDir(string path)
{
    FileInfo file = new(Path.Combine(path, appProps.ImageDirFile));
    appProps.IsImageDir = "0";
    if (file.Exists) appProps.IsImageDir = "1";
}