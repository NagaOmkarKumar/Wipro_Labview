using System.Configuration;
using System.Data;
using System.Windows;

namespace Wipro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static String databaseName = "Wipro.db";
        static String folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static String databasePath = System.IO.Path.Combine(folderPath, databaseName);
    }

}
