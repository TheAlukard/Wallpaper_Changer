using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static WallpaperChanger.FileManaging;

namespace WallpaperChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {  
        static bool failedload = false;
        Dictionary<string, Button> buttons;       
        delegate void SetBG(string filename);
        bool CycleMode = false;
        int index = 0;

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
            catch (Exception et) {
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
                text.Click += delegate { SetWallpaper.changeWallpaper(filename); };
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

        public async void LoadImages()
        { 
            foreach (var file in PathHash.Keys) {
                string pp = "";
                await Task.Run(() => pp = Thumbnails.GetThumbs(file, output));
                buttons[file].Content = GetTheImage(pp);         
            }
        }

        public void AdjustBtns()
        {

           while (true)
            {

                Thread.Sleep(100);

                int ActW = Convert.ToInt32(Math.Ceiling(TheApp.ActualWidth));                           
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
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                }
                
                this.Dispatcher.Invoke(() =>
                {
                    PhotoGrid.Columns = numC;
                    PhotoGrid.Rows = numR + 1;                                                  
                });
                
            }
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
            if (! failedload)
            {
                GetHashes();
                CreateButtons();
                LoadImages();
            }
            Thread TH1 = new Thread(AdjustBtns);
            TH1.IsBackground = true;
            TH1.Start();
        }

        private void CycleR_Click(object sender, RoutedEventArgs e)
        {
            index++;
            if (index < Files.Length) SetWallpaper.changeWallpaper(Files[index]);
            else SetWallpaper.changeWallpaper(Files[Files.Length - 1]);
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
    
}