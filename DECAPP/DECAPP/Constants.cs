using System;
using System.IO;

namespace DECAPP
{
    public static class Constants
    {
        // Переменные для базы данных
        public const string dbName = "DBCatalog.db";
        public const int dbVersion = 65;

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            //SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache |
            //Соединение открывается в режиме сериализованной потоковой передачи.
            SQLite.SQLiteOpenFlags.FullMutex;

        // путь, по которому будет находиться база данных
        public static string DatabasePath
        {
            get
            {
                //string dbPath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, Constants.dbName);
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, dbName);
            }
        }
    }
}
