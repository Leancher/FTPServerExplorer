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
        public string WebHost { get; set; } = "";
        public string FTPHost { get; set; } = "";
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
        public ResponseData() { }
        public ResponseData(string dirName, string command) { DirName = dirName; Command = command; }
    }
}
