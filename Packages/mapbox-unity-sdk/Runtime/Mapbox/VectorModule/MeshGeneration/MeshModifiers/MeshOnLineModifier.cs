using System;
using System.Collections.Generic;
using Mapbox.BaseModule.Data;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers
{
	[Serializable]
	public class MeshOnLineModifier : MeshModifier
	{
		public float Distance;
		public PrefabSet PrefabSet;
		private System.Random _random;

		private bool _doScale;
		private bool _doRotate;

		public bool AlternateUV = false;
		private int _alternateCounter = 0;
		private List<List<Vector2>> _uvs = new List<List<Vector2>>()
		{
			new List<Vector2>(),
			new List<Vector2>()
		};

		// public MeshOnLineModifier()
		// {
		// 	Initialize();
		// }

		public sealed override void Initialize()
		{
			var tileScale = (float) (1 / Conversions.TileBoundsInWebMercator(new UnwrappedTileId(16, 17650, 24245)).Size.x);
			_random = new System.Random();
			PrefabSet.MeshDatas = new List<MeshData>(PrefabSet.Prefabs.Count);
			foreach (var prefab in PrefabSet.Prefabs)
			{
				var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
				var meshData = new MeshData();

				var newVertices = new List<Vector3>();
				var newNormals = new List<Vector3>();

				var meshVertes = mesh.vertices;
				var meshNormals = mesh.normals;
				var rotation = prefab.transform.rotation;
				var pos = prefab.transform.position;

				for (int i = 0; i < meshVertes.Length; i++)
				{
					newVertices.Add(rotation * ((pos + meshVertes[i]) * prefab.transform.localScale.x) * tileScale);
					newNormals.Add(rotation * meshNormals[i]);
				}
				meshData.Vertices = newVertices;
				meshData.Normals = newNormals;

				for (int i = 0; i < mesh.subMeshCount; i++)
				{
					meshData.Triangles.Add(new List<int>());
					mesh.GetTriangles(meshData.Triangles[i], i);
				}
				for (int i = 0; i < mesh.subMeshCount; i++)
				{
					meshData.UV.Add(new List<Vector2>());
					mesh.GetUVs(i, meshData.UV[i]);
				}
				PrefabSet.MeshDatas.Add(meshData);
			}

			if (PrefabSet.ScaleVariety.x != 1 || PrefabSet.ScaleVariety.y != 1)
				_doScale = true;

			if (PrefabSet.RotationVariety.x != 0 || PrefabSet.RotationVariety.y != 0)
				_doRotate = true;

			foreach (var uv in PrefabSet.MeshDatas[0].UV[0])
			{
				_uvs[0].Add(uv/2);
				_uvs[1].Add(new Vector2((uv.x / 2) + .5f, (uv.y / 2) + .5f));
			}
		}

		public override void Run(VectorFeatureUnity feature, MeshData md, IMapInformation mapInfo)
		{
			var unityDistance = Distance * (1 / mapInfo.Scale);
			md.Vertices = new List<Vector3>();
			md.Normals = new List<Vector3>();
			md.Tangents = new List<Vector4>();
			for (int i = 0; i < 2; i++)
			{
				md.Triangles.Add(new List<int>());
			}

			var rotation = feature.Properties.ContainsKey("angle")
				? float.Parse(feature.Properties["angle"].ToString())
				: 0;
			var leftover = 0f;
			foreach (var subfeature in feature.Points)
			{
				for (int j = 0; j < subfeature.Count - 1; j++)
				{
					var first = subfeature[j];
					var second = subfeature[j + 1];
					var lineDistance = (second - first).magnitude;
					var dir = (second - first).normalized;
					var count = Mathf.FloorToInt((lineDistance + leftover) / unityDistance);
					var lineRot = Quaternion.LookRotation(dir, Vector3.up);
					var firstPosition = first + (dir * (unityDistance - leftover));
					leftover = (lineDistance - (unityDistance - leftover)) - ((count-1) * unityDistance);

					for (int u = 0; u < count; u++)
					{
						var position = firstPosition + (u * dir * unityDistance);
						var rotY = 0f;
						if (_doRotate)
						{
							rotY = Mathf.Lerp(Mathf.Max(-360, PrefabSet.RotationVariety.x), Mathf.Min(360, PrefabSet.RotationVariety.y), (float) _random.NextDouble());
						}
						var rot = Quaternion.Euler(0, rotation + (lineRot.eulerAngles.y + rotY), 0);

						var randomMeshData = PrefabSet.MeshDatas[_random.Next(0, PrefabSet.MeshDatas.Count)];
						var start = md.Vertices.Count;
						var itemDelay = (Vector4)position;
						itemDelay.y = _random.Next(0, 10) / 3f;
						if(_doScale)
						{
							var scale = Mathf.Lerp(PrefabSet.ScaleVariety.x, PrefabSet.ScaleVariety.y, (float) _random.NextDouble());
							for (int i = 0; i < randomMeshData.Vertices.Count; i++)
							{
								md.Vertices.Add(position +
								                rot * (randomMeshData.Vertices[i]) //rot
								                    * scale); //scale
								md.Tangents.Add(itemDelay);
								md.Normals.Add(rot * randomMeshData.Normals[i]);
							}
						}
						else
						{
							for (int i = 0; i < randomMeshData.Vertices.Count; i++)
							{
								md.Vertices.Add(position +
								                rot * (randomMeshData.Vertices[i])); //scale
								md.Tangents.Add(itemDelay);
								md.Normals.Add(rot * randomMeshData.Normals[i]);
							}
						}



						for (var submeshIndex = 0; submeshIndex < randomMeshData.Triangles.Count; submeshIndex++)
						{
							var submesh = randomMeshData.Triangles[submeshIndex];
							for (var index = 0; index < submesh.Count; index++)
							{
								var i = submesh[index];
								md.Triangles[submeshIndex].Add(start + i);
							}
						}

						//md.Triangles = randomMeshData.Triangles;
						if (!AlternateUV)
						{
							md.UV[0].AddRange(randomMeshData.UV[0]);
						}
						else
						{
							var index = _alternateCounter % 2;
							if (index == 0)
							{
								md.UV[0].AddRange(_uvs[0]);
							}
							else if (index == 1)
							{
								md.UV[0].AddRange(_uvs[1]);
							}

							_alternateCounter++;
						}

						//md.Normals = randomMeshData.Normals;
					}

				}
			}
		}
	}
}