using Google.Cloud.Storage.V1;
//using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Google.Cloud.Firestore;
using System.Diagnostics;
using static Grpc.Core.Metadata;
using System.Windows;
using HarfBuzzSharp;
using System.Windows.Media;

namespace Wipro
{
    public class FirebaseService
    {
        private FirebaseApp _firebaseApp;
        private StorageClient _storageClient;
        private static bool isFirebaseInitialized = false;
        private static FirestoreDb _firestoreDb;
        private DocumentReference wiproDocRef;
        private static readonly object _lock = new object();

        public FirebaseService()
        {
            if (!isFirebaseInitialized)
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
               string pathToServiceAccountKey = Path.Combine(appDirectory, "assets", "your_json_file.json");

                //string pathToServiceAccountKey = @"D:\WPF_Projects\Wipro\assets\your_json_file.json"; 
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToServiceAccountKey);

                if (FirebaseApp.DefaultInstance == null)
                {
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(pathToServiceAccountKey)

                    });

                    _firestoreDb = FirestoreDb.Create("wiprolabview");
                    _storageClient = StorageClient.Create();
                    Trace.WriteLine("Created");
                    wiproDocRef = _firestoreDb.Collection("Wipro").Document("LabView");
                    isFirebaseInitialized = true;
                    //Trace.WriteLine("FirebaseApp initialized successfully.");
                }
                else
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                    // Trace.WriteLine("FirebaseApp already initialized.");
                    _firestoreDb = FirestoreDb.Create("wiprolabview");
                    wiproDocRef = _firestoreDb.Collection("Wipro").Document("LabView");
                }
            }
            else
            {
                Trace.WriteLine("FirebaseApp already initialized.");
            }
        }

        public static FirestoreDb GetFirestoreDb()
        {
            return _firestoreDb;
        }

        //public FirebaseService Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            lock (_lock)
        //            {
        //                if (_instance == null)
        //                {
        //                    _instance = new FirebaseService();
        //                }
        //            }
        //        }
        //        return _instance;
        //    }
        //}

        //public async Task UploadDataToFirebase(int pressure1Value, int pressure2Value, int pressure3Value, int pressure4Value, int temperatureValue)
        //{
        //    string bucketName = "wiprolabview.firebasestorage.app";

        //    string fileName = $"LabView_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
        //    string objectName = $"Values/{fileName}";

        //    //string pl = payload.Trim('{', '}');
        //    //string[] entries = pl.Split(',');


        //    //byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(pl);
        //    // Trace.WriteLine(payload);


        //    using (var stream = new MemoryStream(payloadBytes))
        //    {
        //        var storageObject = await _storageClient.UploadObjectAsync(bucketName, objectName, "text/plain", stream);
        //        Console.WriteLine($"Data uploaded to Firebase Storage with object name: {storageObject.Name}");
        //    }
        //    List<double> payloadList = ConvertPayloadToDoubleList(entries);

        //    await StoreToFirestore(pressure1Value, pressure2Value, pressure3Value, pressure4Value, temperatureValue);
        //}

        //private List<double> ConvertPayloadToDoubleList(string[] entries)
        //{
        //    List<double> payloadList = new List<double>();
        //    foreach (string entry in entries)
        //    {
        //        string trimmedEntry = entry.Trim();

        //        if (double.TryParse(trimmedEntry, out double number))
        //        {
        //            payloadList.Add(number);
        //            Trace.WriteLine(number);
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Failed to parse value: {trimmedEntry}");
        //        }
        //    }
        //    return payloadList;
        //}

        public async Task StoreToFirestore(int pressure1Value, int pressure2Value, int pressure3Value, int pressure4Value, int temperatureValue)
        {
            string timestamp = DateTime.Now.ToString("dd-MM-yyyy");
            string timestamp1 = DateTime.Now.ToString("dd-MM-yyyy_HH_mm_ss");

            //DocumentReference andonDocRef = _firestoreDb.Document("Andon");
            Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "pressure1", pressure1Value },
            { "pressure2", pressure2Value },
            { "pressure3", pressure3Value },
            { "pressure4", pressure4Value },
            { "temperature", temperatureValue },
            { "timestamp", timestamp }
        };

            Dictionary<string, object> updateData = new Dictionary<string, object>
        {
            { $"{timestamp1}", data }
            //{ "data", FieldValue.ArrayUnion(data) }  // This appends data to the array field
        };
            CollectionReference dateSubCollectionRef = wiproDocRef.Collection(timestamp);
            DocumentReference newDataDocRef = dateSubCollectionRef.Document(timestamp1);

            await newDataDocRef.SetAsync(data);
            //await wiproDocRef.SetAsync(updateData, SetOptions.MergeAll);

            //Trace.WriteLine($"Data for station {stationName} and topic {topic} stored successfully.");
        }
        public async Task DownloadDataFromFirestore()
        {
            try
            {
                DocumentReference wiproDocRef = _firestoreDb.Collection("Wipro").Document("LabView");
                DocumentSnapshot snapshot = await wiproDocRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    Trace.WriteLine("Document {0} does not exist!", snapshot.Id);
                    return;
                }

                              Dictionary<string, object> data = snapshot.ToDictionary();

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FirestoreData.csv");

                using (StreamWriter sw = new StreamWriter(filePath, false))
                {
                    sw.WriteLine("timestamp,pressure1,pressure2,pressure3,pressure4,temperature");

                    foreach (KeyValuePair<string, object> pair in data)
                    {
                        string timestamp1Key = pair.Key;
                        Dictionary<string, object> entry = pair.Value as Dictionary<string, object>;

                        if (entry != null)
                        {
                            string timestamp = entry.ContainsKey("timestamp") ? data["timestamp"].ToString() : "N/A"; ; 
                            string pressure1 = entry.ContainsKey("pressure1") ? entry["pressure1"].ToString() : "N/A";
                            string pressure2 = entry.ContainsKey("pressure2") ? entry["pressure2"].ToString() : "N/A";
                            string pressure3 = entry.ContainsKey("pressure3") ? entry["pressure3"].ToString() : "N/A";
                            string pressure4 = entry.ContainsKey("pressure4") ? entry["pressure4"].ToString() : "N/A";
                            string temperature = entry.ContainsKey("temperature") ? entry["temperature"].ToString() : "N/A";

                            sw.WriteLine($"{timestamp},{pressure1},{pressure2},{pressure3},{pressure4},{temperature}");
                        }
                    }

                    Trace.WriteLine("Data has been written to the CSV file successfully.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error retrieving data or writing to CSV: {ex.Message}");
            }
        }

        public async Task DownloadDataFromFirestore1(DateTime startDate, DateTime endDate)
        {
            //Trace.WriteLine("Collection Download Started");

            try
            {
                string startDateString = startDate.ToString("dd-MM-yyyy");
                string endDateString = endDate.ToString("dd-MM-yyyy");

                // _firestoreDb = FirestoreDb.Create("andon-system-rwa");

                DocumentReference wiproDocRef = _firestoreDb.Collection("Wipro").Document("LabView");
                DocumentSnapshot snapshot = await wiproDocRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    Dictionary<string, object> data = snapshot.ToDictionary();
                    foreach (KeyValuePair<string, object> pair in data)
                    {
                        Trace.WriteLine($"{pair.Key}: {pair.Value}");
                    }
                    //Trace.WriteLine("YESSSSSS");
                }
                else
                {
                    Trace.WriteLine("Document {0} does not exist!", snapshot.Id);
                }

                if (snapshot.Exists)
                {

                    CollectionReference dataCollectionRef = wiproDocRef.Collection("data");
                    QuerySnapshot querySnapshot = await dataCollectionRef.GetSnapshotAsync();

                    if (querySnapshot.Count == 0)
                    {
                        Trace.WriteLine("No documents found in the specified date range.");
                        return;
                    }

                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FirestoreData.csv");

                    using (StreamWriter sw = new StreamWriter(filePath, false)) // false to overwrite file if exists
                    {

                        sw.WriteLine("timestamp,pressure1,pressure2,pressure3,pressure4,temperature");
                        //Trace.WriteLine("CSV Header written.");

                        foreach (DocumentSnapshot document in querySnapshot.Documents)
                        {
                            try
                            {
                                Dictionary<string, object> data = document.ToDictionary();
                                string documentId = document.Id;

                                string timestamp = data.ContainsKey("timestamp") ? data["timestamp"].ToString() : "N/A";
                                string pressure1 = data.ContainsKey("pressure1") ? data["pressure1"].ToString() : "N/A";
                                string pressure2 = data.ContainsKey("pressure2") ? data["pressure2"].ToString() : "N/A";
                                string pressure3 = data.ContainsKey("pressure3") ? data["pressure3"].ToString() : "N/A";
                                string pressure4 = data.ContainsKey("pressure4") ? data["pressure4"].ToString() : "N/A";
                                string temperature = data.ContainsKey("temperature") ? data["temperature"].ToString() : "N/A";

                                //List<double> payload = data.ContainsKey("payload") && data["payload"] is List<object> payloadList
                                //    ? payloadList.Select(p => Convert.ToDouble(p)).ToList()
                                //    : new List<double>();

                                //string payloadString = string.Join(", ", payload);

                                sw.WriteLine($"{timestamp},{pressure1},{pressure2},{pressure3},{pressure4},{temperature}");

                                //Trace.WriteLine($"Written document {document.Id} to CSV.");
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"Error processing document {document.Id}: {ex.Message}");
                            }
                        }
                    }

                    Trace.WriteLine("Data has been written to the CSV file successfully.");
                }
                else
                {
                    Trace.WriteLine($"Document {snapshot.Id} does not exist.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error retrieving data or writing to CSV: {ex.Message}");
            }
        }
        public async Task DownloadFilesFromFirebase(string localDirectory)
        {
            string bucketName = "andon-system-rwa.appspot.com";
            string folderPath = "mqtt-data/esp32";
            StorageClient storageClient = await StorageClient.CreateAsync();

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(documentsPath))
            {
                Directory.CreateDirectory(documentsPath);
            }

            string combinedFilePath = Path.Combine(documentsPath, "combined_report.csv");

            try
            {
                // Trace.WriteLine("Bucket Download Started");


                List<Google.Apis.Storage.v1.Data.Object> objectList = new List<Google.Apis.Storage.v1.Data.Object>();
                await foreach (Google.Apis.Storage.v1.Data.Object storageObject in storageClient.ListObjectsAsync(bucketName, folderPath))
                {
                    objectList.Add(storageObject);
                }

                if (objectList.Count == 0)
                {
                    MessageBox.Show("No files found in the Firebase bucket.");
                    return;
                }

                List<string> allFileContents = new List<string>();

                List<Task> downloadTasks = objectList.Select(async storageObject =>
                {
                    string fileContent = await DownloadFileContent(storageClient, bucketName, storageObject);
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        allFileContents.Add(fileContent);
                    }
                }).ToList();

                await Task.WhenAll(downloadTasks);
                using (StreamWriter writer = new StreamWriter(combinedFilePath, false, Encoding.UTF8))
                {
                    writer.WriteLine("Timestamp,StationName,Topic,Payload");
                    string combinedData = string.Join(Environment.NewLine, allFileContents);
                    writer.WriteLine(combinedData);
                }

                MessageBox.Show("All files downloaded and combined into report.");
                Trace.WriteLine("All files downloaded and written to the combined CSV.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download files: {ex.Message}");
            }
        }

        private async Task<string> DownloadFileContent(StorageClient storageClient, string bucketName, Google.Apis.Storage.v1.Data.Object storageObject)
        {
            try
            {
                // Trace.WriteLine($"Downloading file: {storageObject.Name}");

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await storageClient.DownloadObjectAsync(bucketName, storageObject.Name, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (StreamReader reader = new StreamReader(memoryStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error downloading file {storageObject.Name}: {ex.Message}");
                return null;
            }
        }


        //public async Task<List<DailyRecord>> GetDailyRecordsAsync(string startDate, string endDate)
        //{
        //    CollectionReference collectionRef = _firestoreDb.Collection("JBMOGIHARA").Document("Andon").Collection("DailyRecord");
        //    QuerySnapshot querySnapshot = await collectionRef
        //                                    .GetSnapshotAsync();
        //    Trace.WriteLine("Query");

        //    DateTime start = DateTime.ParseExact(startDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
        //    DateTime end = DateTime.ParseExact(endDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

        //    List<DailyRecord> dailyRecords = querySnapshot.Documents.Where(doc =>
        //    {

        //        string[] parts = doc.Id.Split('_');
        //        // Trace.WriteLine($"Parts: {string.Join(",", parts)}");
        //        if (parts.Length == 2)
        //        {
        //            string dateString = parts[1];

        //            if (DateTime.TryParseExact(dateString, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime docDate))
        //            {
        //                return docDate >= start && docDate <= end;
        //            }
        //        }
        //        return false;
        //    })
        //      .Select(doc =>
        //      {
        //          var data = doc.ToDictionary();
        //          var record = new DailyRecord
        //          {
        //              Station = data.ContainsKey("Station") ? data["Station"].ToString() : string.Empty,
        //              TodayDate = data.ContainsKey("Date") ? data["Date"].ToString() : string.Empty,
        //              Actual = data.ContainsKey("Actual") ? Convert.ToInt32(data["Actual"]) : 0,
        //              Efficiency = data.ContainsKey("Efficiency") ? Convert.ToSingle(data["Efficiency"]) : 0.0f,
        //              DelayInMin = data.ContainsKey("DelayInMin") ? ParseTimeSpan(data["DelayInMin"].ToString()) : TimeSpan.Zero,
        //              MachineDowntime = data.ContainsKey("MachineDowntime") ? ParseTimeSpan(data["MachineDowntime"].ToString()) : TimeSpan.Zero,
        //              pMachineDowntime = data.ContainsKey("pMachineDowntime") ? ParseTimeSpan(data["pMachineDowntime"].ToString()) : TimeSpan.Zero,
        //              mMachineDowntime = data.ContainsKey("mMachineDowntime") ? ParseTimeSpan(data["mMachineDowntime"].ToString()) : TimeSpan.Zero,
        //              qMachineDowntime = data.ContainsKey("qMachineDowntime") ? ParseTimeSpan(data["qMachineDowntime"].ToString()) : TimeSpan.Zero,
        //              sMachineDowntime = data.ContainsKey("sMachineDowntime") ? ParseTimeSpan(data["sMachineDowntime"].ToString()) : TimeSpan.Zero
        //          };

        //          return record;
        //      })
        //.ToList();

        //    return dailyRecords;
        //}

        //private TimeSpan ParseTimeSpan(string timeSpanStr)
        //{
        //    if (TimeSpan.TryParse(timeSpanStr, out TimeSpan result))
        //    {
        //        return result;
        //    }
        //    return TimeSpan.Zero;
        //}

        public async Task<List<LabViewData>> GetSectionDataAsync(string startDate, string endDate)
        {
            DateTime start = DateTime.ParseExact(startDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DateTime end = DateTime.ParseExact(endDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            Trace.WriteLine("FirebaseApp already initialized.");
            List<LabViewData> sectionDataList = new List<LabViewData>();

            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                string docId = date.ToString("dd-MM-yyyy");
                Trace.WriteLine(docId);
                CollectionReference recordsRef = _firestoreDb.Collection("Wipro").Document("LabView").Collection(docId);

                QuerySnapshot recordsSnapshot = await recordsRef.GetSnapshotAsync();

                foreach (var recordDoc in recordsSnapshot.Documents)
                {
                    // var record = recordDoc.ConvertTo<LabViewData>();

                    var rawData = recordDoc.ToDictionary();

                    // Manually extract and convert the fields into the LabViewData object
                    LabViewData record = new LabViewData
                    {
                        // Use Convert.ToDouble to ensure values are converted to double
                        Pressure1 = rawData.ContainsKey("pressure1") ? Convert.ToDouble(rawData["pressure1"]) : 0.0,
                        Pressure2 = rawData.ContainsKey("pressure2") ? Convert.ToDouble(rawData["pressure2"]) : 0.0,
                        Pressure3 = rawData.ContainsKey("pressure3") ? Convert.ToDouble(rawData["pressure3"]) : 0.0,
                        Pressure4 = rawData.ContainsKey("pressure4") ? Convert.ToDouble(rawData["pressure4"]) : 0.0,
                        Temperature = rawData.ContainsKey("temperature") ? Convert.ToDouble(rawData["temperature"]) : 0.0,
                        Timestamp = rawData.ContainsKey("timestamp") ? rawData["timestamp"].ToString() : "N/A"
                    };
                    sectionDataList.Add(record);
                    Trace.WriteLine(record);
                }
            }
            return sectionDataList;
        }
    }
}


