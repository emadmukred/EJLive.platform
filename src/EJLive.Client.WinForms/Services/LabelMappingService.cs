using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace EJLive.Client.WinForms.Services
{
    public class LabelMappingService
    {
        private readonly Dictionary<string, string> _labels = new();

        public LabelMappingService(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                try
                {
                    var json = File.ReadAllText(jsonPath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        foreach (var kv in dict)
                            _labels[kv.Key] = kv.Value;
                    }
                }
                catch
                {
                    // fallback to empty mapping
                }
            }
        }

        public string Get(string key, string fallback = "")
        {
            if (key == null) return fallback;
            return _labels.TryGetValue(key, out var v) ? v : fallback;
        }

        public void Set(string key, string value)
        {
            _labels[key] = value ?? "";
        }
    }
}
