using System.Data.Entity;
using System.IO;
using System.Text;
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

            using (var streamWriter = new StreamWriter(outgoingData, Encoding.UTF8, 100, true))
            {
                streamWriter.Write(dbContext.GetType().FullName);
                streamWriter.Flush();
            }
        }
    }
}