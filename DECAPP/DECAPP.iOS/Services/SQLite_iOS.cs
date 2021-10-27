using SQLite;
using System;
using System.IO;
using System.Security;
using Xamarin.Forms;

[assembly: Dependency(typeof(DECAPP.iOS.Services.SQLite_iOS))]

namespace DECAPP.iOS.Services
{
    [SecurityCritical]
    public class SQLite_iOS : DECAPP.Services.ISQLite
    {
        public SQLite_iOS()
        {
        }

        [SecurityCritical]
        public SQLiteConnection DbConnection()
        {
            //string personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //string libraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library");
            string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library"), App.dbName);

            return new SQLite.SQLiteConnection(path);
        }

        [SecurityCritical]
        public void CreateDBAsync()
        {
            try
            {
                string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library"), App.dbName);

                // копирование файла из папки Assets по пути path
                if (!File.Exists(path) || (File.Exists(path) && (GetCurrentDBVersion() < App.dbVersion)))
                {
                    File.Copy(App.dbName, path);
                }
            }
            catch (Exception)
            {
                return;
                //new UIKit.UIAlertView("Sorry!", ex.ToString(), null, "Cancel", null).Show();
            }
        }

        // Get Current Data Base Version
        [SecurityCritical]
        public int GetCurrentDBVersion()
        {
            int currentDbVersion;
            SQLiteConnection database = DependencyService.Get<DECAPP.Services.ISQLite>().DbConnection();

            try
            {
                if (database != null)
                {
                    currentDbVersion = database.ExecuteScalar<int>("pragma user_version");
                    //database.Close();
                    database.Dispose();
                    database = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                else
                {
                    currentDbVersion = 0;
                }
                return currentDbVersion;
            }
            catch (Exception)
            {
                currentDbVersion = 0;
                return currentDbVersion;
            }
        }
    }
}