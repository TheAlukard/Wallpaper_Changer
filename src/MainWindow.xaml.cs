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


public partial class MainWindow : Window
{
    Dictionary<string, Button> buttons = new();
    FileManager file_manager = new();
    EventManager events;
    bool CycleMode = false;
    int index = 0;
    double width;
    double height;

    public MainWindow()
    {
        InitializeComponent();

        Closing += (e, s) => Application.Current.Shutdown();
        startup();                  
    }

    public void startup()
    {
        if (! file_manager.CheckFile()) {
            file_manager.BrowseFile();
            file_manager.SavePath();
        }
        else {
            file_manager.LoadPath();
        }

        events = new(this, file_manager);
    }

    public void GetDropped(object sender, DragEventArgs e)
    {
        events.GetDropped(sender, e, CreateAButton, AdjustBtns);
    }

    public Button CreateAButton(string file)
    {
        Button button = new();
        button.Click += (s, e) => SetWallpaper.changeWallpaper(file);
        button.MouseMove += (s, e) => events.DragAndDrop(s, e, file);
        button.Background = System.Windows.Media.Brushes.Transparent;
        button.PreviewMouseRightButtonDown += (s, e) => events.ShowProperties(s, e, file, new());
        button.PreviewMouseRightButtonUp += (s, e) => events.HideProperties(button, file);

        lock (buttons) {
            buttons.Add(file, button);
        }

        PhotoGrid.Children.Add(button);

        return button;
    }

    public void LoadAnImage(Button button, string file)
    {
        string thumbnail_path = file_manager.GetThumbnail(file, file_manager.output);
        Dispatcher.Invoke(() => button.Content = file_manager.GetImage(thumbnail_path));
    }

    public void setup_helper(List<string> files)
    {
        Dispatcher.Invoke(() => {
            lock (PhotoGrid.Children) {
                for (int i = 0; i < files.Count; i++) {
                    CreateAButton(files[i]);
                }
            }
            AdjustBtns();
        });
        
        for (int i = 0; i < files.Count; i++) {
            LoadAnImage(buttons[files[i]], files[i]);
        }
    }

    public void setup()
    {
        PhotoGrid.Children.Clear();
        buttons.Clear();

        for (int i = 0; i < file_manager.NThreads; i++) {
            int start = i * file_manager.FilesPerThread;
            int end = (i + 1) * file_manager.FilesPerThread;
            if (end > file_manager.Files.Count) end = file_manager.Files.Count;

            Thread t = new(() => setup_helper(file_manager.Files.Slice(start, end - start)));
            t.IsBackground = true;
            file_manager.HashingThreads[i].Join();
            t.Start();
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
        file_manager.BrowseFile();
        file_manager.SavePath();
        GetReady();
    }

    private void RefreshFiles_Click(object sender, RoutedEventArgs e)
    {
        GetReady();
    }

    public void GetReady()
    {
        file_manager.GetFiles();
        file_manager.GetOutput();
        file_manager.GetHashes();
        setup();
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
        if (index < file_manager.Files.Count) SetWallpaper.changeWallpaper(file_manager.Files[index]);
        else SetWallpaper.changeWallpaper(file_manager.Files[file_manager.Files.Count - 1]);
    }

    private void CycleL_Click(object sender, RoutedEventArgs e)
    {
        index--;
        if (index >= 0) SetWallpaper.changeWallpaper(file_manager.Files[index]);
        else SetWallpaper.changeWallpaper(file_manager.Files[0]);
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

