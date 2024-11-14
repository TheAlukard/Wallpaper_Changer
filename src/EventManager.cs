using System.Collections.Specialized;
using System.Drawing.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace WallpaperChanger;

public class EventManager : Window
{
    MainWindow main_window;
    FileManager file_manager;

    public EventManager(MainWindow window, FileManager filemanager)
    {
        main_window = window;
        file_manager = filemanager;
    }

    public void DragAndDrop(Object sender, MouseEventArgs e, string filename)
    {
        if (e.RightButton != MouseButtonState.Pressed) return;

        DataObject data = new();
        data.SetFileDropList(new() { filename });
        DragDrop.DoDragDrop(main_window, data, DragDropEffects.Copy);
    }

    public void GetDropped(object sender, DragEventArgs e, Func<string , Button> CreateAButton, Action AdjustBtns)
    {
        DataObject data = (DataObject)e.Data;
        StringCollection files = data.GetFileDropList();

        foreach (string file in files) {
            if (file_manager.PathHash.ContainsKey(file)) continue;

            file_manager.Files.Add(file);
            file_manager.PathHash.Add(file, file_manager.GetHash(file));
            CreateAButton(file).Content = file_manager.GetImage(file_manager.GetThumbnail(file, file_manager.output));
            AdjustBtns();
        }
    }

    public void ShowProperties(object sender, MouseEventArgs e, string file, TextBlock tb)
    {
        Button button = sender as Button;

        button.Background = System.Windows.Media.Brushes.LightGray;

        if (tb.Text == "") {
            FileInfo info = new(file);
            BitmapSource img;

            using (FileStream fs = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                img = BitmapFrame.Create(fs);
            }

            const int padding = 10;
            tb.Text = $"res:    {img.PixelWidth} x {img.PixelHeight}\n" +
                       $"name:   {info.Name}\n" +
                       $"format: {info.Extension.Substring(1)}\n" +
                       $"size:   {(((double)info.Length / (1024 * 1024))).ToString("#0.000")} MB\n";
            tb.Background = System.Windows.Media.Brushes.Transparent;
            tb.FontFamily = new("Cascadia Code Mono");
            tb.FontSize = 11;
            tb.Width = button.ActualWidth - padding;
            tb.Height = button.ActualHeight - padding;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.TextAlignment = TextAlignment.Left;

            tb.MouseRightButtonDown += (s, e) => HideProperties(button, file);
        }

        button.Content = tb;
    }

    public void HideProperties(Button button, string file)
    {
        button.Content = file_manager.GetImage(file_manager.GetThumbnail(file, file_manager.output));
        button.Background = System.Windows.Media.Brushes.Transparent;
    }
}