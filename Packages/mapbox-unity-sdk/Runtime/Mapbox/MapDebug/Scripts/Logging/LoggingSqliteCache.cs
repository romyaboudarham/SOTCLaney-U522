using System;
using Mapbox.BaseModule.Data.Platform.Cache.SQLiteCache;
using Mapbox.BaseModule.Data.Tasks;
using Mapbox.BaseModule.Data.Tiles;
using Newtonsoft.Json.Linq;

namespace Mapbox.MapDebug.Scripts.Logging
{
    public class MockSqliteCache : LoggingSqliteCache
    {
        public MockSqliteCache(TaskManager taskManager, uint? maxTileCount = null) : base(taskManager, maxTileCount, "_MOCK")
        {
        }

    }
    
    public class LoggingSqliteCache : SqliteCache, ILogWriter
    {
        private int _addCount = 0;
        private int _getCount = 0;
        private int _getExpirationCount = 0;
        private int _updateCount = 0;
        private int _removeCount = 0;
        
        public LoggingSqliteCache(TaskManager taskManager, uint? maxTileCount = null, string folderNamePostFix = "") : base(taskManager, maxTileCount, folderNamePostFix)
        {
        }


        protected override void Add(string tilesetName, CanonicalTileId tileId, byte[] data, string path, string etag, DateTime? expirationDate, bool forceInsert = false)
        {
            _addCount++;
            base.Add(tilesetName, tileId, data, path, etag, expirationDate, forceInsert);
        }

        public override T Get<T>(string tilesetName, CanonicalTileId tileId)
        {
            _getCount++;
            return base.Get<T>(tilesetName, tileId);
        }

        public override void ReadEtagAndExpiration<T>(T data)
        {
            _getExpirationCount++;
            base.ReadEtagAndExpiration(data);
        }

        public override void UpdateExpiration(string tilesetName, CanonicalTileId tileId, DateTime expirationDate)
        {
            _updateCount++;
            base.UpdateExpiration(tilesetName, tileId, expirationDate);
        }

        public override int RemoveData(string tilesetName, int zoom, int x, int y)
        {
            _removeCount++;
            return base.RemoveData(tilesetName, zoom, x, y);
        }

        public void ResetStats()
        {
            _addCount = 0;
            _getCount = 0;
            _getExpirationCount = 0;
            _updateCount = 0;
            _removeCount = 0;
        }

        public JObject DumpLogs()
        {
            return null;
        }

        public string PrintScreen()
        {
            return string.Format("Sqlite Cache | C:{0} R:{1}, U:{2}, D:{3}, Exp:{4}", _addCount, _getCount, _updateCount, _removeCount, _getExpirationCount);
        }
    }
}