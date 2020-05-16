using Microsoft.Extensions.DependencyInjection;
using System;
using ZIKM.Server.Utils.StartupUtils;

namespace ZIKM.Server.Utils {
    internal static class IoC {
        private static IServiceProvider ServiceProvider { get; set; }

        static IoC() =>
            ServiceProvider = Startup.Instance.Services;

        /// <summary>
        /// Get service of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Service of type T</returns>
        public static T GetService<T>() {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}