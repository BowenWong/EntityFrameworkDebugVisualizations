using System.Data.Entity;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace EntityFramework.Debug.DebugVisualization
{
    public class DbContextVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            var dbContext = target as DbContext;
            if (dbContext == null)
                return;

            var formatter = new BinaryFormatter();
            formatter.Serialize(outgoingData, dbContext.GetType().FullName);
        }
    }
}