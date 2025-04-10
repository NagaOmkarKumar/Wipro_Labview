using Google.Cloud.Firestore;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wipro
{
    [FirestoreData]
    public class LabViewData
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [FirestoreProperty]
        public double Pressure1 { get; set; }
        [FirestoreProperty]
        public double Pressure2 { get; set; }
        [FirestoreProperty]
        public double Pressure3 { get; set; }
        [FirestoreProperty]
        public double Pressure4 { get; set; }
        [FirestoreProperty]
        public double Temperature { get; set; }
        [FirestoreProperty]
        public string Timestamp { get; set; }



    }
}
