using StickyNotesEdge.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StickyNotesEdge
{
    public class NoteManager
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StickyNotesEdge", "stickynotes.json");

        private readonly string _deletedFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StickyNotesEdge", "deletednotes.json");

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
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var notes = JsonSerializer.Deserialize<ObservableCollection<StickyNote>>(json);
                    Notes = notes ?? new ObservableCollection<StickyNote>();
                    // Actually sort
                    Notes = [.. Notes.OrderBy(n => n.SequenceNumber)];
                }
                catch (Exception ex)
                {
                    // Log or notify user
                    Notes = [];
                }
            }
        }

        public void SaveNotes(IList<StickyNote> notes)
        {
            var notesData = notes.Select(n => new
            {
                n.Id,
                n.Text,
                n.SequenceNumber
            });
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(notesData));
        }

        // New method: Archive a deleted note to deletednotes.json
        public void ArchiveDeletedNote(StickyNote note)
        {
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_deletedFilePath)!);

            // Read existing deleted notes (if any)
            List<DeletedStickyNote> deletedNotes = [];
            if (File.Exists(_deletedFilePath))
            {
                string json = File.ReadAllText(_deletedFilePath);
                deletedNotes = JsonSerializer.Deserialize<List<DeletedStickyNote>>(json) ?? [];
            }

            // Add the newly deleted note
            deletedNotes.Add(new DeletedStickyNote
            {
                Id = note.Id,
                Text = note.Text,
                SequenceNumber = note.SequenceNumber,
                DeletedDate = DateTime.Now
            });

            // Save updated list
            File.WriteAllText(_deletedFilePath, JsonSerializer.Serialize(deletedNotes));
        }
    }
}
