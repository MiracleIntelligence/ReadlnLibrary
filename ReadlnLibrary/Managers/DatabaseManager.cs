
using System;
using System.IO;
using ReadlnLibrary.Core.Models;
using SQLite;
using Windows.Storage;

namespace ReadlnLibrary.Managers
{
    public class DatabaseManager
    {
        public const string LOCAL_DB_NAME = "readln.db";

        public static SQLiteConnection Connection { get; private set; }

        public static SQLiteConnection InitConnection()
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, LOCAL_DB_NAME);
            var db = new SQLiteConnection(path);
            db.CreateTable<RdlnDocument>();

            Connection = db;

            return db;
        }
    }
}
