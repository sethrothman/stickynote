using StickyNotesEdge.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StickyNotesEdge
{
    public partial class StickyNoteControl : UserControl
    {
        public StickyNoteControl()
        {
            InitializeComponent();
            Loaded += StickyNoteControl_Loaded;
            NoteRichTextBox.LostFocus += NoteRichTextBox_LostFocus;
        }

        private void StickyNoteControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is StickyNote note)
            {
                NoteRichTextBox.Document = Utilities.XamlToFlowDocument(note.Text);
            }
        }

        private void NoteRichTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is StickyNote note)
            {
                note.Text = Utilities.FlowDocumentToXaml(NoteRichTextBox.Document);
            }
        }
    }
}