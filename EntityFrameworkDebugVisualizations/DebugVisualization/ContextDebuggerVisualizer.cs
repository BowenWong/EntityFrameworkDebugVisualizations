using System.Collections.Generic;
using EntityFramework.Debug.DebugVisualization.Graph;
using EntityFramework.Debug.DebugVisualization.ViewModels;
using EntityFramework.Debug.DebugVisualization.Views;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Newtonsoft.Json;
using WPFExtensions.Converters;

namespace EntityFramework.Debug.DebugVisualization
{
    public class ContextDebuggerVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            // DO NOT REMOVE: this dummy call loads the WpfExtensions assembly
            var dummy = new BoolToVisibilityConverter();

            var jsonSerialized = (string)objectProvider.GetObject();
            var vertices = JsonConvert.DeserializeObject<List<EntityVertex>>(jsonSerialized);

            var window = new MainWindow { DataContext = new VisualizerViewModel(vertices) };
            window.Loaded += (o, e) => window.Activate();
            window.ShowDialog();
        }
    }
}