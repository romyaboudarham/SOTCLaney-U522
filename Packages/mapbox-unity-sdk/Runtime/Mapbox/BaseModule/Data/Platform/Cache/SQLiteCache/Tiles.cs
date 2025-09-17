﻿using System;
using Mapbox.BaseModule.Data.Platform.SQLite;

namespace Mapbox.BaseModule.Data.Platform.Cache.SQLiteCache
{

	/// <summary>
	/// Don't change the class name: sqlite-net uses it for table creation
	/// </summary>
	public class tiles
	{
		

		[PrimaryKey, AutoIncrement]
		public int id { get; set; }
		
		public int tile_set { get; set; }

		//hrmpf: multiple PKs not supported by sqlite.net
		//https://github.com/praeclarum/sqlite-net/issues/282
		//TODO: do it via plain SQL
		//[PrimaryKey]
		public int zoom_level { get; set; }

		//[PrimaryKey]
		public long tile_column { get; set; }

		//[PrimaryKey]
		public long tile_row { get; set; }

		public byte[] tile_data { get; set; }

		public string tile_path { get; set; }
		
		/// <summary>Unix epoch for simple FIFO pruning </summary>
		public int timestamp { get; set; }

		/// <summary> ETag Header value of the reponse for auto updating cache</summary>
		public string etag { get; set; }

		/// <summary>Expiration date of cached data </summary>
		public int? expirationDate { get; set; }
		
		[Ignore]
		public DateTime expirationDateFormatted  { get; set; }
	}

	public class offlineMaps
	{
		[PrimaryKey, AutoIncrement]
		public int id { get; set; }
		public string name { get; set; }
	}

	public class tile2offline
	{
		public int tileId { get; set; }
		public int mapId { get; set; }
	}
}
