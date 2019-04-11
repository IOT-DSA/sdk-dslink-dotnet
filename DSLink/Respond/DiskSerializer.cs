using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using DSLink.Logging;

namespace DSLink.Respond
{
    public class DiskSerializer
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly Responder _responder;

        public DiskSerializer(Responder responder)
        {
            _responder = responder;
        }

        /// <summary>
        /// Serialize and save the node structure to disk for
        /// loading when the DSLink starts again.
        /// </summary>
        public async Task SerializeToDisk()
        {
            var nodesFileName = _responder.Link.Config.NodesFilename;

            var vfs = _responder.Link.Config.Vfs;
            await vfs.CreateAsync(nodesFileName, true);

            using (var stream = await vfs.WriteAsync(nodesFileName))
            {
                // Finally serialize the object after opening the file.
                var obj = _responder.SuperRoot.Serialize();
                var data = obj.ToString();

                using (var streamWriter = new StreamWriter(stream))
                {
                    await streamWriter.WriteAsync(data).ConfigureAwait(false);
                }

                if (Logger.IsDebugEnabled())
                {
                    Logger.Debug($"Wrote {data} to " + nodesFileName);
                }
            }
        }

        /// <summary>
        /// Deserializes nodes.json from the disk, and restores the node
        /// structure to the loaded data.
        /// </summary>
        /// <returns>True on success</returns>
        public async Task<bool> DeserializeFromDisk()
        {
            var nodesFileName = _responder.Link.Config.NodesFilename;

            try
            {
                var vfs = _responder.Link.Config.Vfs;
                using (var stream = await vfs.ReadAsync(nodesFileName))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var data = await streamReader.ReadToEndAsync();
                        _responder.SuperRoot.Deserialize(JObject.Parse(data));
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to load " + nodesFileName);
                Logger.Warn(e.Message);
                Logger.Warn(e.StackTrace);
                _responder.SuperRoot.ResetNode();
            }

            return false;
        }
    }
}