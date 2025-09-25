using Primora.Core.Items.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Primora.GameData.Helpers
{
    /// <summary>
    /// Helper to load game data.
    /// </summary>
    internal static class GameDataLoader
    {
        /// <summary>
        /// Helper method to load game data configuration files.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static Dictionary<string, T> Load<T>(string path) where T : class, IEditorObject
        {
            if (!File.Exists(path))
            {
                Debug.WriteLine("File not found: " + path);
                return [];
            }

            Dictionary<string, T> configs;
            try
            {
                var json = File.ReadAllText(path);
                configs = JsonSerializer.Deserialize<Dictionary<string, T>>(json, Constants.General.SerializerOptions);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to process file \"{path}\": {e.Message}", e);
            }

            // Set name key and adjust json elements
            foreach (var value in configs)
            {
                value.Value.Name = value.Key;
                value.Value.Attributes = ConvertJsonElements(value.Value.Attributes);
            }

            return configs;
        }

        /// <summary>
        /// Helper to convert attributes element into its real object types.
        /// </summary>
        /// <param name="deserializedData"></param>
        /// <returns></returns>
        private static Dictionary<string, object> ConvertJsonElements(Dictionary<string, object> deserializedData)
        {
            if (deserializedData == null) return null;
            if (deserializedData.Count == 0) return deserializedData;

            var newData = new Dictionary<string, object>();
            foreach (var kv in deserializedData)
            {
                var jsonElement = (JsonElement)kv.Value;
                object value = jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString(),
                    JsonValueKind.Number => ConvertNumber(jsonElement),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => jsonElement
                };
                newData[kv.Key] = value;
            }
            return newData;
        }

        /// <summary>
        /// Helper to convert number type into its real type.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static object ConvertNumber(JsonElement element)
        {
            if (element.TryGetInt32(out int i))
                return i; // prefer int
            if (element.TryGetInt64(out long l))
                return l; // fallback to long
            return element.GetDouble(); // floating point
        }
    }
}
