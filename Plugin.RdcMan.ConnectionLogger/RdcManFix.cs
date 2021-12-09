using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RdcMan;

namespace RdcPlgTest
{
    public static class RdcManFix
    {
        private static readonly PropertyInfo _pluginsProperty;
        private static readonly PropertyInfo _nameProperty;

        static RdcManFix()
        {
            var assembly = typeof(IPlugin).Assembly;
            var programType = assembly.GetType("RdcMan.Program");
            var configType = programType.GetNestedType("PluginConfig", BindingFlags.NonPublic | BindingFlags.Static);

            _pluginsProperty = programType.GetProperty("Plugins", BindingFlags.NonPublic | BindingFlags.Static);
            _nameProperty = configType.GetProperty("Name");

            // private static Dictionary<string, Program.PluginConfig> Plugins { get; set; } // in RdcMan.Program, key == name
            // private class PluginConfig
            // {
            //   public string Name { get; set; }
            // ...
            // }
        }

        public static void ApplyFix()
        {
            // Rewrite Plugins dictionary to have names and keys match the plugin names used when loading a plugin configuration
            // Plugin configurations are stored using fully qualified names of plugin class types while the settings are loaded using the assembly name of the plugin.
            var rewritten = new List<(string oldName, string newName, object config)>();

            var pluginsDictionary = _pluginsProperty.GetGetMethod(true).Invoke(null, null) as IDictionary;

            foreach (DictionaryEntry kv in pluginsDictionary)
            {
                var key = (string)kv.Key;

                if (!key.Contains(","))
                {
                    continue;
                }

                var newKey = key.Split(',')[1].Trim();
                _nameProperty.SetValue(kv.Value, newKey);
                rewritten.Add(((string)kv.Key, newKey, kv.Value));
            }

            foreach (var (oldName, newName, config) in rewritten)
            {
                pluginsDictionary.Remove(oldName);
                pluginsDictionary.Add(newName, config);
            }
        }
    }
}