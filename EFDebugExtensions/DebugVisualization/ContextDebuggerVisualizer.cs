using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using EntityFramework.Debug.DebugVisualization.Graph;
using EntityFramework.Debug.DebugVisualization.ViewModels;
using GraphSharp.Controls;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Newtonsoft.Json;
using WPFExtensions.Controls;

namespace EntityFramework.Debug.DebugVisualization
{
    public class ContextDebuggerVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            // DO NOT REMOVE: this dummy calls load the assembly which is required for the xaml parser to know the contained types
            var zoomControl = new ZoomControl();
            var vertexControl = new VertexControl();

            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var resourceName = resourceNames.First(n => n.Contains("DebugWindow.xaml"));

            Window window;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception("Debug window resource not found!");

                window = (Window)XamlReader.Load(stream);
            }

            var jsonSerialized = (string)objectProvider.GetObject();
            var vertices = JsonConvert.DeserializeObject<List<EntityVertex>>(jsonSerialized);

            window.DataContext = new VisualizerViewModel(vertices);
            window.ShowDialog();
        }
    }
}