using DSLink.Util.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DSLink.Respond
{
    public class DiskSerializer
    {
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
            var vfs = _responder.Link.Config.VFS;
            await vfs.CreateAsync("nodes.json", true);
            
            using (var stream = await vfs.WriteAsync("nodes.json"))
            {
                // Finally serialize the object after opening the file.
                JObject obj = _responder.SuperRoot.Serialize();
                var data = obj.ToString();

                using (var streamWriter = new StreamWriter(stream))
                {
                    await streamWriter.WriteAsync(data).ConfigureAwait(false);
                }

                if (_responder.Link.Config.LogLevel.DoesPrint(LogLevel.Debug))
                {
                    _responder.Link.Logger.Debug($"Wrote {data} to nodes.json");
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
            try
            {
                var vfs = _responder.Link.Config.VFS;
                using (var stream = await vfs.ReadAsync("nodes.json"))
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
                _responder.Link.Logger.Warning("Failed to load nodes.json");
                _responder.Link.Logger.Warning(e.Message);
                _responder.Link.Logger.Warning(e.StackTrace);
                _responder.SuperRoot.ResetNode();
            }

            return false;
        }
    }
}
