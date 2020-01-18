using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using ZIKM.Infrastructure;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Services.Authorization;
using ZIKM.Services.Captcha;
using ZIKM.Services.Storages.Factories;
using ZIKM.Services.Storages.Model;

namespace ZIKM{
    static class Logger{
        private static readonly string logSuccessPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogSuccsess");
        private static readonly string logAllPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogAll");
        private static readonly string logProviderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogProvider");

        static Logger(){
            Directory.CreateDirectory(logSuccessPath);
            Directory.CreateDirectory(logAllPath);
            Directory.CreateDirectory(logProviderPath);
        }

        /// <summary>
        /// Write succesfull operation
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void ToLog(string text){
            ToLogAll(text);
            using StreamWriter writer = new StreamWriter(Path.Combine(logSuccessPath, $"{DateTime.Today.ToString("dd-MM-yyyy")}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        /// <summary>
        /// Write all operation 
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void ToLogAll(string text){
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
            using StreamWriter writer = new StreamWriter(Path.Combine(logAllPath, $"{DateTime.Today.ToString("dd-MM-yyyy")}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        /// <summary>
        /// Write provider comunication operation
        /// </summary>
        /// <param name="data">Log entry</param>
        /// <param name="lenth">Lenth of entry</param>
        public static void ToLogProvider(byte[] data, int lenth) {
            using StreamWriter writer = new StreamWriter(Path.Combine(logProviderPath, $"{DateTime.Today.ToString("dd-MM-yyyy")}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {Encoding.UTF8.GetString(data, 0, lenth)}");
        }
    }

    public static class IoC {
        private static readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static IServiceProvider ServiceProvider { get; }

        static IoC() {
            IServiceCollection services = new ServiceCollection();
            Storage storage = Enum.Parse<Storage>(Environment.GetEnvironmentVariable("Storage") ?? configuration["Storage"]);
            Logger.ToLog($"Storage type: {storage}");

            switch (storage) {
                case Storage.Files:
                    services.AddSingleton<ICaptcha, SimpleCaptcha>();
                    services.AddSingleton<IAuthorization, UserFileStorage>();
                    services.AddSingleton<IStorageFactory, FileStorageFactory>();
                    break;

                case Storage.InternalDB:
                    services.AddSingleton<ICaptcha, GeneratedCaptcha>();
                    services.AddDbContext<StorageContext>(options =>
                        options.UseSqlite(GetInternalConnectionString()));
                    services.AddSingleton<IAuthorization, UserDatabaseStorage>();
                    services.AddSingleton<IStorageFactory, DatabaseStorageFactory>();
                    break;

                case Storage.ExternalDB:
                    services.AddSingleton<ICaptcha, GeneratedCaptcha>();
                    services.AddDbContext<StorageContext>(options =>
                        options.UseMySql(GetExternalConnectionString()));
                    services.AddSingleton<IAuthorization, UserDatabaseStorage>();
                    services.AddSingleton<IStorageFactory, DatabaseStorageFactory>();
                    break;
            }

            ServiceProvider = services.BuildServiceProvider();
        }

        #region Private methods
        private static string GetInternalConnectionString() {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "DB")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "DB"));
            return $@"Data Source={Path.Combine("DB", "StorageDB.db")};";
        }

        private static string GetExternalConnectionString() {
            string connectionString = configuration.GetConnectionString("StorageContext");

            if (Environment.GetEnvironmentVariable("Server") != null)
                connectionString = connectionString.Replace("localhost", Environment.GetEnvironmentVariable("Server"));
            if (Environment.GetEnvironmentVariable("UserID") != null)
                connectionString = connectionString.Replace("root", Environment.GetEnvironmentVariable("UserID"));
            if (Environment.GetEnvironmentVariable("Password") != null)
                connectionString = connectionString.Replace("password", Environment.GetEnvironmentVariable("Password"));
            if (Environment.GetEnvironmentVariable("DBName") != null)
                connectionString = connectionString.Replace("ZikmDB", Environment.GetEnvironmentVariable("DBName"));

            return connectionString;
        }
        #endregion

        /// <summary>
        /// Get service of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Service of type T</returns>
        public static T GetService<T>() {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}