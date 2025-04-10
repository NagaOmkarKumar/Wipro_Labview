using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.VisualElements;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Google.Cloud.Firestore;
using System.Net.Http;
using SQLite;

namespace Wipro
{
    /// <summary>
    /// Interaction logic for LabView.xaml
    /// </summary>
    public partial class LabView : Window, INotifyPropertyChanged
    {
        private SerialPort serialPort;
        private StringBuilder _sb = new StringBuilder();
        FlowDocument mcFlowDoc = new FlowDocument();
        Paragraph para = new Paragraph();
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

        public LabView()
        {
            InitializeComponent();
            DataContext = this;
            _httpClient = new HttpClient();
            _firebaseService = new FirebaseService();
            _firestoreDb = FirebaseService.GetFirestoreDb();
            FirestoreDb firestoreDb = FirebaseService.GetFirestoreDb();
            connection.CreateTable<FilePath>();
            connection.CreateTable<LabViewData>();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Update every second
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateDateTime();
            UpdateChartData();
        }

        public List<int> GenerateArithmeticSeries(int startValue, int numberOfTerms = 25, int commonDifference = 10)
        {
            List<int> series = new List<int>();

            for (int i = 0; i < numberOfTerms; i++)
            {
                series.Add(startValue + i * commonDifference);
            }

            return series;
        }

        public static int GenerateRandomInteger(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue);  // Returns a random integer between minValue (inclusive) and maxValue (exclusive)
        }
        
        public ISeries[] Series { get; set; } = new ISeries[]
            {
            new LineSeries<double>
        {
            LineSmoothness = 1,
            Name = "Pressure 1",
            Values = new List<double> (),
            Stroke = new SolidColorPaint(s_blue, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_blue, 2),
            Fill = null,
            ScalesYAt = 0
        },
        new LineSeries<double>
        {
            Name = "Pressure 2",
            Values = new List<double> (),
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
            Values = new List<double> (),
            Stroke = new SolidColorPaint(s_white, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_white, 2),
            Fill = null,
            ScalesYAt = 0 
        },
        new LineSeries<double>
        {
            Name = "Pressure 4",
            Values = new List<double> (),
            Stroke = new SolidColorPaint(s_green, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_green, 2),
            Fill = null,
            ScalesYAt = 0 
        },
        new LineSeries<double>
        {
            Name = "Temperature",
            Values = new List<double>(),
            Stroke = new SolidColorPaint(s_red, 2),
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(s_red, 2),
            Fill = null,
            ScalesYAt = 1  
        },
               
            };

