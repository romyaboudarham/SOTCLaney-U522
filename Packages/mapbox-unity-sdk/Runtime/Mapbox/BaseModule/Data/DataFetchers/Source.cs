using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.Tiles;
using UnityEngine;

namespace Mapbox.BaseModule.Data.DataFetchers
{
	public abstract class Source
	{
		public abstract bool IsReady();
	}
	
	public abstract class Source<T> : Source
	{
		public Action<CanonicalTileId> DataFetched = (data) => { };
		public Action<T, Action> DataFetchedWithCallback = (data, act) => { };
		public Action<TileErrorEventArgs> FetchingError = (data) => { };

		public virtual IEnumerator Initialize()
		{
			return null;
		}
		
		public virtual void SetupRasterData(T data)
		{

		}

		public virtual void LoadTile(CanonicalTileId tileId) { }

		public virtual IEnumerator LoadTileCoroutine(CanonicalTileId requestedDataTileId, Action<T> callback = null)
		{
			return null;
		}
		
		public virtual IEnumerator LoadTilesCoroutine(IEnumerable<CanonicalTileId> retainedTiles, Action<List<T>> callback = null)
		{
			callback?.Invoke(new List<T>());
			yield break;
		}

		public virtual void LoadBackgroundTile(CanonicalTileId tileId)
		{
			
		}
    
		public virtual void CancelActiveRequests(CanonicalTileId tileId)
		{

		}

		public virtual bool GetInstantData(CanonicalTileId requestedDataTileId, out T data)
		{
			data = default(T);
			return false;
		}

		public virtual void DisposeData(T data)
		{
			
		}

		public virtual void SetMeshGenerationMethod(Action<ByteArrayTile, GameObject, Action<List<GameObject>>> action1)
		{
			
		}

		public abstract bool RetainTiles(HashSet<CanonicalTileId> retainedTiles);

		public abstract bool CheckInstantData(CanonicalTileId tileIdCanonical);

		public virtual void InvalidateData(CanonicalTileId tileId)
		{
			
		}
		
		//move this to imageSource after merging imagery&terrain modules
		public virtual void DownloadAndCacheBaseTiles() { }
		
		public virtual bool IsZinSupportedRange(int z)
		{
			return false;
		}

		public Action<CanonicalTileId> CacheItemDisposed = (t) => { };

		public virtual void Cancel(CanonicalTileId tileId)
		{
			
		}

		public virtual void OnDestroy()
		{
			
		}
	}
}