﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static WallpaperChanger.FileManaging;

namespace WallpaperChanger;

struct ButtonWithFilename
{
    public Button button;
    public string filename;

    public ButtonWithFilename(Button b, string f)
    {
        button = b;
        filename = f;
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

    public void CreateButtons()
    {
        PhotoGrid.Children.Clear();
        buttons.Clear();
        foreach (string filename in PathHash.Keys)
        {
            Button button = new();
            button.Click += (e, s) => SetWallpaper.changeWallpaper(filename);
            button.Background = System.Windows.Media.Brushes.Transparent;
            ButtonWithFilename bwf = new(button, filename);
            buttons.Add(bwf);
            PhotoGrid.Children.Add(button);
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
        foreach (ButtonWithFilename bwf in buttons) {
            string thumbnail_path = Thumbnails.GetThumbnail(bwf.filename, output);
            Dispatcher.Invoke(() =>
            {
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

