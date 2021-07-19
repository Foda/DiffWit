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

            ViewModel = new MainWindowViewModel();
            MainPage.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.DataContext = ViewModel;

            await ViewModel.RefreshDiff(@"C:\Users\corsa\Desktop\diff_test\file_a.txt",
                                        @"C:\Users\corsa\Desktop\diff_test\file_b.txt");
        }

        private async void MenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // var flyoutItem = (MenuFlyoutItem)sender;
            // 
            // var option = flyoutItem.Tag.ToString();
            // if (option == "split")
            // {
            //     SelectedViewIcon.Glyph = "\uF30A";
            //     SelectedViewText.Text = "Split";
            // 
            //     await SetDiffViewModel(_splitDiffViewModel);
            // }
            // else if (option == "unified")
            // {
            //     SelectedViewIcon.Glyph = "\uF309";
            //     SelectedViewText.Text = "Unified";
            // 
            //     await SetDiffViewModel(_unifiedDiffViewModel);
            // }
        }
    }
}