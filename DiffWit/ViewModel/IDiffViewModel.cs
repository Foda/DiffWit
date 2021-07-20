using CommunityToolkit.Mvvm.Input;

namespace DiffWit.ViewModel
{
    public interface IDiffViewModel
    {
        AsyncRelayCommand<(string fileA, string fileB)> GenerateDiff { get; }
        int ChangeCount { get; }
    }
}
