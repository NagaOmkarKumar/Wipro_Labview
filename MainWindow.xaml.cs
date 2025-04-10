using System.Text;
using System.Windows;
using System;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Specialized;
using SQLite;

namespace Wipro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SQLiteConnection connection = new SQLiteConnection(App.databasePath);
        private DispatcherTimer _timer;
       // LabView1 _labView1;
        //CCVandAngle_RunningMode cCVandAngle_RunningMode;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //DataContext = _labView1;
            //DataContext = cCVandAngle_RunningMode;
            connection.CreateTable<FilePath>();
            connection.CreateTable<LabViewData>();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Update every second
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateDateTime();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        //[RelayCommand]

        private void UpdateDateTime()
        {
            date.Text = DateTime.Now.ToString("dd-MM-yyyy");
            time.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DynamicContentArea.Content = new LabView1();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DynamicContentArea.Content = new LabView1();
        }

        private void ScreenList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ScreenList.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                string selectedScreen = selectedItem.Content.ToString();

                // Display the corresponding UserControl based on selected screen
                switch (selectedScreen)
                {
                    case "MACHINE 1":
                        LabView1 labView1 = new LabView1();
                        labView1.DataContext = labView1;  // Set the DataContext for LabView1
                        DynamicContentArea.Content = labView1;
                        //DataContext = LabView1();
                        break;

                    case "MACHINE 2":
                        //DynamicContentArea.Content = new CCVandAngle_RunningMode();
                        CCVandAngle_RunningMode cCVandAngle_RunningMode = new CCVandAngle_RunningMode();
                        cCVandAngle_RunningMode.DataContext = cCVandAngle_RunningMode;
                        DynamicContentArea.Content = cCVandAngle_RunningMode;
                        break;

                    case "HOME":
                        DynamicContentArea.Content = null;
                        Image homeImage = new Image();
                        homeImage.Source = new BitmapImage(new Uri("assets/WiproImage.jpg", UriKind.Relative));

                        
                        DynamicContentArea.Content = homeImage;
                        break;

                    // Add more cases for additional screens
                    default:
                        //DynamicContentArea.Content = new DefaultUserControl(); // Or clear content
                        break;
                }
            }
        }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            GenerateReport generateReport = new GenerateReport();
            generateReport.Show();
        }
    }
}
