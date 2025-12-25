using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;

namespace JellyfinUpscalerPlugin.Services
{
    /// <summary>
    /// Lightweight video processor - Real-time streaming only
    /// Pre-processing and caching removed in favor of GLSL shader-based upscaling
    /// </summary>
    public class VideoProcessor : IDisposable
    {
        private readonly ILogger<VideoProcessor> _logger;
        private readonly IMediaEncoder _mediaEncoder;
        private readonly UpscalerCore _upscalerCore;
        private readonly PluginConfiguration _config;
        
        // FFmpeg configuration
        private string _ffmpegPath;
        private string _ffprobePath;
        
        public VideoProcessor(
            ILogger<VideoProcessor> logger,
            IMediaEncoder mediaEncoder,
            UpscalerCore upscalerCore,
            PluginConfiguration config)
        {
            _logger = logger;
            _mediaEncoder = mediaEncoder;
            _upscalerCore = upscalerCore;
            _config = config;
            
            // Initialize FFmpeg paths
            InitializeFFmpeg();
            
            _logger.LogInformation("🎬 VideoProcessor initialized (real-time mode only)");
        }

        /// <summary>
        /// Initialize FFmpeg configuration
        /// </summary>
        private void InitializeFFmpeg()
        {
            try
            {
                _ffmpegPath = _mediaEncoder.EncoderPath;
                _ffprobePath = _mediaEncoder.ProbePath;
                
                if (string.IsNullOrEmpty(_ffmpegPath))
                {
                    _logger.LogWarning("⚠️ FFmpeg path not available from MediaEncoder");
                    return;
                }
                
                _logger.LogInformation($"✅ FFmpeg configured: {_ffmpegPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize FFmpeg");
            }
        }

        /// <summary>
        /// Get hardware detection for shader recommendations
        /// </summary>
        public async Task<HardwareProfile> GetHardwareProfileAsync()
        {
            return await _upscalerCore.DetectHardwareAsync();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}