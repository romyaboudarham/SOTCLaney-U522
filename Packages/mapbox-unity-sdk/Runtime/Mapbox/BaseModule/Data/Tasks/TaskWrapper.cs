using System;
using System.Threading;
using System.Threading.Tasks;
using Mapbox.BaseModule.Data.Tiles;

namespace Mapbox.BaseModule.Data.Tasks
{
    public class TaskWrapper
    {
        public int Id;
        public string TilesetId;
        public int EnqueueFrame;
        public float StartingTime;
        public float FinishedTime;
        public CanonicalTileId TileId;
        public CanonicalTileId OwnerTileId;
        public Action Action;
        public Action<Task> ContinueWith;
        public Action OnCancelled = () => {};
        public string Info;
        
        public TaskWrapper(int id)
        {
            Id = id;
        }
    }
}