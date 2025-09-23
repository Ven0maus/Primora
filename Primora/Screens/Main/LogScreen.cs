using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using Primora.Screens.Abstracts;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Screens.Main
{
    internal class LogScreen : TextScreen
    {
        private const string Title = "Event Log";
        private readonly Queue<LogEntry> _logEntries = [];

        public static LogScreen Instance { get; private set; }

        public LogScreen(int width, int height) : 
            base(Title, width, height, Game.Instance.Fonts["IBM_8x16"])
        {
            if (Instance != null)
                throw new Exception("An instance for LogScreen already exists.");
            Instance = this;
        }

        private void AddEntry(LogEntry logEntry)
        {
            if (logEntry == null) return;

            _logEntries.Enqueue(logEntry);
            if (_logEntries.Count > View.Height)
                _ = _logEntries.Dequeue();

            UpdateDisplay();
        }

        /// <summary>
        /// Adds a new log entry into the log screen.
        /// </summary>
        /// <param name="logEntry"></param>
        public static void Add(LogEntry logEntry)
        {
            Instance.AddEntry(logEntry);
        }

        public override void UpdateDisplay()
        {
            View.Clear();

            // Parse content and handle
            int row = 0;
            foreach (var logEntry in _logEntries)
            {
                var content = ParseContent(logEntry);
                if (content.Length > View.Width - 1)
                    throw new Exception($"Message \"{new string([.. content.Select(a => a.GlyphCharacter)])}\" succeeds max content width by {content.Length - (View.Width - 2)} characters!");

                View.Print(1, row++, content);
            }
        }

        private static readonly Color _defaultColor = "#adadad".HexToColor();
        private static ColoredGlyph[] ParseContent(LogEntry entry)
        {
            var glyphs = new List<ColoredGlyph>();

            // Use default color for prefix and prepend it to the segments
            var adjustedPrefix = (entry.Prefix, _defaultColor);

            // Add segments
            foreach (var (text, color) in entry.Segments.Prepend(adjustedPrefix))
            {
                var fg = color ?? _defaultColor;
                foreach (var c in text)
                    glyphs.Add(new ColoredGlyph(fg, Color.Transparent, c));
            }

            return [.. glyphs];
        }
    }

    public class LogEntry
    {
        /// <summary>
        /// Each segment contains the text and an optional color.
        /// </summary>
        public readonly List<(string Text, Color? Color)> Segments = [];

        /// <summary>
        /// Timestamp prefix for the log entry.
        /// </summary>
        public string Prefix { get; }

        private LogEntry() 
        {
            Prefix = $"[{World.Instance.Clock:HH:mm}]: ";
        }

        /// <summary>
        /// Create a new log entry with optional colored text.
        /// </summary>
        public static LogEntry New(string content, Color? color = null)
        {
            var entry = new LogEntry();
            entry.Append(content, color);
            return entry;
        }

        /// <summary>
        /// Appends text with optional color.
        /// </summary>
        public LogEntry Append(string content, Color? color = null)
        {
            if (!string.IsNullOrEmpty(content))
                Segments.Add((content, color));
            return this;
        }

        /// <summary>
        /// Appends text with a newline.
        /// </summary>
        public LogEntry AppendLine(string content, Color? color = null)
        {
            if (Segments.Count > 0)
                Segments.Add(("\n", null)); // add line break

            return Append(content, color);
        }

        /// <summary>
        /// Clears all segments.
        /// </summary>
        public LogEntry Clear()
        {
            Segments.Clear();
            return this;
        }

        /// <summary>
        /// Concatenates all segments into a single string (ignoring color).
        /// </summary>
        public string GetText()
        {
            return string.Concat(Segments.Select(s => s.Text));
        }
    }
}
