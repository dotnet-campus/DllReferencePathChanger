using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChanger
{
    interface IReferenceChanger
    {
        void Change();
        void UndoChange();
    }
}
