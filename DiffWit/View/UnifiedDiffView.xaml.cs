using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DiffWit.View
{
    public sealed partial class UnifiedDiffView : UserControl
    {
        public UnifiedDiffView()
        {
            this.InitializeComponent();
        }

        private void NextChange_Click(object sender, RoutedEventArgs e)
        {
            //TextRoot.ScrollToNextAnchor();
        }

        private void PreviousChange_Click(object sender, RoutedEventArgs e)
        {
            //TextRoot.ScrollToPreviousAnchor();
        }
    }
}
