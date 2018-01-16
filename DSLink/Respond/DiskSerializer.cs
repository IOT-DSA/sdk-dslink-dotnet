using DSLink.Platform;
using DSLink.Util.Logger;
using Newtonsoft.Json.Linq;
using StandardStorage;
using System;
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
            JObject obj = _responder.SuperRoot.Serialize();
            IFolder folder = await _responder.Link.Config.StorageFolder;
            IFile file = await folder.CreateFileAsync("nodes.json", CreationCollisionOption.ReplaceExisting);

            if (file != null)
            {
                var data = obj.ToString();
                await file.WriteAllTextAsync(data);
                var path = file.Path;
                if (_responder.Link.Config.LogLevel.DoesPrint(LogLevel.Debug))
                {
                    _responder.Link.Logger.Debug($"Wrote {data} to {path}");
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
                var folder = await _responder.Link.Config.StorageFolder;
                var file = await folder.GetFileAsync("nodes.json");

                if (file != null)
                {
                    var data = await file.ReadAllTextAsync();

                    if (data != null)
                    {
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
