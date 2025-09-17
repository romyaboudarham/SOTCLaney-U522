using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapbox.BaseModule.Data.DataFetchers;
using Mapbox.BaseModule.Data.Tasks;
using Mapbox.BaseModule.Data.Tiles;
using UnityEngine;

namespace Mapbox.BaseModule.Data.Platform.Cache
{
	public interface IFileCache
	{
		bool TestAvailability();
		event Action<MapboxTileData, string> FileSaved;
		void Add(MapboxTileData textureCacheItem, bool forceInsert, Action<string> post);
		//bool GetAsync(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<CacheItem> callback);
		bool Exists(CanonicalTileId tileId, string mapId);
		void ClearAll();
		void DeleteTileFile(MapboxTileData cacheItem);
		HashSet<string> GetFileList();
		bool GetAsync<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new();
		IEnumerator GetFileCoroutine<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new();
		void DeleteByFileRelativePath(string fileRelativePath);
	}

	public class FileCache : IFileCache
	{
		public event Action<MapboxTileData, string> FileSaved = (cacheItem, s) => { };

		protected string CacheRootFolderName = "Mapbox/FileCache";
		public string PersistantCacheRootFolderPath;
		private string FileExtension = "png";

		protected FileDataFetcher _fileDataFetcher;
		protected Dictionary<string, string> MapIdToFolderNameDictionary;

		private TaskManager _taskManager;
		
		public FileCache(TaskManager taskManager, string folderNamePostFix = "")
		{
			CacheRootFolderName += folderNamePostFix;
			PersistantCacheRootFolderPath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, CacheRootFolderName));
			_taskManager = taskManager;
			_fileDataFetcher = new FileDataFetcher();
			MapIdToFolderNameDictionary = new Dictionary<string, string>();

