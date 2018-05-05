namespace DllRefChanger.Core
{
    public interface IReferenceChanger
    {
        string Message { get; }

        bool UseDefaultCheckCanChange { get; set; }

        void DoChange();

        void UndoChange();
    }
}
