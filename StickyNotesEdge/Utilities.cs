using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace StickyNotesEdge
{
    internal static class Utilities
    {
        public static string FlowDocumentToXaml(FlowDocument document)
        {
            if (document == null)
                return string.Empty;

            var range = new TextRange(document.ContentStart, document.ContentEnd);
            using var ms = new MemoryStream();
            range.Save(ms, DataFormats.Xaml);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static FlowDocument XamlToFlowDocument(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return new FlowDocument();

            try
            {
                if (text.TrimStart().StartsWith("<FlowDocument"))
                {
                    var doc = new FlowDocument();
                    var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                    using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
                    range.Load(ms, DataFormats.Xaml);
                    return doc;
                }
                else
                {
                    return new FlowDocument(new Paragraph(new Run(text)));
                }
            }
            catch
            {
                return new FlowDocument(new Paragraph(new Run(text ?? string.Empty)));
            }
        }

    }
}
