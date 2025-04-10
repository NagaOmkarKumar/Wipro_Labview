using Google.Cloud.Firestore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LiveChartsCore.Measure;

namespace Wipro
{
    /// <summary>
    /// Interaction logic for LabView1.xaml
    /// </summary>
    public partial class LabView1 : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private readonly Random _random = new Random();
        private int _value1, _value2, _value3, _value4, _formatter;
        public int Value1
        {
            get => _value1;
            set
            {
                _value1 = value;
                OnPropertyChanged(nameof(Value1));
            }
        }
        public int Value2
        {
            get => _value2;
            set
            {
                _value2 = value;
                OnPropertyChanged(nameof(Value2));
            }
        }
        public int Value3
        {
            get => _value3;
            set
            {
                _value3 = value;
                OnPropertyChanged(nameof(Value3));
            }
        }
        public int Value4
        {
            get => _value4;
            set
            {
                _value4 = value;
                OnPropertyChanged(nameof(Value4));
            }
        }
        public int Formatter
        {
            get => _formatter;
            set
            {
                _formatter = value;
                OnPropertyChanged(nameof(Formatter));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        List<int> ints = new List<int>();
        private static readonly SKColor s_blue = new(25, 118, 210);
        private static readonly SKColor s_red = new(229, 57, 53);
        private static readonly SKColor s_yellow = new(198, 167, 0);
        private static readonly SKColor s_green = new(170, 22, 200);
        private static readonly SKColor s_white = new(255, 120, 0);
        private readonly HttpClient _httpClient;
        private FirebaseService _firebaseService;
        private static FirestoreDb _firestoreDb;
        SQLiteConnection connection = new SQLiteConnection(App.databasePath);

        public LabView1()
        {
            InitializeComponent();

            DataContext = this;
            _httpClient = new HttpClient();
            _firebaseService = new FirebaseService();
            _firestoreDb = FirebaseService.GetFirestoreDb();
            FirestoreDb firestoreDb = FirebaseService.GetFirestoreDb();
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateDateTime();
            InitializeChartSeries();
            UpdateChartData();
            
        }
        private void InitializeChartSeries()
        {
            Series = new ISeries[]
            {
        new LineSeries<double>
        {
            LineSmoothness = 1,
            Name = "Pressure 1",
            Values = new ObservableCollection<double>(),
            Stroke = new SolidColorPaint(s_blue, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_blue, 2),
            Fill = null,
            ScalesYAt = 0
        },
        new LineSeries<double>
        {
            Name = "Pressure 2",
            Values = new ObservableCollection<double>(),
            Stroke = new SolidColorPaint(s_yellow, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_yellow, 2),
            Fill = null,
            ScalesYAt = 0
        },
        new LineSeries<double>
        {
            LineSmoothness = 1,
            Name = "Pressure 3",
            Values = new ObservableCollection<double>(),
            Stroke = new SolidColorPaint(s_white, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_white, 2),
            Fill = null,
            ScalesYAt = 0
        },
        new LineSeries<double>
        {
            Name = "Pressure 4",
            Values = new ObservableCollection<double>(),
            Stroke = new SolidColorPaint(s_green, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_green, 2),
            Fill = null,
            ScalesYAt = 0
        },
        new LineSeries<double>
        {
            Name = "Temperature",
            Values = new ObservableCollection<double>(),
            Stroke = new SolidColorPaint(s_red, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_red, 2),
            Fill = null,
            ScalesYAt = 1
        }
            };

            YAxes = new Axis[]
            {
        new Axis
        {
            Name = "Pressure",
            NameTextSize = 14,
            NamePaint = new SolidColorPaint(s_blue),
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 20),
            Padding = new LiveChartsCore.Drawing.Padding(0, 0, 20, 0),
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(s_blue),
            TicksPaint = new SolidColorPaint(s_blue),
            SubticksPaint = new SolidColorPaint(s_blue),
            DrawTicksPath = true
        },
        new Axis
        {
            Name = "Temperature",
            NameTextSize = 14,
            NamePaint = new SolidColorPaint(s_red),
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 20),
            Padding = new LiveChartsCore.Drawing.Padding(20, 0, 0, 0),
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(s_red),
            TicksPaint = new SolidColorPaint(s_red),
            SubticksPaint = new SolidColorPaint(s_red),
            DrawTicksPath = true,
            ShowSeparatorLines = false,
            Position = AxisPosition.End
        }
            };

            LegendTextPaint = new SolidColorPaint
            {
                Color = new SKColor(50, 50, 50),
                SKTypeface = SKTypeface.FromFamilyName("Courier New")
            };
        }

