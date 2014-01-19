using System;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Newtonsoft.Json;

namespace EntityFramework.Debug.DebugVisualization
{
    public class ContextVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            if (!(target is IObjectContextAdapter))
                throw new ArgumentException("This debugger visualizer only works with an IObjectContextAdapter.");

            var vertices = ((IObjectContextAdapter)target).ObjectContext.GetEntityVertices();
            var json = JsonConvert.SerializeObject(vertices, new JsonSerializerSettings{PreserveReferencesHandling = PreserveReferencesHandling.All});

            var formatter = new BinaryFormatter();
            formatter.Serialize(outgoingData, json);
        }
    }
}