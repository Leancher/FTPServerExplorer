namespace MyFTPserverExplorer
{
    public class AppProps
    {
        public string RootDir { get; set; } = "";
        public string PicDir { get; set; } = "";
        public string ImageDirFile { get; set; } = "";
        public string ImagePic { get; set; } = "";
        public AppProps() { }
        public AppProps(string rootDir, string picDir, string imageDirFile, string imagePic)
        {
            this.RootDir = rootDir;
            this.PicDir = picDir;
            this.ImageDirFile = imageDirFile;
            this.ImagePic = imagePic;
        }
    }

    public class Dir
    {
        public string Name { get; set; } = "";
        public string Test { get; set; } = "";
        public Dir() { }
        public Dir(string name, string test) { Name = name; Test = test; }
    }
}