        public static int GenerateRandomInteger(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue);
        }

        public ISeries[] Series { get; set; }
        public ICartesianAxis[] YAxes { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; } =
            new SolidColorPaint
            {
                Color = new SKColor(50, 50, 50),
                SKTypeface = SKTypeface.FromFamilyName("Courier New")
            };

        public SolidColorPaint LedgendBackgroundPaint { get; set; } =
            new SolidColorPaint(new SKColor(240, 240, 240));

       
        public void UpdateChartData()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                int pressure1Value = GenerateRandomInteger(0, 400); 
                int pressure2Value = GenerateRandomInteger(0, 400);
                int pressure3Value = GenerateRandomInteger(0, 400);
                int pressure4Value = GenerateRandomInteger(0, 400); 
                int temperatureValue = GenerateRandomInteger(0, 100);

                AddDataToSeries(Series[0], pressure1Value); 
                AddDataToSeries(Series[1], pressure2Value);
                AddDataToSeries(Series[2], pressure3Value);
                AddDataToSeries(Series[3], pressure4Value);
                AddDataToSeries(Series[4], temperatureValue);

                OnPropertyChanged(nameof(Series)); 

                Value1 = pressure1Value;
                Value2 = pressure2Value;
                Value3 = pressure3Value;
                Value4 = pressure4Value;
                Formatter = temperatureValue;

                SaveToDatabase(pressure1Value, pressure2Value, pressure3Value, pressure4Value, temperatureValue);
            }));
        }

        private void AddDataToSeries(ISeries series, int value)
        {
            if (series.Values is ObservableCollection<double> observableValues)
            {
                observableValues.Add(value);
                if (observableValues.Count > 25)
                {
                    observableValues.RemoveAt(0);
                }
            }
            else if (series.Values is List<double> listValues)
            {
                listValues.Add(value);
                if (listValues.Count > 25)
                {
                    listValues.RemoveAt(0);
                }
            }
        }

        private void SaveToDatabase(int pressure1, int pressure2, int pressure3, int pressure4, int temperature)
        {
            LabViewData labViewData = new LabViewData()
            {
                Temperature = temperature,
                Pressure4 = pressure4,
                Pressure3 = pressure3,
                Pressure1 = pressure1,
                Pressure2 = pressure2,
                Timestamp = DateTime.Now.ToString("dd-MM-yyyy")
            };
            connection.Insert(labViewData);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateChartData();
            //UpdateDateTime();

        }

        //[RelayCommand]

        private void UpdateDateTime()
        {
            //date.Text = DateTime.Now.ToString("dd-MM-yyyy");
            //time.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await _firebaseService.DownloadDataFromFirestore();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GenerateReport generateReport = new GenerateReport();
            generateReport.Show();
        }

        public void UpdateChartData1()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                int pressure1Value = GenerateRandomInteger(0, 400); // Pressure 1
                int pressure2Value = GenerateRandomInteger(0, 400); // Pressure 2
                int pressure3Value = GenerateRandomInteger(0, 400); // Pressure 3
                int pressure4Value = GenerateRandomInteger(0, 400); // Pressure 4
                int temperatureValue = GenerateRandomInteger(0, 100); // Temperature

                if (Series[0].Values is ObservableCollection<double> series0Values)
                {
                    series0Values.Add(pressure1Value);
                    if (series0Values.Count > 25) series0Values.RemoveAt(0);
                }
                else if (Series[0].Values is List<double> series0List)
                {
                    series0List.Add(pressure1Value);
                    if (series0List.Count > 25) series0List.RemoveAt(0);
                }
                else
                {
                    //Trace.WriteLine("Series[0].Values is neither ObservableCollection<double> nor List<double>");
                    return;
                }

                if (Series[1].Values is ObservableCollection<double> series1Values)
                {
                    series1Values.Add(pressure2Value);
                    if (series1Values.Count > 25) series1Values.RemoveAt(0);
                }
                else if (Series[1].Values is List<double> series1List)
                {
                    series1List.Add(pressure2Value);
                    if (series1List.Count > 25) series1List.RemoveAt(0);
                }

                if (Series[2].Values is ObservableCollection<double> series2Values)
                {
                    series2Values.Add(pressure3Value);
                    if (series2Values.Count > 25) series2Values.RemoveAt(0);
                }
                else if (Series[2].Values is List<double> series2List)
                {
                    series2List.Add(pressure3Value);
                    if (series2List.Count > 25) series2List.RemoveAt(0);
                }

                if (Series[3].Values is ObservableCollection<double> series3Values)
                {
                    series3Values.Add(pressure4Value);
                    if (series3Values.Count > 25) series3Values.RemoveAt(0);
                }
                else if (Series[3].Values is List<double> series3List)
                {
                    series3List.Add(pressure4Value);
                    if (series3List.Count > 25) series3List.RemoveAt(0);
                }

                if (Series[4].Values is ObservableCollection<double> series4Values)
                {
                    series4Values.Add(temperatureValue);
                    if (series4Values.Count > 25) series4Values.RemoveAt(0);
                }
                else if (Series[4].Values is List<double> series4List)
                {
                    series4List.Add(temperatureValue);
                    if (series4List.Count > 25) series4List.RemoveAt(0);
                }

                OnPropertyChanged(nameof(Series));

                Value1 = pressure1Value;
                Value2 = pressure2Value;
                Value3 = pressure3Value;
                Value4 = pressure4Value;
                Formatter = temperatureValue;

                LabViewData labViewData = new LabViewData()
                {
                    Temperature = temperatureValue,
                    Pressure4 = pressure4Value,
                    Pressure3 = pressure3Value,
                    Pressure1 = pressure1Value,
                    Pressure2 = pressure2Value,
                    Timestamp = DateTime.Now.ToString("dd-MM-yyyy")

                };
                connection.Insert(labViewData);

                //await _firebaseService.UploadDataToFirebase(pressure1Value, pressure2Value, pressure3Value, pressure4Value, temperatureValue);
                //await _firebaseService.StoreToFirestore(pressure1Value, pressure2Value, pressure3Value, pressure4Value, temperatureValue);
                //Trace.WriteLine(Value1); Trace.WriteLine(Value2); Trace.WriteLine(Value3); Trace.WriteLine(Value4); Trace.WriteLine(Formatter);
            }));
        }
    }
}
