namespace DllRefChanger.Changer
{
    public interface IReferenceChanger
    {
        string Message { get; }

        bool UseDefaultCheckCanChange { get; set; }

        void Change();

        void UndoChange();
    }
}
