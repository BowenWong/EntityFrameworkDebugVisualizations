using System.Data.Entity;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using EntityFramework.Debug.DebugVisualization.Graph;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Newtonsoft.Json;

namespace EntityFramework.Debug.DebugVisualization
{
    public class DbContextVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            var dbContext = target as DbContext;
            if (dbContext == null)
                return;

            var vertices = dbContext.GetEntityVertices();
            var json = JsonConvert.SerializeObject(vertices);

            var formatter = new BinaryFormatter();
            formatter.Serialize(outgoingData, json);
        }
    }
}