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
            base(Title, width, height)
        {
            if (Instance != null)
                throw new Exception("An instance for LogScreen already exists.");
            Instance = this;
        }

        private void AddEntry(LogEntry logEntry)
        {
            if (logEntry == null) return;

            _logEntries.Enqueue(logEntry);
            if (_logEntries.Count > View.Height - 2)
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
            int row = 1;
            foreach (var logEntry in _logEntries)
            {
                var content = ParseContent(logEntry.Content);
                if (content.Length > View.Width - 2)
                    throw new Exception($"Message \"{new string([.. content.Select(a => a.GlyphCharacter)])}\" succeeds max content width by {content.Length - (View.Width - 2)} characters!");

                View.Print(1, row++, content);
            }
        }

        private static ColoredGlyph[] ParseContent(string content)
        {
            // TODO: Improve to parse colors from content string
            var defaultColor = "#adadad".HexToColor();
            return [.. content.Select(a => new ColoredGlyph(defaultColor, Color.Transparent, a))];
        }
    }

    public class LogEntry
    {
        public string Content { get; private set; } = string.Empty;

        private LogEntry()
        { }

        public static LogEntry New(string content, Color? color = null)
        {
            var entry = new LogEntry();
            entry.Append(content, color);
            return entry;
        }

        public LogEntry Append(string content, Color? color = null)
        {
            // TODO: Put color into the content with formatting
            Content += content;
            return this;
        }

        public LogEntry AppendLine(string content, Color? color = null)
        {
            if (Content.Length == 0)
                return Append(content, color);

            Content += "\n" + content;
            return this;
        }

        public LogEntry Clear()
        {
            Content = string.Empty;
            return this;
        }
    }
}
