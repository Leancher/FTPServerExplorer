//Список настроек:
//Путь к текущей папке
//Имя файла с картинкой диска
//Папки или диски?
//Адрес сайта
//Адрес ФТП-севера
var appProps = {
    curDirPath: "",
    url: "",
    diskImagePic: "Cover 00.jpg",
    isImageDir: "0",
    webhost: "",
    ftpHost: "ftp:///192.168.0.127//",
    nextDir: "nextDir",
    prevDir: "prevDir"
}

//Имя выбранной папки, направление: показ следующей папки или предыдущей
async function getResponse(dirName, direction) {
    await getProps(dirName, direction);
    await getDirsList();
}

async function getProps(dirName, direction) {
    const response = await fetch("/", {
        method: "POST", //Метод для отправки данных
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            //Имя выбранной папки
            DirName: dirName,
            //Команда
            Command: "props",
            //Следующая папка или предыдущая
            Direction: direction
        })
    });
    if (response.ok) {
        const responseData = JSON.parse(await response.json());
        appProps.curDirPath = responseData.CurDirPath;
        appProps.url = responseData.URL;
        appProps.diskImagePic = responseData.DiskImagePic;
        appProps.isImageDir = responseData.IsImageDir;
        appProps.webhost =  responseData.WebHost;
        appProps.ftpHost = responseData.FTPHost;
    }
}

async function getDirsList() {
    //Стираем содержимое перед обновлением
    document.getElementById("main").innerHTML = "";
    // отправляет запрос и получаем ответ
    const response = await fetch("/", {
        method: "POST", //Метод для отправки данных
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            Command: "getDir"
        })
    });
    // Если запрос прошел нормально
    if (response.ok) {
        // Получаем данные
        const responseData = await response.json();
        if (parseInt(appProps.isImageDir)) {
            await showDiskImages(responseData);
        }
        else {
            await showDirs(responseData);
        }
    };
};
async function showDirs(responseData) {
    responseData.forEach((dirName) => {
        const button = document.createElement("button");
        button.innerText = dirName;
        button.id = dirName;
        button.className = "button";
        button.addEventListener("click", async () => await getResponse(dirName, appProps.nextDir));
        document.getElementById("main").appendChild(button);
    });
}
async function showDiskImages(responseData) {
    responseData.forEach((dirName) => {
        const cell = document.createElement("div");
        cell.className = "cell-disk";

        const imgContent = document.createElement("div");
        imgContent.className = "cell-disk-content";

        const img = document.createElement("img");
        img.onload = function () { console.log("img.onload ok"); };           
        img.onerror = function () {
            img.src = appProps.webhost + "/images/" + appProps.diskImagePic;
        };
        img.src = appProps.webhost + appProps.url + "/" + dirName + "/" + appProps.diskImagePic;
        imgContent.appendChild(img);

        const textContent = document.createElement("div");
        textContent.className = "cell-disk-content";

        const name = document.createTextNode(dirName);
        textContent.appendChild(name);
        textContent.appendChild(document.createElement("br"));
        const link = document.createElement("a");
        //link.href = "ftp://195.46.162.215//Images//Education//Litra_ru//"
        link.href = appProps.ftpHost + appProps.url + "/" + dirName;
        link.text="Открыть папку в прводнике"
        textContent.appendChild(link);

        cell.appendChild(imgContent);
        cell.appendChild(textContent);
        document.getElementById("main").appendChild(cell);
    });
}
//При первом открытии страницы запрашиваем корневую папку
getResponse("\\", appProps.nextDir);
document.getElementById("btBack").addEventListener("click", async () => await getResponse("", appProps.prevDir));
