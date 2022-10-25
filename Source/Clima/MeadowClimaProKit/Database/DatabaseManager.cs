using Meadow;
using Meadow.Foundation;
using MeadowClimaProKit.Controller;
using System.Text.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeadowClimaProKit.Database
{
    public class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> instance =
            new Lazy<DatabaseManager>(() => new DatabaseManager());
        public static DatabaseManager Instance => instance.Value;

        bool isConfigured = false;

        //SQLiteConnection Database { get; set; }
        string Database { get; set; }

        private static object _db_lock = new object();

        private DatabaseManager()
        {
            Initialize();
        }

        private void _write(object? value)
        {
            var json = JsonSerializer.Serialize(value);
            lock (_db_lock)
            {
                using var fs = File.Open(Database, FileMode.Truncate, FileAccess.Write, FileShare.None);
                using var sw = new StreamWriter(fs);
                sw.Write(json);
            }
        }

        private T? _read<T>()
        {
            string json;
            lock (_db_lock)
            {
                using var fs = File.Open(Database, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                using var sr = new StreamReader(fs);
                json = sr.ReadToEnd();
            }
            return JsonSerializer.Deserialize<T>(json);
        }

        protected void Initialize()
        {
            var databasePath = Path.Combine(MeadowOS.FileSystem.DataDirectory, "ClimateReadings.db");
            Console.WriteLine($"{nameof(DatabaseManager)}: Creating database at {databasePath}.");
            Database = databasePath;
            try
            {
                File.Delete(Database);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not delete: {e.Message}.");
            }
            File.Create(Database).Dispose();
        }

        public bool SaveReading(ClimateReading climate)
        {
            LedController.Instance.SetColor(WildernessLabsColors.ChileanFireDark);

            if (climate == null)
            {

                Console.WriteLine($"{nameof(DatabaseManager)}: climate is null");
                return false;
            }

            try
            {
                climate.ID = 1;
                _write(climate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            LedController.Instance.SetColor(Color.Green);
            return true;
        }

        public ClimateReading GetClimateReading(int id)
        {
            try
            {
                return _read<ClimateReading>() ?? new ClimateReading();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not read: {e.Message}");
                return new ClimateReading();
            }

        }

        public Dictionary<string, ClimateReading> GetAllClimateReadings()
        {
            try
            {
                return new Dictionary<string, ClimateReading>
                {
                    { "1", GetClimateReading(1) }
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(DatabaseManager)}: {e.Message}");
                return new Dictionary<string, ClimateReading>
                {
                    { "1", new ClimateReading() }
                };
            }
        }
    }
}
//using Meadow;
//using Meadow.Foundation;
//using MeadowClimaProKit.Controller;
//using SQLite;
//using System;
//using System.Collections.Generic;
//using System.IO;

//namespace MeadowClimaProKit.Database
//{
//    public class DatabaseManager
//    {
//        private static readonly Lazy<DatabaseManager> instance =
//            new Lazy<DatabaseManager>(() => new DatabaseManager());
//        public static DatabaseManager Instance => instance.Value;

//        bool isConfigured = false;

//        SQLiteConnection Database { get; set; }

//        private DatabaseManager()
//        {
//            Initialize();
//        }

//        protected void Initialize()
//        {
//            var databasePath = Path.Combine(MeadowOS.FileSystem.DataDirectory, "ClimateReadings.db");
//            Database = new SQLiteConnection(databasePath);

//            Database.DropTable<ClimateReading>(); //convenience while we work on the model object
//            Database.CreateTable<ClimateReading>();
//            isConfigured = true;
//        }

//        public bool SaveReading(ClimateReading climate)
//        {
//            LedController.Instance.SetColor(WildernessLabsColors.ChileanFireDark);

//            if (isConfigured == false)
//            {
//                Console.WriteLine("SaveUpdateReading: DB not ready");
//                return false;
//            }

//            if (climate == null)
//            {
//                Console.WriteLine("SaveUpdateReading: Conditions is null");
//                return false;
//            }

//            Console.WriteLine("Saving climate reading to DB");

//            Database.Insert(climate);

//            Console.WriteLine($"Successfully saved to database");

//            LedController.Instance.SetColor(Color.Green);
//            return true;
//        }

//        public ClimateReading GetClimateReading()
//        {
//            return Database.Table<ClimateReading>().OrderByDescending(o => o.ID).FirstOrDefault();
//        }

//        public ClimateReading GetClimateReading(int id)
//        {
//            return Database.Get<ClimateReading>(id);
//        }

//        public List<ClimateReading> GetAllClimateReadings()
//        {
//            return Database.Table<ClimateReading>().ToList();
//        }
//    }
//}