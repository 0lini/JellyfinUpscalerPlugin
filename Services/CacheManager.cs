using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JellyfinUpscalerPlugin.Services
{
    /// <summary>
    /// Minimal cache manager for compatibility - Real caching removed
    /// Real-time shader-based upscaling doesn't require file caching
    /// </summary>
    public class CacheManager : IDisposable
    {
        private readonly ILogger<CacheManager> _logger;
        
        public CacheManager(
            ILogger<CacheManager> logger)
        {
            _logger = logger;
            _logger.LogInformation("📦 Cache manager initialized (real-time mode - no caching)");
        }

        /// <summary>
        /// Get cache statistics (minimal implementation for compatibility)
        /// </summary>
        public CacheStatistics GetCacheStatistics()
        {
            return new CacheStatistics
            {
                TotalEntries = 0,
                TotalSize = 0,
                MaxSize = 0,
                HitRate = 0,
                TotalHits = 0,
                TotalMisses = 0,
                UsagePercentage = 0
            };
        }

        /// <summary>
        /// Get cache stats async (for compatibility)
        /// </summary>
        public async Task<CacheStatistics> GetCacheStatsAsync()
        {
            await Task.CompletedTask;
            return GetCacheStatistics();
        }

        /// <summary>
        /// Clear cache (no-op in real-time mode)
        /// </summary>
        public async Task ClearCacheAsync()
        {
            await Task.CompletedTask;
            _logger.LogInformation("Cache clear requested (real-time mode - no cache to clear)");
        }

        /// <summary>
        /// Pre-process content (disabled in real-time mode)
        /// </summary>
        public async Task<bool> PreProcessContentAsync(
            string inputPath,
            string model,
            int scale,
            string quality,
            VideoProcessor videoProcessor)
        {
            await Task.CompletedTask;
            _logger.LogWarning("Pre-processing is disabled in real-time shader mode");
            return false;
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }

    /// <summary>
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public long TotalSize { get; set; }
        public long MaxSize { get; set; }
        public double HitRate { get; set; }
        public int TotalHits { get; set; }
        public int TotalMisses { get; set; }
        public double UsagePercentage { get; set; }
    }
}
