namespace DllRefChanger.Core
{
    public interface IReferenceChanger
    {
        string Message { get; }

        bool IsUseGitWhenUndo { get; set; }

        void DoChange();

        void UndoChange();
    }
}
