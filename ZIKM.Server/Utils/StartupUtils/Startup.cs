using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;
using System.IO;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Services.Authorization;
using ZIKM.Server.Services.Captcha;
using ZIKM.Server.Services.Protectors;
using ZIKM.Server.Services.Storages.Factories;
using ZIKM.Server.Services.Storages.Model;

namespace ZIKM.Server.Utils.StartupUtils {
    /// <summary>
    /// Setup default services
    /// </summary>
    internal sealed class Startup {
        /// <summary>
        /// Instance of <see cref="Startup"/>
        /// </summary>
        public static Startup Instance { get; } = new Startup();

        public IConfiguration Configuration { get; }

        /// <summary>
        /// <see cref="ServiceProvider"/> containing default server services
        /// </summary>
        public IServiceProvider Services { get; private set; }

        private Startup() {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            if (Environment.GetEnvironmentVariable("Environment") == "Development") {
                configurationBuilder.AddUserSecrets<Startup>();
            }
            else {
                Logger.LogInformation($"Start loading encyption keys");
                Configuration = configurationBuilder.Build();
                string address = Environment.GetEnvironmentVariable("KeySource") ?? Configuration["KeySource"];
                configurationBuilder.AddInMemoryCollection(new KeyLoader(address, new AesRsaProtector()).GetKeys());
                Logger.LogInformation($"Encyption keys loaded");
            }
            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();
            Storage storage = Enum.Parse<Storage>(Environment.GetEnvironmentVariable("Storage") ?? Configuration["Storage"]);
            Logger.LogInformation($"Storage type: {storage}");

            services.AddTransient<IProtector, AesRsaProtector>();
            Logger.LogInformation($"Protector type: Aes+Rsa");

            ProviderLogger.CreateInstance(Configuration["logKey"], Configuration["logVector"]);
            Logger.LogInformation($"Provider log prottection enabled");

            switch (storage) {
                case Storage.Files:
                    services.AddSingleton<ICaptcha, SimpleCaptcha>();
                    Logger.LogInformation("Using captcha from prepeared files");

                    services.AddSingleton<IAuthorization, UserFileStorage>();
                    services.AddSingleton<IStorageFactory, FileStorageFactory>();
                    Logger.LogWarning("Password hashing disabled in this configuration");
                    break;

                case Storage.InternalDB:
                    services.AddDbContext<StorageContext>(options =>
                        options.UseSqlite(GetInternalConnectionString()));
                    AddDBConfigurationComponents(services);
                    break;

                case Storage.ExternalDB:
                    services.AddDbContext<StorageContext>(options =>
                        options.UseMySql(GetExternalConnectionString()));
                    AddDBConfigurationComponents(services);
                    break;
            }

            Services = services.BuildServiceProvider();
        }

        #region Private methods
        private void AddDBConfigurationComponents(IServiceCollection services) {
            services.AddSingleton<ICaptcha, GeneratedCaptcha>();
            Logger.LogInformation("Using generating captcha");

            services.AddSingleton<IAuthorization, UserDatabaseStorage>();
            services.AddSingleton<IStorageFactory, DatabaseStorageFactory>();
            Logger.LogInformation("Password hashing enabled");
        }

        private string GetInternalConnectionString() {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "DB")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "DB"));
            return $@"Data Source={Path.Combine("DB", "StorageDB.db")};";
        }

        private string GetExternalConnectionString() {
            ConfigurationDecryptor decryptor = new ConfigurationDecryptor(Configuration["key"]);
            var connectionString = new SqlConnectionStringBuilder(decryptor.Decrypt(Configuration.GetConnectionString("StorageContext")));

            string server = Environment.GetEnvironmentVariable("Server");
            string userId = Environment.GetEnvironmentVariable("UserID");
            string password = Environment.GetEnvironmentVariable("Password");
            string dBName = Environment.GetEnvironmentVariable("DBName");

            if (server != null)
                connectionString["Server"] = decryptor.Decrypt(server);
            if (userId != null)
                connectionString["User ID"] = decryptor.Decrypt(userId);
            if (password != null)
                connectionString["Password"] = decryptor.Decrypt(password);
            if (dBName != null)
                connectionString["DBName"] = decryptor.Decrypt(dBName);

            return connectionString.ConnectionString;
        }
        #endregion
    }
}
