using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace WallpaperChanger
{
    public static class FileManaging
    {
        public static Dictionary<string, string> PathHash;
        public static string[] Files;
        public static string path;
        public static string output;

        public static void SavePath()
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string name = "WP_Changer_Input_Path.json";
            string filename = Path.Combine(docs, name);
            string ser = JsonConvert.SerializeObject(path);
            File.WriteAllText(filename, ser);
        }

        public static void LoadPath()
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string name = "WP_Changer_Input_Path.json";
            string filename = Path.Combine(docs, name);
            string LoadedPath = File.ReadAllText(filename);
            path = JsonConvert.DeserializeObject<string>(LoadedPath);
        }

        public static void GetFiles()
        {
            var FilterList = new List<string>();
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                if (ext == ".png" || ext == ".jpeg" || ext == ".jpg")
                {
                    FilterList.Add(file);
                }
            }
            string[] FileList = FilterList.ToArray();
            Files = FileList;
        }

        public static bool CheckFile()
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string name = "WP_Changer_Input_Path.json";
            string filename = Path.Combine(docs, name);
            if (File.Exists(filename))
            {
                return true;
            }
            return false;
        }

        public static void GetOutput()
        {
            string foldername = "";
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '\\' || path[i] == '/')
                {
                    break;
                }
                foldername += path[i];
            }
            string Doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            char[] charArray = foldername.ToCharArray();
            Array.Reverse(charArray);
            string name = new string(charArray);
            name += "_Thumbnails";
            string FinalOutPath = $"WP_Thumbnails\\{name}";
            output = Path.Combine(Doc, FinalOutPath);
        }

        public static string GetHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
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

        public static void GetHashes()
        {
            PathHash = new();
            foreach (string file in Files)
            {
                string hash = GetKeyName(file);
                PathHash.Add(file, hash);
            }
        }

        public static void BrowseFile()
        {
            OpenFolderDialog filedialog = new OpenFolderDialog();
            bool? Success = filedialog.ShowDialog();

            if (Success == true)
            {
                path = filedialog.FolderName;
            }
        }

    }
}