			TestAvailability();
		}

		public bool TestAvailability()
		{
			if (!Directory.Exists(PersistantCacheRootFolderPath))
			{
				Directory.CreateDirectory(PersistantCacheRootFolderPath);
			}

			if (!Directory.Exists(PersistantCacheRootFolderPath))
				return false;

			try
			{
				string filePath = RelativeFilePathToFileInfoExpects("MapboxTestFie.txt");
				string content = "Mapbox";
				File.WriteAllText(filePath, content);
				string actualContent = File.ReadAllText(filePath);
				if (actualContent == content)
				{
					File.Delete(filePath);
					return true;
				}
				else
				{
					return false;
				}

			}
			catch
			{
				//throw;
				return false;
			}
		}

		private string TileToRelativeFilePath(CanonicalTileId tileId, string tilesetId)
		{
			return string.Format("{0}/{1}{2}{3}.{4}", MapIdToFolderName(tilesetId), tileId.X, tileId.Y, tileId.Z, FileExtension);
		}
		
		public virtual bool Exists(CanonicalTileId tileId, string mapId)
		{
			var info = new FileInfo(TileToPathFileInfoExpects(tileId, mapId));
			return info.Exists;
		}

		public virtual void Add(MapboxTileData textureCacheItem, bool forceInsert, Action<string> postSave)
		{
			var fileRelativePath = TileToRelativePath(textureCacheItem);
			var infoWrapper = new InfoWrapper(textureCacheItem, fileRelativePath, postSave);
			SaveInfo(infoWrapper);
		}

		public virtual bool GetAsync<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new()
		{
			string relativePath = TileToRelativeFilePath(tileId, tilesetId);
			var info = new FileInfo(RelativeFilePathToFileInfoExpects(relativePath));
			if (info.Exists)
			{
				var fullPath = RelativePathToUnityRequestExpects(relativePath);
				_fileDataFetcher.FetchData<T>(fullPath, tileId, tilesetId, isTextureNonreadable, callback);
			}
			else
			{
				
			}

			return info.Exists;
		}
		
		public virtual IEnumerator GetFileCoroutine<T>(CanonicalTileId tileId, string tilesetId, bool isTextureNonreadable, Action<T> callback) where T : RasterData, new()
		{
			string relativePath = TileToRelativeFilePath(tileId, tilesetId);
			var info = new FileInfo(RelativeFilePathToFileInfoExpects(relativePath));
			if (info.Exists)
			{
				var fullFilePath = RelativePathToUnityRequestExpects(relativePath);
				var finished = false;
				yield return _fileDataFetcher.FetchDataCoroutine<T>(fullFilePath, tileId, tilesetId,
					isTextureNonreadable,
					(data) =>
					{
						finished = true;
						callback(data);
					});
				while (!finished) yield return null;
			}
			else
			{
				callback(null);
			}
		}

		public void DeleteByFileRelativePath(string fileRelativePath)
		{
			var path = RelativeFilePathToFileInfoExpects(fileRelativePath);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			else
			{
				Debug.Log($"File {path} does not exist");
			}
		}

		public virtual void ClearAll()
		{
			DirectoryInfo di = new DirectoryInfo(PersistantCacheRootFolderPath);

			foreach (DirectoryInfo folder in di.GetDirectories())
			{
				ClearFolder(folder.FullName);
			}
		}

		public virtual void DeleteTileFile(MapboxTileData cacheItem)
		{
			var filePath = TileToPathFileInfoExpects(cacheItem);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		public virtual HashSet<string> GetFileList()
		{
			var pathList = new HashSet<string>();
			if (Directory.Exists(PersistantCacheRootFolderPath))
			{
				var dir = Directory.GetDirectories(PersistantCacheRootFolderPath);
				foreach (var rasterDirectory in dir)
				{
					var directoryInfo = new DirectoryInfo(rasterDirectory);
					var files = directoryInfo.GetFiles();
					foreach (var fileInfo in files)
					{
						pathList.Add(FullFilePathToRelativePath(fileInfo.FullName));
					}
				}
			}

			return pathList;
		}

		

		protected virtual void SaveInfo(InfoWrapper info)
		{
			if (info.TextureCacheItem == null)
			{
				return;
			}

			string folderPath = RelativeFilePathToFileInfoExpects(MapIdToFolderName(info.TextureCacheItem.TilesetId));

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			_taskManager.AddTask(
				new TaskWrapper(info.TextureCacheItem.TileId.GenerateKey(info.TextureCacheItem.TilesetId, "FileCache"))
				{
					TileId = info.TextureCacheItem.TileId,
					TilesetId = info.TextureCacheItem.TilesetId,
					Action = () =>
					{
						var fullPath = RelativeFilePathToFileInfoExpects(info.Path);
						FileStream sourceStream = new FileStream(
							RelativeFilePathToFileInfoExpects(info.Path),
							FileMode.Create, FileAccess.Write, FileShare.Read,
							bufferSize: 4096, useAsync: false);

						sourceStream.Write(info.TextureCacheItem.Data, 0, info.TextureCacheItem.Data.Length);
						sourceStream.Close();

						var finalRelativePath = FullFilePathToRelativePath(fullPath);
						info.PostSaveAction(finalRelativePath);
						//Debug.Log(string.Format("File saved {0} - {1}", info.TextureCacheItem.TileId, info.Path));
						OnFileSaved(info.TextureCacheItem, finalRelativePath);
					},
					ContinueWith = (t) =>
					{
						
					},
#if UNITY_EDITOR
					Info = "FileCache.SaveInfo"
#endif
				}, 4);
		}

		

		protected virtual void OnFileSaved(MapboxTileData infoTextureCacheItem, string path)
		{
			FileSaved(infoTextureCacheItem, path);
		}

		private string MapIdToFolderName(string mapId)
		{
			if (MapIdToFolderNameDictionary.ContainsKey(mapId))
			{
				return MapIdToFolderNameDictionary[mapId];
			}
			var folderName = mapId;
			var chars = Path.GetInvalidFileNameChars();
			foreach (Char c in chars)
			{
				folderName = folderName.Replace(c, '-');
			}
			MapIdToFolderNameDictionary.Add(mapId, folderName);
			return folderName;
		}

		private void ClearFolder(string folderPath)
		{
			DirectoryInfo di = new DirectoryInfo(folderPath);

			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}

			di.Delete();
		}

		protected class InfoWrapper
		{
			public MapboxTileData TextureCacheItem;
			public string Path;
			public Action<string> PostSaveAction;

			public InfoWrapper(MapboxTileData textureCacheItem, string path, Action<string> postSave)
			{
				TextureCacheItem = textureCacheItem;
				Path = path;
				PostSaveAction = postSave;
			}
		}

		private string TileToRelativePath(MapboxTileData cacheItem)
		{
			return TileToRelativeFilePath(cacheItem.TileId, cacheItem.TilesetId);
		}
		
		public string TileToPathFileInfoExpects(MapboxTileData cacheItem)
		{
			return RelativeFilePathToFileInfoExpects(TileToRelativeFilePath(cacheItem.TileId, cacheItem.TilesetId));
		}
		
		public string TileToPathFileInfoExpects(CanonicalTileId tileId, string tilesetId)
		{
			return RelativeFilePathToFileInfoExpects(TileToRelativeFilePath(tileId, tilesetId));
		}
		
		public string RelativeFilePathToFileInfoExpects(string relativeFilePath)
		{
			var fullPath = Path.GetFullPath(Path.Combine(PersistantCacheRootFolderPath, relativeFilePath)); 
			return fullPath;
		}

		public string RelativePathToUnityRequestExpects(string relativeFilePath)
		{
			var fullPath = Path.GetFullPath(Path.Combine(PersistantCacheRootFolderPath, relativeFilePath));
			//I'm not sure if there's a better way to do this "file://" thing but unity needs that
			//otherwise it adds https which of course fails the web requests
			return fullPath.Insert(0, "file://");
		}
		
		private string FullFilePathToRelativePath(string fileInfoFullName)
		{
			return fileInfoFullName.Substring(PersistantCacheRootFolderPath.Length,
				fileInfoFullName.Length - PersistantCacheRootFolderPath.Length).Trim('/').Trim('\\');
		}

		public static bool ClearAllFiles()
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "Mapbox"));
				foreach (DirectoryInfo folder in di.GetDirectories())
				{
					if (folder.Name.StartsWith("FileCache"))
					{
						folder.Delete(true);
					}
				}

				return true;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return false;
			}
		}
	}
}
