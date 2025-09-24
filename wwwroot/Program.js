//Список настроек:
//Путь к текущей папке
//Имя файла с картинкой диска
//Папки или диски?
//Адрес сайта
//Адрес ФТП-севера
var appProps = {
    curDirPath: "",
    diskImagePic: "Cover 00.jpg",
    isImageDir: "0",
    webhost: "",
    ftpHost: "ftp:///192.168.0.127//",
}

async function getResponse(dirName, command) {
    await getProps(dirName, command);
    await getDirsList(dirName, command);
}

async function getProps(dirName) {
    const response = await fetch("/", {
        method: "POST", //Метод для отправки данных
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            DirName: dirName, //Записываем данные
            Command: "props"
        })
    });
    if (response.ok) {
        const responseData = JSON.parse(await response.json());
        appProps.curDirPath = responseData.CurDirPath;
        appProps.diskImagePic = responseData.DiskImagePic;
        appProps.isImageDir = responseData.IsImageDir;
        appProps.webhost =  responseData.WebHost;
        appProps.ftpHost = responseData.FTPHost;
    }
}

async function getDirsList(dirName, command) {
    //Стираем содержимое перед обновлением
    document.getElementById("main").innerHTML = "";

    // отправляет запрос и получаем ответ
    const response = await fetch("/", {
        method: "POST", //Метод для отправки данных
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            DirName: dirName, //Записываем данные
            Command: command
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
        button.addEventListener("click", async () => await getResponse(dirName, "getDir"));
        document.getElementById("main").appendChild(button);
    });
}
async function showDiskImages(responseData) {
    responseData.forEach((dirName) => {
        const cell = document.createElement("div");
        const img = document.createElement("img");
        img.src = appProps.webhost + "/" + appProps.curDirPath + "/" + dirName + "/" + appProps.diskImagePic;
        cell.appendChild(img);
        document.getElementById("main").appendChild(cell);
    });
}
//При первом открытии страницы запрашиваем корневую папку
getResponse("/", "root");
document.getElementById("btBack").addEventListener("click", async () => await getResponse("", "back"));
