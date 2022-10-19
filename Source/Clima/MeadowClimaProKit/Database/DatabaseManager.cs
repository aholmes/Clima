using Meadow;
using Meadow.Foundation;
using MeadowClimaProKit.Controller;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeadowClimaProKit.Database
{
    public class DatabaseManager: IDisposable
    {
        private static readonly Lazy<DatabaseManager> instance =
            new Lazy<DatabaseManager>(() => new DatabaseManager());
        public static DatabaseManager Instance => instance.Value;

        bool isConfigured = false;

        //SQLiteConnection Database { get; set; }
        string Database { get; set; }
        FileStream _databaseFile;
        StreamReader _dbReader;
        StreamWriter _dbWriter;

        private static object _db_lock = new object();
        private bool disposedValue;

        private DatabaseManager()
        {
            Console.WriteLine($"{nameof(DatabaseManager)}: ctor called.");
            Initialize();
            Console.WriteLine($"{nameof(DatabaseManager)}: Initialized.");
        }

        protected void Initialize() 
        {
            Console.WriteLine($"{nameof(DatabaseManager)}: Initializing database.");
            var databasePath = Path.Combine(MeadowOS.FileSystem.DataDirectory, "ClimateReadings.db");
            Console.WriteLine($"{nameof(DatabaseManager)}: Creating database at {databasePath}.");
            //Database = new SQLiteConnection(databasePath);
            Database = databasePath;
            try
            {
                File.Delete(Database);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Could not delete: {e.Message}.");
            }
            _databaseFile = File.Create(Database);
            _dbReader = new StreamReader(_databaseFile);
            _dbWriter = new StreamWriter(_databaseFile);

            //Console.WriteLine($"{nameof(DatabaseManager)}: Dropping table {nameof(ClimateReading)}.");
            //Database.DropTable<ClimateReading>(); //convenience while we work on the model object
            Console.WriteLine($"{nameof(DatabaseManager)}: Creating table {nameof(ClimateReading)}.");
            //Database.CreateTable<ClimateReading>();
            Console.WriteLine($"{nameof(DatabaseManager)}: Database created.");
            isConfigured = true;
        }

        public bool SaveReading(ClimateReading climate)
        {
            Console.WriteLine($"{nameof(SaveReading)}: Setting LED color.");
            LedController.Instance.SetColor(WildernessLabsColors.ChileanFireDark);

            Console.WriteLine($"{nameof(SaveReading)}: Checking {nameof(isConfigured)}: {isConfigured}.");
            if (isConfigured == false)
            {
                Console.WriteLine("SaveUpdateReading: DB not ready");
                return false;
            }

            Console.WriteLine($"{nameof(SaveReading)}: Checking {nameof(climate)}: {climate}.");
            if (climate == null)
            {
                Console.WriteLine("SaveUpdateReading: Conditions is null");
                return false;
            }

            Console.WriteLine("Saving climate reading to DB");

            //Database.BeginTransaction();
            //Database.DeleteAll<ClimateReading>();
            //Database.Insert(climate);
            //Database.Commit();
            var allClimateReadings = GetAllClimateReadings();
            Console.WriteLine("Got readings content");
            try
            {
                // +1 to simulate autoincrement columns starting at 1
                allClimateReadings.Add((allClimateReadings.Count + 1).ToString(), climate);
                Console.WriteLine("Serializing");
                var json = JsonConvert.SerializeObject(allClimateReadings);
                Console.WriteLine("Writing");
                _databaseFile.Seek(0, SeekOrigin.Begin);
                _dbWriter.Write(json);
                Console.WriteLine($"{nameof(DatabaseManager)}: Wrote JSON {json}.");

                Console.WriteLine($"Successfully saved to database");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            LedController.Instance.SetColor(Color.Green);
            return true;
        }

        public ClimateReading GetClimateReading(int id)
        {
            var allClimateReadings = GetAllClimateReadings();
            return allClimateReadings.SingleOrDefault(o => o.Key == id.ToString()).Value;

            //return Database.Get<ClimateReading>(id);
        }

        //public List<ClimateReading> GetAllClimateReadings()
        public Dictionary<string, ClimateReading> GetAllClimateReadings()
        {
            try
            {
                Console.WriteLine($"{nameof(GetAllClimateReadings)}: Reading contents.");
                _databaseFile.Seek(0, SeekOrigin.Begin);
                var contents = _dbReader.ReadToEnd();
                Console.WriteLine($"{nameof(DatabaseManager)}: contents: {contents}");
                if (contents.Length == 0) return new Dictionary<string, ClimateReading>();
                return JsonConvert.DeserializeObject<Dictionary<string, ClimateReading>>(contents);
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine($"{nameof(DatabaseManager)}: {e.Message}");
                return new Dictionary<string, ClimateReading>();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return new Dictionary<string, ClimateReading>();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dbReader.Dispose();
                    _dbWriter.Dispose();
                    _databaseFile.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("DISPOSE CALLED");
            Console.WriteLine(Environment.StackTrace);
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}