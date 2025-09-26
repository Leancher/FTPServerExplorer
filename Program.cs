using MyFTPserverExplorer;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// ��������� ������� CORS
builder.Services.AddCors(); 

var app = builder.Build();

AppProps appProps = new();
ResponseData responseData = new();

await init();

//������������� ����� ��� ��������� ������ �� ��������
//����� wwwroot
var webRootProvider = new PhysicalFileProvider(builder.Environment.WebRootPath);
//���� �����
var newPathProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, appProps.FileHost));
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
    
    if (bodyAsString == "")
    {
        //���� ������ ���, ������ ����� �������� �����
        responseData.DirName = "\\";
        responseData.Command = "init";
        responseData.Direction = "";
    }
    else
    {
        //���� ������ ����, ��������� �� JSON.
        responseData = JsonSerializer.Deserialize<ResponseData>(bodyAsString);      
    }
    await handleResponseCommand(response);
}
//��������� ���������� �������
async Task handleResponseCommand(HttpResponse response)
{
    switch (responseData.Command)
    {
        //���������� ��� ������ �������, ���������� ��������
        case "init":
            response.ContentType = "text/html";
            await response.SendFileAsync("index.html");
            break;
        case "props":
            await sendProps(response);
            break;
        //�� ���� ��������� ������� �������� �������� ��������� �����
        default:
            await sendDirsLsit(response);
            break;
    }
}

//���������� ������ � �������, � ����� ��������� ���-�� ����� �
//��� ������: ���������� ��� ������ ����� ��� ��� ������ �������� ������
async Task sendDirsLsit(HttpResponse response)
{
    List<string> responseData = [];
    var dir = new DirectoryInfo(appProps.FileHost + appProps.CurDirPath);
    DirectoryInfo[] dirs = dir.GetDirectories();
    for (int n = 0; n < dirs.Length; n++)
    {
        responseData.Add(dirs[n].Name);
    }
    await response.WriteAsJsonAsync(responseData);
}

async Task sendProps(HttpResponse response)
{
    //������ ��������:
    //���� � ������� �����
    //���� � ����� � �������� /
    //��� ����� � ��������� �����
    //����� �����
    //����� ���-������

    if (responseData.Direction == "nextDir") appProps.CurDirPath = Path.Combine(appProps.CurDirPath, responseData.DirName);
    if (responseData.Direction == "prevDir")
    {
        var len = appProps.CurDirPath.LastIndexOf('\\');
        if (len != -1) appProps.CurDirPath = appProps.CurDirPath.Substring(0, len);
        if (appProps.CurDirPath=="") appProps.CurDirPath = "\\";
    }
    appProps.URL = appProps.CurDirPath.Replace('\\', '/');
    checkImageDir(appProps.CurDirPath);
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
    FileInfo file = new(string.Concat(appProps.FileHost, path, "/", appProps.ImageDirFile));
    appProps.IsImageDir = "0";
    if (file.Exists) appProps.IsImageDir = "1";
}