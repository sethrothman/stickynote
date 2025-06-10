using System;

namespace StickyNotesEdge.Models
{
    public class StickyNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Text { get; set; } = "";
        public int SequenceNumber { get; set; }
    }

    public class DeletedStickyNote
    {
        public string Id { get; set; }
        public string? Text { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime DeletedDate { get; set; }
    }

}