        public ICartesianAxis[] YAxes { get; set; } = [
            new Axis 
        {
            Name = "Pressure",
            NameTextSize = 14,
            NamePaint = new SolidColorPaint(s_blue),
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 20),
            Padding =  new LiveChartsCore.Drawing.Padding(0, 0, 20, 0),
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
            Padding =  new LiveChartsCore.Drawing.Padding(20, 0, 0, 0),
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(s_red),
            TicksPaint = new SolidColorPaint(s_red),
            SubticksPaint = new SolidColorPaint(s_red),
            DrawTicksPath = true,
            ShowSeparatorLines = false,
            Position = AxisPosition.End
        }
       
        ];

        public SolidColorPaint LegendTextPaint { get; set; } =
            new SolidColorPaint
            {
                Color = new SKColor(50, 50, 50),
                SKTypeface = SKTypeface.FromFamilyName("Courier New")
            };

        public SolidColorPaint LedgendBackgroundPaint { get; set; } =
            new SolidColorPaint(new SKColor(240, 240, 240));

        public async void UpdateChartData()
        {
            // Trace.WriteLine("ChartValues");
            //call in timer
            // int ra = GenerateRandomInteger(1, 10000);
            // ints = GenerateArithmeticSeries(ra, 25, 10);

            // Value1 = GenerateRandomInteger(0, 400);
            // Value2 = GenerateRandomInteger(0, 400);
            // Value3 = GenerateRandomInteger(0, 400);
            // Value4 = GenerateRandomInteger(0, 400);
            // Formatter = GenerateRandomInteger(0, 100);
            // Trace.WriteLine(Value1); Trace.WriteLine(Value2); Trace.WriteLine(Value3); Trace.WriteLine(Value4); Trace.WriteLine(Formatter);
            //// Trace.WriteLine("Updated");

            // List<int> pressure1Values = GenerateArithmeticSeries(ra, 25, 10); // Arithmetic series for Pressure 1
            // List<int> pressure2Values = GenerateArithmeticSeries(ra + 10, 25, 10); // Arithmetic series for Pressure 2
            // List<int> pressure3Values = GenerateArithmeticSeries(ra + 20, 25, 10); // Arithmetic series for Pressure 3
            // List<int> pressure4Values = GenerateArithmeticSeries(ra + 30, 25, 10); // Arithmetic series for Pressure 4

            // // Assign values to the series
            // Series[0].Values = pressure1Values.Select(x => (double)x).ToList(); // Pressure 1
            // Series[1].Values = pressure2Values.Select(x => (double)x).ToList(); // Pressure 2
            // Series[2].Values = pressure3Values.Select(x => (double)x).ToList(); // Pressure 3
            // Series[3].Values = pressure4Values.Select(x => (double)x).ToList(); // Pressure 4

            // // Assign Formatter value to Temperature series
            // Formatter = GenerateRandomInteger(0, 100); // Random temperature value
            // Series[4].Values = Enumerable.Repeat((double)Formatter, 25).ToList(); // Temperature series with constant value

            // // Update the Value properties, they can be used to show the most recent value or be used elsewhere
            // Value1 = pressure1Values.Last();
            // Value2 = pressure2Values.Last();
            // Value3 = pressure3Values.Last();
            // Value4 = pressure4Values.Last();
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
            await _firebaseService.StoreToFirestore(pressure1Value, pressure2Value, pressure3Value, pressure4Value, temperatureValue);
            Trace.WriteLine(Value1); Trace.WriteLine(Value2); Trace.WriteLine(Value3); Trace.WriteLine(Value4); Trace.WriteLine(Formatter);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
           UpdateChartData();
           UpdateDateTime();
            
        }

        //[RelayCommand]
        
        private void UpdateDateTime()
        {
            date.Text = DateTime.Now.ToString("dd-MM-yyyy");
            time.Text = DateTime.Now.ToString("HH:mm:ss");
        }
      
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            serialPort = new SerialPort
            {
                PortName = "COM8", // Specify your COM port
                BaudRate = 9600,   // Adjust baud rate as per your device
                Parity = Parity.Even,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            byte slaveId = 0x01;  // Set your slave ID here
            byte functionCode = 0x03;  // Set your function code here (e.g., Read Holding Registers)
            byte[] data = new byte[] { 0x00, 0x01 };  // Set the data bytes (e.g., the register addresses or values)

            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    MessageBox.Show("Serial port opened and ready to write.");
                    SendModbusFrame(slaveId, functionCode, data);  // Start writing data
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        private void SendModbusFrame(byte slaveId, byte functionCode, byte[] data)
        {
            // Construct Modbus RTU frame
            List<byte> frame = new List<byte>();
            frame.Add(slaveId);             // Slave ID
            frame.Add(functionCode);        // Function Code
            frame.AddRange(data);           // Data bytes (depends on the function)

            // Calculate CRC (Cyclic Redundancy Check) for Modbus RTU frame
            byte[] frameBytes = frame.ToArray();
            byte[] crc = CalculateCRC(frameBytes);
            frame.AddRange(crc);            // Add CRC to the frame

            // Send the frame to the serial port
            serialPort.Write(frame.ToArray(), 0, frame.Count);
        }

        private byte[] CalculateCRC(byte[] frame)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < frame.Length; i++)
            {
                crc ^= frame[i];
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }
            return new byte[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
        }


        private void ProcessReceivedData(byte[] buffer)
        {
            if (buffer.Length < 5)  // Minimum frame length: Slave Address + Function Code + 2 bytes for CRC
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Invalid data received. Frame too short.");
                });
                return;
            }

            byte slaveId = buffer[0];              // Extract slave ID
            byte functionCode = buffer[1];         // Extract function code
            byte[] data = buffer.Skip(2).Take(buffer.Length - 4).ToArray(); // Extract data (excluding CRC)
            int slaveAddress = BitConverter.ToUInt16(new byte[] { data[0], data[1] }, 0);
            // Validate the CRC
            byte[] receivedCrc = buffer.Skip(buffer.Length - 2).Take(2).ToArray();
            byte[] calculatedCrc = CalculateCRC(buffer.Take(buffer.Length - 2).ToArray());

            if (!receivedCrc.SequenceEqual(calculatedCrc))
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Invalid CRC for slave {slaveId}, function {functionCode}");
                });
                return;
            }

            // Process based on function code
            if (functionCode == 0x03)  // Read Holding Registers
            {
                // Extract register data (2 bytes per register)
                // Assuming the data represents a 16-bit register (modify as needed)
                foreach (var val in data)
                {
                    Dispatcher.Invoke(() =>
                    {
                        //Commdata.AppendText($"Slave {slaveId} Data: {val:X2}\n");
                    });
                }
            }
            else if (functionCode == 0x06)  // Write Holding Register
            {
                // Process the response for a written register (e.g., echo the sent data)
                Dispatcher.Invoke(() =>
                {
                    //Commdata.AppendText($"Slave {slaveId} acknowledged write: {BitConverter.ToString(data)}\n");
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    //Commdata.AppendText($"Received unsupported function code {functionCode} from Slave {slaveId}\n");
                });
            }
        }
        private void Start_Click1(object sender, RoutedEventArgs e)
        {

            serialPort = new SerialPort
            {
                PortName = "COM8", // Specify your COM port
                BaudRate = 9600,   // Adjust baud rate as per your device
                Parity = Parity.Even,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };
            //SerialPort_DataSending();
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    MessageBox.Show("Serial port opened and ready to write.");
                    // Start writing data
                }
               // SerialPort_DataSending();// Begin sending data to the serial port
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }

        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                MessageBox.Show("Serial port closed.");
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Initialize the SerialPort object
            serialPort = new SerialPort
            {
                PortName = "COM8", // Specify your COM port
                BaudRate = 9600,   // Adjust baud rate as per your device
                Parity = Parity.Even,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            // Attach the DataReceived event to read incoming data
            serialPort.DataReceived += SerialPort_DataReceived3;

            try
            {
                serialPort.Open();
                MessageBox.Show("Serial port opened and ready to read.");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the serial port when done
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                MessageBox.Show("Serial port closed.");
            }
        }
       
        private void SerialPort_DataReceived3(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Create a buffer to hold the incoming data
                byte[] buffer = new byte[serialPort.BytesToRead];

                // Read the raw byte data from the serial port
                serialPort.Read(buffer, 0, buffer.Length);

                // Handle the received byte data
                ProcessReceivedData1(buffer);
               // SerialPort_DataSending(buffer);
                SendModbusFrame(buffer[0], buffer[1], buffer);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }

        private void ProcessReceivedData1(byte[] buffer)
        {
            // Process each byte individually or in chunks as needed
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                byte b = buffer[i];

                // Display the byte as a hexadecimal value
                //sb.Append($"0x{b:X2} ");

                // If the byte sequence seems to be a multi-byte number (e.g., integers), we can interpret them
                // For example, assuming 4 bytes per integer, you could do something like:
                //if (i + 3 < buffer.Length)  // Ensure we have enough bytes to form an integer
                //{
                //    int value = BitConverter.ToInt32(buffer, i);
                //    sb.Append($"(int: {value}) ");
                //    i += 3; // Skip ahead after processing an integer (4 bytes)
                //}

                //// If the byte sequence should be interpreted as a float, we could do something like:
                //if (i + 3 < buffer.Length)  // Check if there are enough bytes for a float
                //{
                //    float floatValue = BitConverter.ToSingle(buffer, i);
                //    sb.Append($"(float: {floatValue}) ");
                //    i += 3; // Skip ahead after processing a float (4 bytes)
                //}
            }

            foreach (byte b in buffer)
            {
                // Convert byte to an ASCII character (if it's printable)
                if (b >= 32 && b <= 126) // Printable ASCII range
                {
                    sb.Append((char)b);
                }
                else
                {
                    // Handle non-printable bytes (could be a numeric value or other format)
                    //sb.Append($"0x{b:X2} ({b})");
                    sb.Append($"{b}" + Environment.NewLine);// Display as hexadecimal
                }
            }

            // Update the UI with the processed data
            Dispatcher.Invoke(() =>
            {
                //ReceivedDataTextBox.AppendText(sb.ToString() + Environment.NewLine);
            });
        }


        private void SerialPort_DataSending(byte[] buffer)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    // Send the same data byte by byte
                    foreach (byte b in buffer)
                    {
                        byte[] byteToSend = new byte[] { b }; // Create a byte array with a single byte
                        serialPort.Write(byteToSend, 0, 1);  // Send the byte array
                        Thread.Sleep(50); // Optional delay between sending each byte
                    }

                    // Log the sent data in the Commdata RichTextBox
                    StringBuilder sentData = new StringBuilder();
                    foreach (byte b in buffer)
                    {
                        sentData.Append($"{b} ");
                    }

                    // Update the Commdata RichTextBox with the sent data
                    Dispatcher.Invoke(() =>
                    {
                        WriteData($"Sent data: {sentData.ToString()}");
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        WriteData($"Failed to send data: {ex.Message}");
                    });
                }
            }
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


        // Log the sent data in the Commdata RichTextBox
        private void WriteData(string text)
        {
            Trace.WriteLine(text);
           // Commdata.AppendText(text + Environment.NewLine);

            // Assign the value of the plot to the RichTextBox
            para.Inlines.Add(text);
            mcFlowDoc.Blocks.Add(para);
           // Commdata.Document = mcFlowDoc;
        }
        //private void SerialPort_DataSending(byte[] buffer)
        //{
        //    if (serialPort.IsOpen)
        //    {
        //        string data = serialPort.ReadLine(); 
        //        Trace.WriteLine(data);
        //        try
        //        {
        //            //string data = serialPort.ReadExisting();
        //            //Send the binary data out the port
        //            byte[] hexstring = Encoding.ASCII.GetBytes(data);
        //            foreach (byte hexval in hexstring)
        //            {
        //                byte[] _hexval = new byte[] { hexval };     // need to convert byte 
        //                                                            // to byte[] to write
        //                serialPort.Write(_hexval, 0, 1);
        //                Thread.Sleep(1);
        //            }
        //            foreach (byte hexval in hexstring)
        //            {
        //                byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
        //                serialPort.Write(_hexval, 0, 1);
        //                Thread.Sleep(1);
        //            }
        //            byte[] data1 = Encoding.ASCII.GetBytes(data);
        //            Trace.WriteLine(data1);
        //            // Send the data byte by byte
        //            foreach (byte b in data1)
        //            {
        //                Trace.WriteLine(b);
        //                byte[] byteToSend = new byte[] { b };
        //                Trace.WriteLine(byteToSend);
        //                serialPort.Write(byteToSend, 0, 1); // Send a single byte
        //                Thread.Sleep(100);
        //                WriteData($"Sent data: {b}");// Pause briefly before sending the next byte
        //            }
        //            Dispatcher.Invoke(() =>
        //            {
        //                Trace.WriteLine(data1);
        //                WriteData($"Sent data: 1 + {data}");  // Log the sent data to the UI
        //            });
        //            Thread.Sleep(1000); // Delay between sending data
        //            //SerialPort_DataSending();
        //        }
        //        catch (Exception ex)
        //        {
        //            para.Inlines.Add("Failed to SEND" + data + "\n" + ex + "\n");
        //            mcFlowDoc.Blocks.Add(para);
        //            Commdata.Document = mcFlowDoc;
        //        }
        //    }


        //}
        //private void WriteData(string text)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    Trace.WriteLine(text);
        //    Commdata.AppendText(text + Environment.NewLine);
        //    // Assign the value of the plot to the RichTextBox.
        //    para.Inlines.Add(text);
        //    mcFlowDoc.Blocks.Add(para);
        //    Commdata.Document = mcFlowDoc;
        //}
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);

                // Convert byte array to ASCII string
                string data = Encoding.ASCII.GetString(buffer);
                int numericValue = BitConverter.ToInt32(buffer, 0);
                float floatValue = BitConverter.ToSingle(buffer, 0);
                // Read the data from the serial port asynchronously
                // string data = serialPort.ReadExisting(); // You can also use ReadLine() if data is line-based
                Dispatcher.Invoke(() =>
                {
                    // Update the UI with the received data
                    //ReceivedDataTextBox.AppendText(data + Environment.NewLine);
                    //ReceivedDataTextBox.AppendText("Received Value: " + numericValue + Environment.NewLine);
                    //ReceivedDataTextBox.AppendText("Received Value: " + floatValue + Environment.NewLine);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }
        private void SerialPort_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Read the data from the serial port (in bytes)
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);

                // Example: First byte could be an identifier for numeric data or ASCII
                if (buffer[0] == 1) // Assuming identifier 1 means numeric data
                {
                    int numericValue = BitConverter.ToInt32(buffer, 1); // Skipping the first byte
                    Dispatcher.Invoke(() =>
                    {
                        //ReceivedDataTextBox.AppendText("Numeric Value: " + numericValue + Environment.NewLine);
                    });
                }
                else if (buffer[0] == 2) // Assuming identifier 2 means ASCII data
                {
                    string asciiValue = Encoding.ASCII.GetString(buffer, 1, buffer.Length - 1); // Skip identifier byte
                    Dispatcher.Invoke(() =>
                    {
                       // ReceivedDataTextBox.AppendText("ASCII Value: " + asciiValue + Environment.NewLine);
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }
        private void SerialPort_DataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Read the data from the serial port using ReadExisting for ASCII data
                string data = serialPort.ReadExisting();

                // Append the received data to the StringBuilder
                _sb.Append(data);

                // If the data includes new line, we display it and reset the StringBuilder
                if (data.Contains(Environment.NewLine))
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Display the received data in the TextBox
                        //ReceivedDataTextBox.AppendText(_sb.ToString());
                        //_sb.Clear(); // Clear the StringBuilder for the next data chunk
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error reading from serial port: {ex.Message}");
                });
            }
        }

    }

}

