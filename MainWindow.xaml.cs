using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static WallpaperChanger.FileManaging;

namespace WallpaperChanger;

public partial class MainWindow : Window
{  
    static bool failedload = false;
    Dictionary<string, Button> buttons;       
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

        GetOutput();

        try {
            GetFiles();
        }
        catch (Exception) {
            failedload = true;
        }
    }

    public void GetReady()
    {
        GetFiles();
        GetOutput();
        GetHashes();
        CreateButtons();
        LoadImages();
    }

    public void CreateButtons()
    {
        buttons = new();
        PhotoGrid.Children.Clear();
        foreach (var filename in PathHash.Keys)
        {
            Button text = new Button();
            text.Click += (e, s) => SetWallpaper.changeWallpaper(filename);
            PhotoGrid.Children.Add(text);
            text.Background = System.Windows.Media.Brushes.Transparent;
            buttons.Add(filename, text);
        }
    }  

    public Image GetTheImage(string thep)
    {
        Uri uri = new Uri(thep);
        Image image = new Image
        {
            Source = new BitmapImage(uri)
        };
        return image;
    }

    public void LoadImages()
    { 
        foreach (var file in PathHash.Keys) {
            string pp = Thumbnails.GetThumbs(file, output);
            buttons[file].Content = GetTheImage(pp);         
        }
    }

    public void AdjustBtns()
    {
        width = TheApp.ActualWidth;
        height = TheApp.ActualHeight;
        int ActW = Convert.ToInt32(Math.Ceiling(width));                           
        int numC = 2;
        int numR = 50;

        try
        { 
            numC =  ActW / 180 ;
            if (numC < 2)
            {
                numC = 2;
            }
            numR = buttons.Count / numC;
        }
        catch (Exception)
        {
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

    private void TheApp_Loaded(object sender, RoutedEventArgs e)
    {
        if (failedload) return;

        GetHashes();
        CreateButtons();
        LoadImages();
        AdjustBtns();
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
        if (CycleMode)
        {
            ThePhotos.Visibility = Visibility.Visible;
            Cycler.Visibility = Visibility.Hidden;
            CycleMode = false;
        }
        else
        {         
            ThePhotos.Visibility = Visibility.Hidden;
            Cycler.Visibility = Visibility.Visible;
            CycleMode = true;
        }
    }
}  

