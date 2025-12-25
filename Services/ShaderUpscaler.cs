using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JellyfinUpscalerPlugin.Services
{
    /// <summary>
    /// GLSL Shader-based real-time upscaling service
    /// Manages shader compilation, loading, and performance monitoring
    /// </summary>
    public class ShaderUpscaler : IDisposable
    {
        private readonly ILogger<ShaderUpscaler> _logger;
        private readonly PluginConfiguration _config;
        private readonly UpscalerCore _upscalerCore;
        
        // Shader cache
        private readonly Dictionary<string, ShaderInfo> _availableShaders = new();
        private ShaderConfig? _shaderConfig;
        
        // Performance monitoring
        private readonly Dictionary<string, ShaderPerformanceMetrics> _performanceMetrics = new();
        
        // Shader directory
        private readonly string _shaderDirectory;
        
        public ShaderUpscaler(
            ILogger<ShaderUpscaler> logger,
            PluginConfiguration config,
            UpscalerCore upscalerCore)
        {
            _logger = logger;
            _config = config;
            _upscalerCore = upscalerCore;
            
            // Determine shader directory path
            _shaderDirectory = Path.Combine(
                Path.GetDirectoryName(typeof(ShaderUpscaler).Assembly.Location) ?? "",
                "Shaders"
            );
            
            InitializeShaders();
        }

        /// <summary>
        /// Initialize and load available shaders
        /// </summary>
        private void InitializeShaders()
        {
            try
            {
                _logger.LogInformation("🎨 Initializing GLSL shaders for real-time upscaling...");
                
                // Load shader configuration
                LoadShaderConfig();
                
                // Load shader source files
                LoadShaderFiles();
                
                _logger.LogInformation($"✅ Loaded {_availableShaders.Count} GLSL shaders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize shaders");
            }
        }

        /// <summary>
        /// Load shader configuration from JSON
        /// </summary>
        private void LoadShaderConfig()
        {
            try
            {
                var configPath = Path.Combine(_shaderDirectory, "shader-config.json");
                
                if (!File.Exists(configPath))
                {
                    _logger.LogWarning($"⚠️ Shader config not found: {configPath}");
                    return;
                }
                
                var jsonContent = File.ReadAllText(configPath);
                _shaderConfig = JsonSerializer.Deserialize<ShaderConfig>(jsonContent);
                
                _logger.LogInformation($"📋 Loaded shader configuration with {_shaderConfig?.Shaders?.Count ?? 0} shaders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load shader configuration");
            }
        }

        /// <summary>
        /// Load shader source files
        /// </summary>
        private void LoadShaderFiles()
        {
            if (!Directory.Exists(_shaderDirectory))
            {
                _logger.LogWarning($"⚠️ Shader directory not found: {_shaderDirectory}");
                return;
            }
            
            var shaderFiles = Directory.GetFiles(_shaderDirectory, "*.glsl");
            
            foreach (var shaderFile in shaderFiles)
            {
                try
                {
                    var shaderId = Path.GetFileNameWithoutExtension(shaderFile);
                    var shaderSource = File.ReadAllText(shaderFile);
                    
                    // Find corresponding config
                    var config = _shaderConfig?.Shaders?.FirstOrDefault(s => s.Id == shaderId);
                    
                    var shaderInfo = new ShaderInfo
                    {
                        Id = shaderId,
                        Name = config?.Name ?? shaderId,
                        Description = config?.Description ?? "",
                        SourceCode = shaderSource,
                        Category = config?.Category ?? "general",
                        Performance = config?.Performance ?? "balanced",
                        Quality = config?.Quality ?? "medium",
                        SupportedScales = config?.SupportedScales ?? new List<int> { 2 },
                        Features = config?.Features ?? new List<string>()
                    };
                    
                    _availableShaders[shaderId] = shaderInfo;
                    
                    _logger.LogDebug($"📄 Loaded shader: {shaderId} ({shaderInfo.Name})");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to load shader: {shaderFile}");
                }
            }
        }

        /// <summary>
        /// Get list of available shaders
        /// </summary>
        public List<ShaderInfo> GetAvailableShaders()
        {
            return _availableShaders.Values.ToList();
        }

        /// <summary>
        /// Get shader by ID
        /// </summary>
        public ShaderInfo? GetShader(string shaderId)
        {
            _availableShaders.TryGetValue(shaderId, out var shader);
            return shader;
        }

        /// <summary>
        /// Get shader source code
        /// </summary>
        public string? GetShaderSource(string shaderId)
        {
            var shader = GetShader(shaderId);
            return shader?.SourceCode;
        }

        /// <summary>
        /// Validate shader for compilation
        /// </summary>
        public async Task<ShaderValidationResult> ValidateShaderAsync(string shaderId)
        {
            await Task.CompletedTask; // Placeholder for async work
            
            var shader = GetShader(shaderId);
            if (shader == null)
            {
                return new ShaderValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { $"Shader '{shaderId}' not found" }
                };
            }
            
            // Basic validation (in real implementation, could use GLSL validator)
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(shader.SourceCode))
            {
                errors.Add("Shader source code is empty");
            }
            
            if (!shader.SourceCode.Contains("#version"))
            {
                errors.Add("Missing GLSL version directive");
            }
            
            if (!shader.SourceCode.Contains("void main()"))
            {
                errors.Add("Missing main() function");
            }
            
            return new ShaderValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// Get recommended shader based on hardware profile
        /// </summary>
        public async Task<string> GetRecommendedShaderAsync()
        {
            try
            {
                // Get hardware profile
                var hardware = await _upscalerCore.DetectHardwareAsync();
                
                // Determine GPU tier
                string gpuTier = "default";
                
                if (!string.IsNullOrEmpty(hardware.GpuModel))
                {
                    var gpuLower = hardware.GpuModel.ToLower();
                    
                    if (gpuLower.Contains("rtx 4070") || gpuLower.Contains("rtx 4080") || gpuLower.Contains("rtx 4090"))
                    {
                        gpuTier = "rtx4070";
                    }
                    else if (gpuLower.Contains("rtx 3060") || gpuLower.Contains("rtx 3070") || gpuLower.Contains("rtx 3080"))
                    {
                        gpuTier = "rtx3060";
                    }
                    else if (gpuLower.Contains("rtx 2060") || gpuLower.Contains("rtx 2070") || gpuLower.Contains("rtx 2080"))
                    {
                        gpuTier = "rtx2060";
                    }
                    else if (gpuLower.Contains("gtx 1060") || gpuLower.Contains("gtx 1070") || gpuLower.Contains("gtx 1080"))
                    {
                        gpuTier = "gtx1060";
                    }
                }
                
                // Get recommendation from config
                var recommendation = _shaderConfig?.HardwareRecommendations?.GetValueOrDefault(gpuTier)
                    ?? _shaderConfig?.HardwareRecommendations?.GetValueOrDefault("default");
                
                return recommendation?.RecommendedShader ?? "ravu";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get recommended shader, using default");
                return "ravu"; // Fallback to fastest shader
            }
        }

        /// <summary>
        /// Record shader performance metrics
        /// </summary>
        public void RecordPerformance(string shaderId, ShaderPerformanceMetrics metrics)
        {
            _performanceMetrics[shaderId] = metrics;
        }

        /// <summary>
        /// Get shader performance metrics
        /// </summary>
        public ShaderPerformanceMetrics? GetPerformanceMetrics(string shaderId)
        {
            _performanceMetrics.TryGetValue(shaderId, out var metrics);
            return metrics;
        }

        /// <summary>
        /// Get all performance metrics
        /// </summary>
        public Dictionary<string, ShaderPerformanceMetrics> GetAllPerformanceMetrics()
        {
            return new Dictionary<string, ShaderPerformanceMetrics>(_performanceMetrics);
        }

        /// <summary>
        /// Get shader presets
        /// </summary>
        public Dictionary<string, ShaderPreset>? GetPresets()
        {
            return _shaderConfig?.Presets;
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }

    /// <summary>
    /// Shader information
    /// </summary>
    public class ShaderInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string SourceCode { get; set; } = "";
        public string Category { get; set; } = "";
        public string Performance { get; set; } = "";
        public string Quality { get; set; } = "";
        public List<int> SupportedScales { get; set; } = new();
        public List<string> Features { get; set; } = new();
    }

    /// <summary>
    /// Shader validation result
    /// </summary>
    public class ShaderValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Shader performance metrics
    /// </summary>
    public class ShaderPerformanceMetrics
    {
        public string ShaderId { get; set; } = "";
        public double AverageFPS { get; set; }
        public double MinFPS { get; set; }
        public double MaxFPS { get; set; }
        public double GpuUsagePercent { get; set; }
        public double VramUsageMB { get; set; }
        public DateTime LastUpdated { get; set; }
        public int SampleCount { get; set; }
    }

    /// <summary>
    /// Shader configuration (deserialized from JSON)
    /// </summary>
    public class ShaderConfig
    {
        public List<ShaderConfigEntry>? Shaders { get; set; }
        public Dictionary<string, ShaderPreset>? Presets { get; set; }
        public Dictionary<string, HardwareRecommendation>? HardwareRecommendations { get; set; }
    }

    public class ShaderConfigEntry
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string Performance { get; set; } = "";
        public string Quality { get; set; } = "";
        public List<int>? SupportedScales { get; set; }
        public List<string>? Features { get; set; }
    }

    public class ShaderPreset
    {
        public string Shader { get; set; } = "";
        public int Scale { get; set; }
        public string Description { get; set; } = "";
    }

    public class HardwareRecommendation
    {
        public string RecommendedShader { get; set; } = "";
        public int MaxScale { get; set; }
        public string Preset { get; set; } = "";
    }
}
