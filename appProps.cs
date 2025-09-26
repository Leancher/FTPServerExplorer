namespace MyFTPserverExplorer
{
    public class AppProps
    {
        //ПУть к текущей папке, из которой будет браться список папок
        public string CurDirPath { get; set; } = "";
        //Имя файла-индетификатора того, что дальше в папках образы дисков
        public string ImageDirFile { get; set; } = "";
        //Имя картинки с этикеткой диска
        public string DiskImagePic { get; set; } = "";
        //Дальше папка с образами дисков или нет: 1 или 0
        public string IsImageDir { get; set; } = "";
        //Адрес сайта
        public string WebHost { get; set; } = "";
        //Адрес ФТП-сервера
        public string FTPHost { get; set; } = "";
        //Корневая папка с дисками
        public string FileHost { get; set; } = "";
        //Путь к папке с чертой "/"
        public string URL { get; set; } = "";
        public AppProps() { }
        public AppProps(string curDirPath, string imageDirFile, string diskImagePic, string isImageDir)
        {
            CurDirPath = curDirPath;
            ImageDirFile = imageDirFile;
            DiskImagePic = diskImagePic;
            IsImageDir = isImageDir;
        }
    }

    public class ResponseData
    {
        public string DirName { get; set; } = "";
        public string Command { get; set; } = "";
        public string Direction { get; set; } = "";
        public ResponseData() { }
        public ResponseData(string dirName, string command, string direction) 
        {
            DirName = dirName;
            Command = command;
            Direction = direction;
        }
    }
}
