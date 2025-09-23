using MyFTPserverExplorer;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Добавляем сервисы CORS
builder.Services.AddCors(); 

var app = builder.Build();

List<string> responseData = [];
string responseStr = "";
AppProps appProps = new();
string initFile = Path.Combine(Environment.CurrentDirectory, "Config.ini");
await ReadInitFile();
string curDir = appProps.RootDir;

// Настраиваем CORS. Можно использовать все методы, заголовки, источники.
app.UseCors(builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(appProps.RootDir), // Используем физический файловый провайдер
});

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    await getResponse(request, response);
});

app.Run();

async Task getResponse(HttpRequest request, HttpResponse response)
{
    //При запросе с клиента данные записываются в Body
    //Получаем в виде строки, затем преобразоваем в JSON
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    Dir? dir = new();
    string host = request.Host.ToString();
    string path = request.Path.ToString();
    if (bodyAsString == "")
    {
        //Если данных нет, значит нужна корневая папка
        await handleResponse("/", response);
    }
    else
    {
        //Если данные есть, извлекаем из JSON.
        //Оператор ?? возвращает левый операнд, если этот операнд не равен null.
        //Иначе возвращается правый операнд. При этом левый операнд должен принимать null.
        dir = JsonSerializer.Deserialize<Dir>(bodyAsString ?? "/");
        //? - если dir не равен null, то происходит обращение к его свойству Name
        await handleResponse(dir?.Name ?? "/", response);
    }
}
//Обработка полученной команды
async Task handleResponse(string selectedDir, HttpResponse response)
{
    switch (selectedDir)
    {
        //Происходит при первом запуске, отправляем страницу
        case "/":
            response.ContentType = "text/html";
            await response.SendFileAsync("index.html");
            break;
        //После отправки страницы, она запрашивает корневую папку
        case "root":
            await sendResponse(response, curDir);
            break;
        //При нажатии кнопки Назад
        case "back":
            var len = curDir.LastIndexOf('/');
            if (len != -1) curDir = curDir.Substring(0, len);
            await sendResponse(response, curDir);
            break;
        //Во всех остальных случаях получаем название выбранной папки
        default:
            curDir = Path.Combine(curDir, selectedDir);
            await sendResponse(response, curDir);
            break;
    }
}

async Task sendResponse(HttpResponse response, string path)
{
    responseData = new List<string>();
    addDirsListToResponse(path);
    //await addParamsToResponse(path);
    await response.WriteAsJsonAsync(responseData);
};

//Составляем массив с папками, в конце добавляем кол-во папок и
//тип данных: тображать как список папок или как список картинок дисков
void addDirsListToResponse(string path)
{
    var dir = new DirectoryInfo(path + "\\");
    DirectoryInfo[] dirs = dir.GetDirectories();
    for (int n = 0; n < dirs.Length; n++)
    {
        responseData.Add(dirs[n].Name);
    }
    responseData.Add(checkImageDir(path).ToString() ?? "0");
    responseData.Add(dirs.Length.ToString());
    responseStr = JsonSerializer.Serialize(responseData);
}

void addParamsToResponse(string path)
{
    responseStr = JsonSerializer.Serialize<AppProps>(appProps);
}

async Task ReadInitFile()
{
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

int checkImageDir(string path)
{
    FileInfo file = new(Path.Combine(path, appProps.ImageDirFile));
    if (file.Exists) return 1;
    return 0;
}