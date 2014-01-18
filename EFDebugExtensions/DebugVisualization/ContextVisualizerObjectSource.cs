using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using EntityFramework.Debug.DebugVisualization.Graph;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Newtonsoft.Json;

namespace EntityFramework.Debug.DebugVisualization
{
    public class ContextVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            var contextAdapter = target as IObjectContextAdapter;
            var context = contextAdapter != null ? contextAdapter.ObjectContext : target as ObjectContext;
            if (context == null)
                return;

            var vertices = context.GetEntityVertices();
            var json = JsonConvert.SerializeObject(vertices, new JsonSerializerSettings{PreserveReferencesHandling = PreserveReferencesHandling.All});

            var formatter = new BinaryFormatter();
            formatter.Serialize(outgoingData, json);
        }
    }
}