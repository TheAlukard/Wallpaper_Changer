using System.Collections.Specialized;
using System.Drawing.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static WallpaperChanger.FileManaging;

namespace WallpaperChanger;

public struct ButtonWithFilename
{
    public Button button;
    public string filename;
    public TextBlock text_block;

    public ButtonWithFilename(Button b, string f)
    {
        button = b;
        filename = f;
        text_block = new();
        text_block.Text = "";
    }
}

public partial class MainWindow : Window
{
    List<ButtonWithFilename> buttons = new();       
    bool CycleMode = false;
    int index = 0;
    double width;
    double height;

    public MainWindow()
    {
        InitializeComponent();

        startup();                   
    }

    public void startup()
    {
        if (! CheckFile()) {
            BrowseFile();
            SavePath();
        }
        else {
            LoadPath();
        }
    }

    public Button CreateAButton(string file)
    {
        Button button = new();
        button.Click += (s, e) => SetWallpaper.changeWallpaper(file);
        button.MouseMove += (s, e) => DragAndDrop(s, e, file);
        button.Background = System.Windows.Media.Brushes.Transparent;
        ButtonWithFilename bwf = new(button, file);
        button.PreviewMouseRightButtonDown += (s, e) => ShowProperties(s, e, file, bwf.text_block);
        button.PreviewMouseRightButtonUp += (s, e) => HideProperties(button, file); 
        buttons.Add(bwf);
        PhotoGrid.Children.Add(button);

        return button;
    }

    public void CreateButtons()
    {
        PhotoGrid.Children.Clear();
        buttons.Clear();
        foreach (string file in Files) {
            CreateAButton(file);
        }
    }

    public void DragAndDrop(Object sender, MouseEventArgs e, string filename)
    {
        if (e.RightButton != MouseButtonState.Pressed) return;

        DataObject data = new();
        data.SetFileDropList(new() { filename });
        DragDrop.DoDragDrop(TheApp, data, DragDropEffects.Copy);
    }

    public void GetDropped(object sender, DragEventArgs e)
    {
        DataObject data = (DataObject)e.Data;
        StringCollection files = data.GetFileDropList();

        foreach (string file in files) {
            if (PathHash.ContainsKey(file)) continue;

            Files.Add(file);
            PathHash.Add(file, GetHash(file));
            CreateAButton(file).Content = GetTheImage(Thumbnails.GetThumbnail(file, output));
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
            tb.Text =  $"res:    {img.PixelWidth} x {img.PixelHeight}\n" +
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
        button.Content = GetTheImage(Thumbnails.GetThumbnail(file, output));
        button.Background = System.Windows.Media.Brushes.Transparent;
    }

    public Image GetTheImage(string image_path)
    {
        Uri uri = new Uri(image_path);
        Image image = new Image {
            Source = new BitmapImage(uri)
        };
        return image;
    }

    public void LoadImages()
    { 
        foreach (ButtonWithFilename bwf in buttons) {
            string thumbnail_path = Thumbnails.GetThumbnail(bwf.filename, output);
            Dispatcher.Invoke(() => {
                bwf.button.Content = GetTheImage(thumbnail_path);
            }); 
        }
    }

    public void AdjustBtns()
    {
        width = TheApp.ActualWidth;
        height = TheApp.ActualHeight;
        int ActW = Convert.ToInt32(Math.Ceiling(width));
        int numC = 2;
        int numR = 50;

        try { 
            numC =  ActW / 180 ;
            if (numC < 2)
            {
                numC = 2;
            }
            numR = buttons.Count / numC;
        }
        catch (Exception) {
            return;
        }
            
        PhotoGrid.Columns = numC;
        PhotoGrid.Rows = numR + 1;
    }

    private void FileDalog_Click(object sender, RoutedEventArgs e)
    {
        BrowseFile();
        SavePath();
        GetReady();
    }

    private void RefreshFiles_Click(object sender, RoutedEventArgs e)
    {
        GetReady();
    }

    public void GetReady()
    {
        GetFiles();
        GetOutput();
        GetHashes();
        CreateButtons();
        Thread t = new(() => LoadImages());
        t.Start();
        AdjustBtns();
    }

    private void TheApp_Loaded(object sender, RoutedEventArgs e)
    {
        GetReady();
        TheApp.SizeChanged += (e, s) => AdjustBtns();
    }

    private void CycleR_Click(object sender, RoutedEventArgs e)
    {
        index++;
        if (index < Files.Count) SetWallpaper.changeWallpaper(Files[index]);
        else SetWallpaper.changeWallpaper(Files[Files.Count - 1]);
    }

    private void CycleL_Click(object sender, RoutedEventArgs e)
    {
        index--;
        if (index >= 0) SetWallpaper.changeWallpaper(Files[index]);
        else SetWallpaper.changeWallpaper(Files[0]);
    }

    private void ToggleMode_Click(object sender, RoutedEventArgs e)
    {
        if (CycleMode) {
            ThePhotos.Visibility = Visibility.Visible;
            Cycler.Visibility = Visibility.Hidden;
            CycleMode = false;
        }
        else {         
            ThePhotos.Visibility = Visibility.Hidden;
            Cycler.Visibility = Visibility.Visible;
            CycleMode = true;
        }
    }
}  

