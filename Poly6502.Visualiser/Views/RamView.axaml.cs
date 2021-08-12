using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Poly6502.Visualiser.Views
{
    public partial class RamView : UserControl
    {
        public RamView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}