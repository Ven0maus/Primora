using Primora.Core.Items.Interfaces;
using Primora.Extensions;
using SadRogue.Primitives;
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
        /// Read helper to parse attributes to the correct type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributes"></param>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static T GetAttribute<T>(Dictionary<string, object> attributes, string attributeKey)
        {
            if (attributes != null && attributes.TryGetValue(attributeKey, out var value))
            {
                // Direct type support (int, long, double)
                if (value is not string && value is T tValue)
                    return tValue;

                var type = typeof(T);

                // Double conversion support
                if (value is double fValue)
                {
                    if (type == typeof(float))
                        return (T)Convert.ChangeType((float)fValue, type);

                    throw new Exception($"Not support double conversion to type \"{type.Name}\".");
                }

                if (value is string sValue && !string.IsNullOrWhiteSpace(sValue))
                {
                    // Enum, color, char, int support
                    if (type.IsEnum)
                        return (T)Enum.Parse(type, sValue, true);
                    if (type == typeof(Color))
                        return (T)Convert.ChangeType(sValue.HexToColor(), type);
                    if (type == typeof(char) && sValue.Length == 1)
                        return (T)Convert.ChangeType(sValue[0], type);
                    if (type == typeof(int) && int.TryParse(sValue, out var intResult))
                        return (T)Convert.ChangeType(intResult, type);
                }

                throw new Exception($"Not supported attribute value conversion for key \"{attributeKey}\" from \"{value.GetType().Name}\" to \"{type.Name}\".");
            }
            return default;
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
