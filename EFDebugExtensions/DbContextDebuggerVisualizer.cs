using System.Data.Entity;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace EntityFramework.Debug
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

            var dbContext = (DbContext)objectProvider.GetObject();
            window.Title += " " + dbContext.GetType().Name;

            window.ShowDialog();
        }
    }
}
