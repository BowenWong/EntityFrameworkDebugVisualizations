using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace EntityFramework.Debug.DebugVisualization
{
    public class DbContextDebuggerVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var window = new Window
            {
                Title = "DbContext Debugger Visualizer",
                Width = 600,
                Height = 400,
                Background = Brushes.LightBlue,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Title += " " + objectProvider.GetObject();

            window.ShowDialog();
        }
    }
}
