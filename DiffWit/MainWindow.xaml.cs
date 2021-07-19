using DiffWit.ViewModel;
using Microsoft.UI.Xaml;

namespace DiffWit
{
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = new MainWindowViewModel(this);
            MainPage.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.DataContext = ViewModel;
        }

        private async void MenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // var flyoutItem = (MenuFlyoutItem)sender;
            // 
            // var option = flyoutItem.Tag.ToString();
            // if (option == "split")
            // {
            //     SelectedViewIcon.Glyph = "\uF57C";
            //     SelectedViewText.Text = "Split";
            // 
            //     await SetDiffViewModel(_splitDiffViewModel);
            // }
            // else if (option == "unified")
            // {
            //     SelectedViewIcon.Glyph = "\uF57D";
            //     SelectedViewText.Text = "Unified";
            // 
            //     await SetDiffViewModel(_unifiedDiffViewModel);
            // }
        }
    }
}