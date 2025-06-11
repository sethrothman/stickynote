using CommunityToolkit.Mvvm.Input;
using StickyNotesEdge.Models;
using StickyNotesEdge.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StickyNotesEdge.ViewModels
{
    public class MainViewModel : INotifyCollectionChanged
    {
        private ObservableCollection<StickyNote> _notes = [];
        public ICommand DeleteNoteCommand { get; }
        public ICommand MagnifyNoteCommand { get; }
        public ICommand AddNoteCommand { get; }
        public ICommand LostFocusCommand { get; }

        private NoteManager _noteManager = new();
        private bool _notePopupIsOpen;

        public bool NotePopupIsOpen
        {
            get => _notePopupIsOpen;
            set
            {
                _notePopupIsOpen = value;
                OnPropertyChanged(nameof(NotePopupIsOpen));
            }
        }

        public ObservableCollection<StickyNote> Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public IDialogService _dialogService { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event Action<StickyNote> RequestLargeNoteWindow;
        public event Action RequestScrollToEnd;
        public event Action<StickyNote> NewNoteAdded;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel(IDialogService dialogService)
        {
            _noteManager.Load();
            _notes = _noteManager.Notes;
            DeleteNoteCommand = new RelayCommand<StickyNote>(DeleteNote);
            MagnifyNoteCommand = new RelayCommand<StickyNote>(MagnifyNote);
            AddNoteCommand = new RelayCommand(AddNote);
            LostFocusCommand = new RelayCommand<StickyNote>(OnNoteLostFocus);

            _dialogService = dialogService;
        }

        private void DeleteNote(StickyNote? note)
        {
            NotePopupIsOpen = true;
            if (note != null && _dialogService.ShowConfirmation("Confirm Deletion", "Delete note?"))
            { 
                _noteManager.ArchiveDeletedNote(note);
                Notes.Remove(note);
                SaveChanges(Notes);
            }
            NotePopupIsOpen = false;
        }

        private void MagnifyNote(StickyNote? note)
        {
            if (note != null) 
            {
                NotePopupIsOpen = true;
                RequestLargeNoteWindow?.Invoke(note);
            }
        }

        private void AddNote()
        {
            var note = new StickyNote
            {
                Text = string.Empty,
                SequenceNumber = Notes.Count + 1
            };
            Notes.Add(note);
            SaveChanges(Notes);

            RequestScrollToEnd?.Invoke();
            NewNoteAdded?.Invoke(note);
        }

        private void OnNoteLostFocus(StickyNote note)
        {
            // Save logic here
            SaveChanges(Notes);
        }

        public void SaveChanges(IList<StickyNote> notes)
        { 
            _noteManager.SaveNotes(notes);
        }
    }

}
