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
            Initialize();
        }

        private void _write(object? value)
        {
            var json = JsonSerializer.Serialize(value);
            lock (_db_lock)
            {
                _databaseFile.Seek(0, SeekOrigin.Begin);
                _dbWriter.Write(json);
            }
        }

        private T? _read<T>()
        {
            string contents;
            lock (_db_lock)
            {
                _databaseFile.Seek(0, SeekOrigin.Begin);
                contents = _dbReader.ReadToEnd();
            }
            return JsonSerializer.Deserialize<T>(contents);
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
            catch(Exception e)
            {
                Console.WriteLine($"Could not delete: {e.Message}.");
            }
            _databaseFile = File.Create(Database);
            _dbReader = new StreamReader(_databaseFile);
            _dbWriter = new StreamWriter(_databaseFile);
        }

        public bool SaveReading(ClimateReading climate)
        {
            LedController.Instance.SetColor(WildernessLabsColors.ChileanFireDark);

            if (climate == null)
                return false;

            try
            {
                climate.ID = 1;
                _write(climate);
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
            try
            {
                return _read<ClimateReading>() ?? new ClimateReading();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
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
            catch(Exception e)
            {
                Console.WriteLine($"{nameof(DatabaseManager)}: {e.Message}");
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
            Console.WriteLine(Environment.StackTrace);
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}