using Microsoft.Win32;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Windows.Media.Animation;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace WallpaperChanger;

public class FileManager
{
    public Dictionary<string, string> PathHash;
    public List<string> Files = new();
    public string path = "";
    public string output = "";
    public readonly string InputPath = "WP_Changer\\Input_Path.txt";
    public int FilesPerThread;
    public int NThreads;
    public Thread[] HashingThreads;

    public void BrowseFile()
    {
        OpenFolderDialog filedialog = new OpenFolderDialog();
        bool? Success = filedialog.ShowDialog();

        if (Success == true) {
            path = filedialog.FolderName;
        }
    }

    public void SavePath()
    {
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filename = Path.Combine(docs, InputPath);
        File.WriteAllText(filename, path);
    }

    public void LoadPath()
    {
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filename = Path.Combine(docs, InputPath);
        try {
            path = File.ReadAllText(filename);
        }
        catch (Exception) {
            BrowseFile();
            SavePath();
        }
    }

    public void GetFiles()
    {
        string[] files = Directory.GetFiles(path);
        string[] extensions = { ".png", ".jpeg", ".jpg" };
        Files.Clear();
        foreach (string file in files) {
            string ext = Path.GetExtension(file);
            if (extensions.Contains(ext)) {
                Files.Add(file);
            }
        }
    }

    public bool CheckFile()
    {
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filename = Path.Combine(docs, InputPath);
        if (File.Exists(filename)) {
            return true;
        }
        return false;
    }

    public void GetOutput()
    {
        int start = 0;
        for (int i = path.Length - 1; i >= 0; i--) {
            if (path[i] == '\\' || path[i] == '/') {
                start = i + 1;
                break;
            }
        }
        string Doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string name = path.Substring(start);
        string FinalOutPath = $"WP_Changer\\Thumbnails";
        output = Path.Combine(Doc, FinalOutPath);
    }

    public string GetHash(string filename)
    {
        using (var md5 = MD5.Create()) {
            using (var stream = File.OpenRead(filename)) {
                var hash = md5.ComputeHash(stream);
                string jj = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return jj;
            }
        }
    }

    public string GetKeyName(string file)
    {
        string ex = Path.GetExtension(file);
        string hash = GetHash(file);
        return hash + ex;
    }
    
    public void GetHashesHelper(List<string> files)
    {
        for (int i = 0; i < files.Count; i++) {
            string hash = GetKeyName(files[i]);
            lock (PathHash) {
                PathHash.Add(files[i], hash);
            }
        }
    }

    public void GetHashes()
    {
        PathHash = new();
        FilesPerThread = 50;
        NThreads = (int)Math.Ceiling((double)Files.Count / FilesPerThread);
        HashingThreads = new Thread[NThreads];

        for (int i = 0; i < NThreads; i++) {
            int start = i * FilesPerThread;
            int end = (i + 1) * FilesPerThread;
            if (end > Files.Count) end = Files.Count;
            
            HashingThreads[i] = new(() => GetHashesHelper(Files.Slice(start, end - start)));
            HashingThreads[i].IsBackground = true;
            HashingThreads[i].Start();
        }
    }

    public string GetThumbnail(string file, string output)
    {
        if (!Directory.Exists(output)) {
            Directory.CreateDirectory(output);
        }

        string keyname = PathHash[file];
        string outname = keyname + ".jpg";
        string thumbpath = Path.Combine(output, outname);

        if (!File.Exists(thumbpath)) {
            Size size = new(200, 133);
            using (Image image = Image.FromFile(file)) {
                using (Bitmap bm = new Bitmap(image, size)) {
                    bm.Save(thumbpath, ImageFormat.Jpeg);
                }
            }
        }

        return thumbpath;
    }

    public System.Windows.Controls.Image GetImage(string image_path)
    {
        Uri uri = new Uri(image_path);
        System.Windows.Controls.Image image = new()
        {
            Source = new BitmapImage(uri)
        };

        return image;
    }
}
