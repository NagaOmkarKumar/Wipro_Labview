using Google.Cloud.Firestore;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wipro
{
    class FilePath
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } 
        public string BayName { get; set; }
        public string Station { get; set; }
        public string Fileurl { get; set; }
        public string TimeStamp { get; set; }
    }
}
