using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace WallpaperChanger;

public static class FileManaging
{
    public static Dictionary<string, string> PathHash;
    public static List<string> Files = new();
    public static string path = "";
    public static string output = "";
    public static readonly string InputPath = "WP_Changer\\Input_Path.txt";
    public static int FilesPerThread;
    public static int NThreads;
    public static Thread[] HashingThreads;
    public static void BrowseFile()
    {
        OpenFolderDialog filedialog = new OpenFolderDialog();
        bool? Success = filedialog.ShowDialog();

        if (Success == true) {
            path = filedialog.FolderName;
        }
    }
    public static void SavePath()
    {
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filename = Path.Combine(docs, InputPath);
        File.WriteAllText(filename, path);
    }

    public static void LoadPath()
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

    public static void GetFiles()
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

    public static bool CheckFile()
    {
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filename = Path.Combine(docs, InputPath);
        if (File.Exists(filename)) {
            return true;
        }
        return false;
    }

    public static void GetOutput()
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

    public static string GetHash(string filename)
    {
        using (var md5 = MD5.Create()) {
            using (var stream = File.OpenRead(filename)) {
                var hash = md5.ComputeHash(stream);
                string jj = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return jj;
            }
        }
    }

    public static string GetKeyName(string file)
    {
        string ex = Path.GetExtension(file);
        string hash = GetHash(file);
        return hash + ex;
    }
    
    public static void GetKeyName(string file, out string result)
    {
        string ex = Path.GetExtension(file);
        string hash = GetHash(file);
        result = hash + ex;
    }

    public static void GetHashesHelper(List<string> files)
    {
        for (int i = 0; i < files.Count; i++) {
            string hash = GetKeyName(files[i]);
            lock (PathHash) {
                PathHash.Add(files[i], hash);
            }
        }
    }

    public static void GetHashes()
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
}
