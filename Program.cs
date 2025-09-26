using MyFTPserverExplorer;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Добавляем сервисы CORS
builder.Services.AddCors(); 

var app = builder.Build();

AppProps appProps = new();
ResponseData responseData = new();

await init();

//Устанавливаем папки для получения файлов на странице
//Папка wwwroot
var webRootProvider = new PhysicalFileProvider(builder.Environment.WebRootPath);
//Своя папка
var newPathProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, appProps.FileHost));
var compositeProvider = new CompositeFileProvider(webRootProvider, newPathProvider);
app.Environment.WebRootFileProvider = compositeProvider;
app.UseStaticFiles();

// Настраиваем CORS. Можно использовать все методы, заголовки, источники.
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
    //При запросе с клиента данные записываются в Body
    //Получаем в виде строки, затем преобразоваем в JSON
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    
    if (bodyAsString == "")
    {
        //Если данных нет, значит нужна корневая папка
        responseData.DirName = "\\";
        responseData.Command = "init";
        responseData.Direction = "";
    }
    else
    {
        //Если данные есть, извлекаем из JSON.
        responseData = JsonSerializer.Deserialize<ResponseData>(bodyAsString);      
    }
    await handleResponseCommand(response);
}
//Обработка полученной команды
async Task handleResponseCommand(HttpResponse response)
{
    switch (responseData.Command)
    {
        //Происходит при первом запуске, отправляем страницу
        case "init":
            response.ContentType = "text/html";
            await response.SendFileAsync("index.html");
            break;
        case "props":
            await sendProps(response);
            break;
        //Во всех остальных случаях получаем название выбранной папки
        default:
            await sendDirsLsit(response);
            break;
    }
}

//Составляем массив с папками, в конце добавляем кол-во папок и
//тип данных: отображать как список папок или как список картинок дисков
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
    //Список настроек:
    //Путь к текущей папке
    //Путь к папке с обратной /
    //Имя файла с картинкой диска
    //Адрес сайта
    //Адрес ФТП-севера

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