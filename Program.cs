using MyFTPserverExplorer;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Добавляем сервисы CORS
builder.Services.AddCors(); 

var app = builder.Build();

AppProps appProps = new();

await init();
//Путь к родительской папке, список папок которой будет передаваться
string curDirPath = appProps.CurDirPath;

//Устанавливаем папки для получения файлов на странице
//Папка wwwroot
var webRootProvider = new PhysicalFileProvider(builder.Environment.WebRootPath);
//Своя папка
var newPathProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, appProps.CurDirPath));
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
    ResponseData? data = new();
    if (bodyAsString == "")
    {
        //Если данных нет, значит нужна корневая папка
        await handleResponse("/", "init", response);
    }
    else
    {
        //Если данные есть, извлекаем из JSON.
        //Оператор ?? возвращает левый операнд, если этот операнд не равен null.
        //Иначе возвращается правый операнд. При этом левый операнд должен принимать null.
        data = JsonSerializer.Deserialize<ResponseData>(bodyAsString ?? "/");
        //? - если dir не равен null, то происходит обращение к его свойству Name
        await handleResponse(data?.DirName ?? "/", data?.Command ?? "/", response);
    }
}
//Обработка полученной команды
async Task handleResponse(string selectedDir, string command, HttpResponse response)
{
    switch (command)
    {
        //Происходит при первом запуске, отправляем страницу
        case "init":
            response.ContentType = "text/html";
            await response.SendFileAsync("index.html");
            break;
        //После отправки страницы, она запрашивает корневую папку
        case "root":
            await sendDirsLsit(response, curDirPath);
            break;
        //При нажатии кнопки Назад
        case "back":
            var len = curDirPath.LastIndexOf('/');
            if (len != -1) curDirPath = curDirPath.Substring(0, len);
            await sendDirsLsit(response, curDirPath);
            break;
        case "props":
            await sendProps(response, curDirPath);
            break;

        //Во всех остальных случаях получаем название выбранной папки
        default:
            curDirPath = Path.Combine(curDirPath, selectedDir);
            await sendDirsLsit(response, curDirPath);
            break;
    }
}

//Составляем массив с папками, в конце добавляем кол-во папок и
//тип данных: отображать как список папок или как список картинок дисков
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
    //Список настроек:
    //Путь к текущей папке
    //Имя файла с картинкой диска
    //Адрес сайта
    //Адрес ФТП-севера
    checkImageDir(curDirPath);
    //Удаляем имя диска
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