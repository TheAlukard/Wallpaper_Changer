using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using WallpaperChanger;


public static class Thumbnails
{

    public static Dictionary<string, string> Dict = new Dictionary<string, string>();

    public static string GetThumbs(string inp, string outp)
    {

        var file = inp;

        if (! Directory.Exists(outp))
        {
            Directory.CreateDirectory(outp);
        }
        
        string keyname = FileManaging.PathHash[file];

        int index = keyname.LastIndexOf('.');

        string pname = keyname.Remove(index, 1);

        string outname = pname + ".jpg";

        string thumbpath = Path.Combine(outp, outname);


        if (! Dict.ContainsKey(keyname))
        {
            Dict.Add(keyname, thumbpath);
        }           

        if (! File.Exists(thumbpath))
        {
            Image image = Image.FromFile(file);
            Bitmap bm = new Bitmap(image, new Size(200, 113));
            bm.Save(thumbpath, ImageFormat.Jpeg);               
        }

        return thumbpath;
            
    }

}