using StickyNotesEdge.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace StickyNotesEdge
{
    public class NoteManager
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StickyNotesEdge", "StickyNotes.json");

        public ObservableCollection<StickyNote> Notes { get; set; } = new();

        public NoteManager()
        {
            Load();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(Notes));
        }

        public void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                Notes = JsonSerializer.Deserialize<ObservableCollection<StickyNote>>(json) ?? new();
            }
        }
    }
}
