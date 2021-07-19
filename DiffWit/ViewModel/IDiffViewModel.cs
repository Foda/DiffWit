using CommunityToolkit.Mvvm.Input;

namespace DiffWit.ViewModel
{
    public interface IDiffViewModel
    {
        AsyncRelayCommand GenerateDiff { get; }
        int ChangeCount { get; }
    }
}
