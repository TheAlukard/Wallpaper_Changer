using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace WallpaperChanger;

public static class Thumbnails
{
    public static string GetThumbnail(string file, string output)
    {
        if (! Directory.Exists(output)) {
            Directory.CreateDirectory(output);
        }
        
        string keyname = FileManaging.PathHash[file];
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
}
