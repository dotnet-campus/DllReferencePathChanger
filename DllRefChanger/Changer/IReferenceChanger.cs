namespace DllRefChanger.Changer
{
    public interface IReferenceChanger
    {
        string Message { get; }
        void Change();
        void UndoChange();
    }
}
