using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickyNotesEdge.Services
{
    public interface IDialogService
    {
        bool ShowConfirmation(string title, string message);
    }

}
