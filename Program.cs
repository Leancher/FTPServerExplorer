using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Добавляем сервисы CORS
builder.Services.AddCors(); 

var app = builder.Build();

string[] dirsList;
string curDir= "d:";
string initFile = Environment.CurrentDirectory + "\\setup.ini";

await ReadInitFile();

// Настраиваем CORS. Можно использовать все методы, заголовки, источники.
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
    //При запросе с клиента данные записываются в Body
    //Получаем в виде строки, затем преобразоваем в JSON
    using StreamReader reader = new(request.Body);
    string bodyAsString = await reader.ReadToEndAsync();
    Dir? dir = new("");
    string json = JsonSerializer.Serialize(bodyAsString);    
    if (bodyAsString == "")
    {
        //Если данных нет, значит нужна корневая папка
        await handleSelectedDir("/", response);
    }
    else
    {
        //Если данные есть, извлекаем из JSON.
        //Оператор ?? возвращает левый операнд, если этот операнд не равен null.
        //Иначе возвращается правый операнд. При этом левый операнд должен принимать null.
        dir = JsonSerializer.Deserialize<Dir>(bodyAsString ?? "/");
        //? - если dir не равен null, то происходит обращение к его свойству Name
        await handleSelectedDir(dir?.Name ?? "/", response);
    }
}
//Обработка полученной команды
async Task handleSelectedDir(string selectedDir, HttpResponse response)
{
    switch (selectedDir)
    {
        //Происходит при первом запуске, отправляем страницу
        case "/":
            response.ContentType = "text/html; charset=utf-8";
            await response.SendFileAsync("index.html");
            break;
        //После отправки старницы, она запрашивает корневую папку
        case "root":
            await sendDirsList(response, curDir);
            break;
        //При нажатии кнопки Назад
        case "back":
            var len = curDir.LastIndexOf('/');
            if (len != -1) curDir = curDir.Substring(0, len);
            await sendDirsList(response, curDir);
            break;
        //Во всех остальных случаях получаем название выбранной папки
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