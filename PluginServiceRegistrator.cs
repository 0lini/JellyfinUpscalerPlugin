using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using JellyfinUpscalerPlugin.Services;

namespace JellyfinUpscalerPlugin
{
    /// <summary>
    /// Plugin service registrator for dependency injection
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <summary>
        /// Register plugin services
        /// </summary>
        /// <param name="serviceCollection">Service collection</param>
        /// <param name="serverApplicationHost">Server application host</param>
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost serverApplicationHost)
        {
            // Core AI Services
            serviceCollection.AddSingleton<UpscalerCore>();
            
            // Real-time Shader Services
            serviceCollection.AddSingleton<ShaderUpscaler>();
            
            // Video Processing Services (minimal - hardware detection only)
            serviceCollection.AddSingleton<VideoProcessor>();
            
            // Cache Management Services (minimal - compatibility only)
            serviceCollection.AddSingleton<CacheManager>();
            
            // Background Services
            serviceCollection.AddHostedService<UpscalerService>();
            
            // Hardware Benchmark Service
            serviceCollection.AddSingleton<HardwareBenchmarkService>();
            serviceCollection.AddHostedService<HardwareBenchmarkService>(provider => provider.GetService<HardwareBenchmarkService>());
        }
    }
}