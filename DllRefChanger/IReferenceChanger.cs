using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChanger
{
    public interface IReferenceChanger
    {
        string Message { get; }
        void Change();
        void UndoChange();
    }
}
