using Google.Cloud.Firestore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using static Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData;
using SQLite;

namespace Wipro
{
    /// <summary>
    /// Interaction logic for GenerateReport.xaml
    /// </summary>
    public partial class GenerateReport : Window
    {
        private string loc;
        public string Loc
        {
            get { return loc; }
            set
            {
                loc = value;
                OnPropertyChanged("Loc");
            }
        }
        public List<string> Classes { get; set; }
        public string SelectedClass { get; set; }
        public GenerateReport()
        {
            InitializeComponent();

            DataContext = this;
            Classes = new List<string> { "LabViewData Report" };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetFileName()
        {
            string qu1 = "";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    var query = connection.Table<FilePath>().FirstOrDefault();
                    
                    if (query != null)
                    {
                        qu1 = query.Fileurl;
                        //string filename = System.IO.Path.GetFileName(FileUrl.LocalPath)
                    }
                    else
                    {
                        qu1 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return qu1;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var firebaseService = new FirebaseService();
            FileStream stream = null;
            StreamWriter writer = null;
            //FileStream stream1 = null;
            //StreamWriter writer1 = null;
            string selectedClass = SelectedClass;
            string startDate = Date0.SelectedDate?.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString("dd-MM-yyyy");
            string endDate = Date2.SelectedDate?.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString("dd-MM-yyyy");

            try
            {
                if (selectedClass == "LabViewData Report")
                {
                    int success = Validate(Date0.ToString(), Date2.ToString());
                    string qu = "";
                    string qu1 = GetFileName();

                    SQLiteConnection connection = new SQLiteConnection(App.databasePath);

                    string fileName1 = qu1 + "\\";
                    string filename3 = "LabView Report" + DateTime.Parse(Date0.ToString()).ToString("dd-MM-yy") + "-" + DateTime.Parse(Date2.ToString()).ToString("dd-MM-yy") + DateTime.Now.ToString("HH-mm");
                    string filename4 = ".csv";
                    string fileName = fileName1 + filename3 + filename4;


                    stream = new FileStream(fileName, FileMode.OpenOrCreate);
                    stream.Seek(0, SeekOrigin.End);
                    writer = new StreamWriter(stream, Encoding.UTF8);
                    { writer.WriteLine("TimeStamp,Pressure1,Pressure2,Pressure3,Pressure4,Temperature"); }
                    if (success >= 0)
                    {
                        List<DateTime> queryDates = GetDatesBetween(DateTime.Parse(Date0.ToString()), DateTime.Parse(Date2.ToString()));
                        try
                        {
                            foreach (DateTime date in queryDates)
                            {
                                qu = date.ToString("dd-MM-yyyy");
                                var q = connection.Table<LabViewData>().Where(v => v.Timestamp.StartsWith(qu));
                                foreach (LabViewData labviewData in q)
                                {
                                    if (labviewData != null)
                                    {
                                        writer.WriteLine($"{labviewData.Timestamp},{labviewData.Pressure1},{labviewData.Pressure2},{labviewData.Pressure3},{labviewData.Pressure4},{labviewData.Temperature}");
                                    }
                                }
                            }
                            MessageBox.Show($"Report saved to {fileName}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                            MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                await DownloadSectionDataReport(startDate, endDate);

                //else if (selectedClass == "Eff/DT Report")
                //{
                //    int success = Validate(Date0.ToString(), Date2.ToString());
                //    string qu = "";
                //    string qu1 = GetFileName();

                //    SQLiteConnection connection = new SQLiteConnection(App.databasePath);

                //    string fileName1 = qu1 + "\\";
                //    string filename5 = "EFF_DT Report" + DateTime.Parse(Date0.ToString()).ToString("dd-MM-yy") + "-" + DateTime.Parse(Date2.ToString()).ToString("dd-MM-yy") + DateTime.Now.ToString("HH-mm");
                //    string filename4 = ".csv";
                //    string fileName0 = fileName1 + filename5 + filename4;


                //    stream1 = new FileStream(fileName0, FileMode.OpenOrCreate);
                //    stream1.Seek(0, SeekOrigin.End);
                //    writer1 = new StreamWriter(stream1, Encoding.UTF8);
                //    { writer1.WriteLine("DATE,STATION,ACTUAL,EFFICIENCY,DELAYINMIN,MACHINEDOWNTIME,PRODEDOWNTIME,MAINTDOWNTIME,QUALITYDOWNTIME,STOREDOWNTIME"); }
                //    if (success >= 0)
                //    {
                //        List<DateTime> queryDates = GetDatesBetween(DateTime.Parse(Date0.ToString()), DateTime.Parse(Date2.ToString()));
                //        try
                //        {
                //            foreach (DateTime date in queryDates)
                //            {
                //                qu = date.ToString("dd-MM-yyyy");
                //                var qd = connection.Table<DailyRecord>().Where(v => v.TodayDate.StartsWith(qu));
                //                foreach (DailyRecord dailyRecord in qd)
                //                {
                //                    if (dailyRecord != null)
                //                    {
                //                        writer1.WriteLine($"{dailyRecord.TodayDate},{dailyRecord.Station},{dailyRecord.Actual},{dailyRecord.Efficiency},{dailyRecord.DelayInMin},{dailyRecord.MachineDowntime},{dailyRecord.pMachineDowntime},{dailyRecord.mMachineDowntime},{dailyRecord.qMachineDowntime},{dailyRecord.sMachineDowntime}");
                //                    }
                //                }
                //            }
                //            MessageBox.Show($"Report saved to {fileName0}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                //        }
                //        catch (Exception ex)
                //        {
                //            Trace.WriteLine(ex.Message);
                //            MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                //        }

                //    }
                //}
            }
            catch { }

            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Dispose();
                }
                //if (writer1 != null)
                //{
                //    writer1.Dispose();
                //}
                //if (stream1 != null)
                //{
                //    stream1.Dispose();
                //}
            }
        }


        private static List<DateTime> GetDatesBetween(DateTime date1, DateTime date2)
        {
            List<DateTime> allDates = new List<DateTime>();

            for (DateTime date = date1; date <= date2; date = date.AddDays(1))
            {
                //Trace.WriteLine(date);
                allDates.Add(date.Date);
            }
            //Trace.WriteLine(allDates.Count);
            return allDates;
        }

        private int Validate(string date1, string date2)

        {
            if (!string.IsNullOrEmpty(date1) && !string.IsNullOrEmpty(date2))
            {
                DateTime Date1 = DateTime.Parse(date1);
                DateTime Date2 = DateTime.Parse(date2);
                int value = Date2.CompareTo(Date1);

                if (value < 0)
                {
                    MessageBox.Show("From Date should be earlier than To Date. Please select again", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                //Trace.WriteLine(V);
                return value;
            }
            else
            {
                MessageBox.Show("Date should not be empty. Please select again", "Empty Date", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog()
            {
                Title = "AndonSystem2",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = true
            };

            string folderName = "";
            if (dialog.ShowDialog() == true)
            {
                folderName = dialog.FolderName;
                Loc = folderName;
            }
        }
        private string selectedStation;
        public string SelectedStation
        {
            get { return selectedStation; }
            set
            {
                selectedStation = value;
                OnPropertyChanged("SelectedStation");
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(Loc))
            {
                MessageBox.Show("Please select a folder first.");
                return;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    connection.Execute("DELETE FROM FilePath");
                    //connection.Execute("DELETE FROM FilePath WHERE MeterName = ?", "Schneider EM6436H");

                    FilePath file = new FilePath()
                    {
                        BayName = "Wipro",
                        Station = "LabView",
                        Fileurl = Loc,
                        TimeStamp = DateTime.Now.ToString("dd-MM-yyyy HH_mm"),
                    };

                    connection.Insert(file);
                }

                MessageBox.Show("Document location stored successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                // Handle or log the exception as needed
            }
        }

        private async Task DownloadSectionDataReport(string startDate, string endDate)
        {
            var firebaseService = new FirebaseService();
            var sectionDataRecords = await firebaseService.GetSectionDataAsync(startDate, endDate);

            if (sectionDataRecords == null || !sectionDataRecords.Any())
            {
                 MessageBox.Show("No data found for the selected date range.");
                return;
            }

            string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"LabViewData_Report_{startDate:dd-MM-yyyy}_{endDate:dd-MM-yyyy}.csv");

            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine("Timestamp,Pressure1,Pressure2,Pressure3,Pressure4,Temperature");

                foreach (var record in sectionDataRecords)
                {
                    writer.WriteLine($"{record.Timestamp},{record.Pressure1},{record.Pressure2},{record.Pressure3},{record.Pressure4},{record.Temperature}");
                }
            }

            MessageBox.Show($"Report saved to {filePath}", "Report Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
