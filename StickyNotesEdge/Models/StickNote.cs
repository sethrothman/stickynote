using System;

namespace StickyNotesEdge.Models
{
    public class StickyNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = "";
        public double X { get; set; }
    }
}
