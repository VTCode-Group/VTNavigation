using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using VTNavigation.Editor;
using VTNavigation.Geometry;
using VTNavigation.Navigation;
using VTNavigation.Tree;
using VTNavigation.Util;
using VTPartition.Scene;
using HashCode = VTNavigation.Tree.HashCode;

namespace VTNavigation.Scene
{
	public enum ESceneStatus
	{
		Unloaded = 0,
		Loading = 1,
		Loaded = 2,
	}
	
	public class VTSceneGroup:IMapGroup
	{
		private int m_SubSceneXCount;
		private int m_SubSceneYCount;
		private int m_SubSceneZCount;
		private float m_MinVoxelSizeWS; //	World Size : Tree Size
		private VTScene[] m_VTScenes;
		private ESceneStatus[] m_VTScenesStatus;
		private int[] m_VTScenesReferenceCounts;
		private Bounds m_SceneBounds;

		public VTSceneGroup(int subScenXCount, int subSceneYCount, int subSceneZCount, float minVoxelSizeWS,
			Vector3 sceneOrigin)
		{
			m_SubSceneXCount = subScenXCount;
			m_SubSceneYCount = subSceneYCount;
			m_SubSceneZCount = subSceneZCount;
			m_MinVoxelSizeWS = minVoxelSizeWS;
			
			float sceneSize = CalcSceneSize(minVoxelSizeWS);
			Vector3 totalSize = new Vector3(
				subScenXCount * sceneSize,
				subSceneYCount * sceneSize,
				subSceneZCount * sceneSize);
			Bounds sceneBounds = new Bounds(sceneOrigin + totalSize * 0.5f, totalSize);

			m_SceneBounds = sceneBounds;
			
			m_VTScenes = new VTScene[SubSceneCount];
			m_VTScenesStatus = new ESceneStatus[SubSceneCount];
			m_VTScenesReferenceCounts = new int[SubSceneCount];
			
			Array.Fill(m_VTScenesStatus, ESceneStatus.Unloaded);
			Array.Fill(m_VTScenesReferenceCounts, 0);
		}

		private float SceneSize
		{
			get
			{
				return CalcSceneSize(m_MinVoxelSizeWS);
			}
		}

		public Bounds SceneBounds
		{
			get
			{
				return m_SceneBounds;
			}
		}

		public int SubSceneXCount
		{
			get
			{
				return m_SubSceneXCount;
			}
		}

		public int SubSceneYCount
		{
			get
			{
				return m_SubSceneYCount;
			}
		}

		public int SubSceneZCount
		{
			get
			{
				return m_SubSceneZCount;
			}
		}

		public int SubSceneCount
		{
			get
			{
				return m_SubSceneXCount * m_SubSceneYCount * m_SubSceneZCount;
			}
		}

		public Vector3 Origin
		{
			get
			{
				return m_SceneBounds.min;
			}
		}

		public VTSceneGroup()
		{

		}

		public void SetSubScene(int x, int y, int z, VTScene scene)
		{
			int index = GetIndex(x, y, z);
			if (index < 0 || index >= SubSceneCount)
				return;
			m_VTScenes[index] = scene;
			scene.XIndex = x;
			scene.YIndex = y;
			scene.ZIndex = z;
		}

		public VTScene ReferenceSubScene(int x, int y, int z)
		{
			int index = GetIndex(x, y, z);
			if (index < 0 || index >= SubSceneCount)
				return null;
			m_VTScenesReferenceCounts[index]++;
			return m_VTScenes[index];
		}

		public void UnReferenceSubScene(int x, int y, int z)
		{
			int index = GetIndex(x, y, z);
			if (index < 0 || index >= SubSceneCount)
				return;
			m_VTScenesReferenceCounts[index]--;
		}

		public VTScene this[int x, int y, int z]
		{
			get
			{
				int index = GetIndex(x, y, z);
				if (index < 0 || index >= SubSceneCount)
					return null;
				return m_VTScenes[index];
			} 
			set
			{
				int index = GetIndex(x, y, z);
				if (index < 0)
				{
					index = 0;
				}
				if (index >= SubSceneCount)
				{
					index = SubSceneCount - 1;
				}
				m_VTScenes[index] = value;
			}
		}

		public VTScene this[int index]
		{
			get
			{
				if (index < 0 || index >= SubSceneCount)
					return null;
				return m_VTScenes[index];
			}
		}

		public int GetIndex(int x, int y, int z)
		{
			return y * SubSceneXCount * SubSceneZCount + z * SubSceneXCount + x;
		}

		public void ReadGroupDataFromMemory(BinaryReader br)
		{
			m_SubSceneXCount = IOUtil.ReadInt(br);
			m_SubSceneYCount = IOUtil.ReadInt(br);
			m_SubSceneZCount = IOUtil.ReadInt(br);
			m_MinVoxelSizeWS = IOUtil.ReadFloat(br);
			m_SceneBounds = IOUtil.ReadBounds(br);
			m_VTScenes = new VTScene[SubSceneCount]; 
		}

		public void ReadVTSceneFromMemory(int x, int y, int z, BinaryReader br)
		{
			if (this[x, y, z] == null)
			{
				VTScene scene = new VTScene();
				scene.ReadFromMemory(br);
				this[x, y, z] = scene;
			}
		}

		public void ReadGroupDataFromFile(string filePath)
		{
			using (var fs = File.OpenRead(filePath))
			{
				using (var br = new BinaryReader(fs))
				{
					m_SubSceneXCount = IOUtil.ReadInt(br);
					m_SubSceneYCount = IOUtil.ReadInt(br);
					m_SubSceneZCount = IOUtil.ReadInt(br);
					m_MinVoxelSizeWS = IOUtil.ReadFloat(br);
					m_SceneBounds = IOUtil.ReadBounds(br);
				}
			}
		}

		public void ReadAllScene(string directory, string sceneName)
		{
			m_VTScenes = new VTScene[SubSceneCount];
			for (int x = 0; x < m_SubSceneXCount; x++)
			{
				for (int z = 0; z < m_SubSceneZCount; z++)
				{
					for (int y = 0; y < m_SubSceneYCount; y++)
					{
						int index = GetIndex(x, y, z);
						m_VTScenes[index] = new VTScene();
						string path = Path.Combine(directory, sceneName + $"_{x}_{y}_{z}.vtscene");
						m_VTScenes[index].ReadFromFile(path);
						m_VTScenes[index].XIndex = x;
						m_VTScenes[index].YIndex = y;
						m_VTScenes[index].ZIndex = z;
					}
				}
			}
		}

		public void ReadAllFromFile(string filePath)
		{
			string directory = Path.GetDirectoryName(filePath);
			string sceneName = Path.GetFileNameWithoutExtension(filePath);
			ReadGroupDataFromFile(filePath);

			ReadAllScene(directory, sceneName);
		}

		public Bounds GetSubSceneBounds(int x, int y, int z)
		{
			Vector3 sceneSize = m_SceneBounds.size;
			Vector3 singleSceneSize = new Vector3(sceneSize.x / m_SubSceneXCount, sceneSize.y / m_SubSceneYCount, sceneSize.z / m_SubSceneZCount);


			Vector3 min = Origin + Vector3.right * singleSceneSize.x * x + Vector3.up * singleSceneSize.y * y + Vector3.forward * singleSceneSize.z * z;
			Vector3 center = min + singleSceneSize * 0.5f;
			return new Bounds(center, singleSceneSize);
		}

		public (int, int, int) GetLocatedSceneIndices(Vector3 position)
		{
			Vector3 sceneSize = m_SceneBounds.size;
			Vector3 origin = Origin;
			Vector3 singleSceneSize = new Vector3(sceneSize.x / m_SubSceneXCount, sceneSize.y / m_SubSceneYCount, sceneSize.z / m_SubSceneZCount);
			Vector3 localPositionInScene = position - origin;
			float x = localPositionInScene.x / singleSceneSize.x;
			float y = localPositionInScene.y / singleSceneSize.y;
			float z = localPositionInScene.z / singleSceneSize.z;
			return (Mathf.FloorToInt(x), Mathf.FloorToInt(y), Mathf.FloorToInt(z));
		}

		public void SetLoadedScene(int x, int y, int z, VTScene scene)
		{
			this[x, y, z] = scene;
		}

		public void ClearScene(int x, int y, int z)
		{
			VTScene scene = this[x, y, z];
			scene.Reset();
			this[x, y, z] = null;
		}

		public bool TryGetSceneBlockBounds(int x, int y, int z, out List<Bounds> bounds)
		{
			VTScene scene = this[x, y, z];
			if(scene != null)
			{
				bounds = scene.GetBlockBounds();
				return true;
			}
			bounds = null;
			return false;
		}
		
		public void GetWalkableAreas(MapHashCode mapHashCode, HashSet<MapHashCode> ignoreSet, out List<MapHashCode> result)
		{
			result = new List<MapHashCode>();
			GetWalkableAreasInternal(mapHashCode, ignoreSet, result,-1, 0, 0);
			GetWalkableAreasInternal(mapHashCode, ignoreSet, result, 1, 0, 0);
			GetWalkableAreasInternal(mapHashCode, ignoreSet, result, 0,-1, 0);
			GetWalkableAreasInternal(mapHashCode, ignoreSet, result, 0, 1, 0);
			GetWalkableAreasInternal(mapHashCode, ignoreSet, result, 0, 0,-1);
			GetWalkableAreasInternal(mapHashCode, ignoreSet, result, 0, 0, 1);
		}

		public bool FastRayCastHit(Ray rayWS, out float minDistance, float edageError = 0.2f)
		{
			Profiler.BeginSample("RayCastHit");
			minDistance = float.MaxValue;
			if (!m_SceneBounds.Contains(rayWS.origin))
			{
				return false;
			}

			Ray currentRay = rayWS;
			
			bool hit = false;
			do
			{
				IMap currentMap = GetMap(rayWS.origin);
				
				if(currentMap == null)break;
				
				hit = currentMap.FastRayCastHit(currentRay, out float resultMinDistance, edageError);
				if (!hit)
				{
					currentMap.SceneBounds.IntersectsRayInside(currentRay, out float nearestDistance);
					Vector3 intersectPoint = currentRay.GetPoint(nearestDistance) + currentRay.direction * 0.02f;
					currentRay = new Ray(intersectPoint, rayWS.direction);
				}
				else
				{
					minDistance = resultMinDistance;
				}
			}while(!hit && m_SceneBounds.Contains(currentRay.origin));
			Profiler.EndSample();
			return hit;
		}

		private void GetWalkableAreasInternal(MapHashCode mapHashCode, HashSet<MapHashCode> ignoreSet,List<MapHashCode> result,
			int xoffset, int yoffset, int zoffset)
		{
			MapHashCode nextMapHashCode = ToNextHashCode(mapHashCode, xoffset, yoffset, zoffset);
			IMap nextMap = nextMapHashCode.map;
			HashCode nextCode = nextMapHashCode.hashCode;
			if (nextMap == null || !nextCode.IsValide || nextMap.IsBlock(nextCode))
			{
				return;
			}

			if (!nextMap.IsSparse(nextCode))
			{
				if (!ignoreSet.Contains(nextMapHashCode))
				{
					result.Add(nextMapHashCode);
				}
			}
			else
			{
				List<HashCode> edgeFreeSpaceCodes = new List<HashCode>();
				int filterXMask = -1; 
				int filterYMask = -1;
				int filterZMask = -1;
				if (xoffset != 0)
				{
					filterXMask = xoffset == 1 ? 0 : 1;
				}

				if (yoffset != 0)
				{
					filterYMask = yoffset == 1 ? 0 : 1;
				}

				if (zoffset != 0)
				{
					filterZMask = zoffset == 1 ? 0 : 1;
				}
 				nextMap.GetEdgeFreeSpace(nextCode, edgeFreeSpaceCodes, filterXMask, filterYMask, filterZMask);
			    for (int i = 0; i < edgeFreeSpaceCodes.Count; i++)
			    {
				    MapHashCode edgeFreeSpace = new MapHashCode(){map = nextMap, hashCode = edgeFreeSpaceCodes[i]};
				    if (!ignoreSet.Contains(edgeFreeSpace))
				    {
					    result.Add(edgeFreeSpace);
				    }
			    }
			}
		}
		
		private static float CalcSceneSize(float voxelSize)
		{
			return 1024f * voxelSize;
		}
		
		#region ------Navigation Map------

		public IMap GetMap(int x, int y, int z)
		{
			if (m_VTScenes.Length <= 0)
			{
				return null;
			}
			if (x < 0 || x >= m_SubSceneXCount || y < 0 || y >= m_SubSceneYCount || z < 0 || z >= m_SubSceneZCount)
			{
				return null;
			}
			
			int index = GetIndex(x, y, z);
			return m_VTScenes[index];
		}

		public IMap GetMap(Vector3 position)
		{
			if (m_VTScenes.Length <= 0)
			{
				return null;
			}
			if (!m_SceneBounds.Contains(position))
			{
				return null;
			}
			Vector3 offset = position - Origin;
			float sceneSize = SceneSize;
			int x = Mathf.FloorToInt(offset.x / sceneSize);
			int y = Mathf.FloorToInt(offset.y / sceneSize);
			int z = Mathf.FloorToInt(offset.z / sceneSize);

			return GetMap(x, y, z);
		}

		public MapHashCode GetHashCode(Vector3 position, int layer = 0)
		{
			VTScene scene = (VTScene)GetMap(position);
			if (scene == null)
			{
				return new MapHashCode(){map = null, hashCode = HashCode.INVALID_CODE};
			}

			return new MapHashCode() { map = scene, hashCode = scene.GetHashCode(position, layer) };
		}

		public MapHashCode ToNextHashCode(MapHashCode mapHashCode, int xoffset, int yoffset, int zoffset)
		{
			mapHashCode.hashCode.Decode(out UInt32 x, out UInt32 y, out UInt32 z, out int layer);
			int coordinateCount = HashCode.LayerCoordinateCount(layer);
			
			int nextX = (int)x + xoffset;
			int nextY = (int)y + yoffset;
			int nextZ = (int)z + zoffset;

			(int subSceneX, int subSceneY, int subSceneZ) = mapHashCode.map.GetSubSceneIndex();
			
			IMap nextMap = mapHashCode.map;
			if (nextX < 0 || nextY < 0 || nextZ < 0 || nextX >= coordinateCount || nextY >= coordinateCount ||
			    nextZ >= coordinateCount)
			{
				//	超出curMap
				if (nextX < 0)
				{
					subSceneX--;
					nextX = coordinateCount + nextX;
				}
				else if (nextX >= coordinateCount)
				{
					subSceneX++;
					nextX = nextX - coordinateCount;
				}

				if (nextY < 0)
				{
					subSceneY--;
					nextY = coordinateCount + nextY;
				}
				else if (nextY >= coordinateCount)
				{
					subSceneY++;
					nextY = nextY - coordinateCount;
				}

				if (nextZ < 0)
				{
					subSceneZ--;
					nextZ = coordinateCount + nextZ;
				}
				else if (nextZ >= coordinateCount)
				{
					subSceneZ++;
					nextZ = nextZ - coordinateCount;
				}

				nextMap = GetMap(subSceneX, subSceneY, subSceneZ);
				if (nextMap == null)
				{
					return new MapHashCode() { map = null, hashCode = HashCode.INVALID_CODE };
				}
			}

			return new MapHashCode()
			{
				map = nextMap, 
				hashCode = HashCode.Encode((UInt32)nextX, (UInt32)nextY, (UInt32)nextZ, layer)
			};
		}

		#endregion
		
		private void ForeachScene(Action<int, int, int, int, VTScene> callback)
		{
			for (int x = 0; x < m_SubSceneXCount; x++)
			{
				for (int z = 0; z < m_SubSceneZCount; z++)
				{
					for (int y = 0; y < m_SubSceneYCount; y++)
					{
						int index = GetIndex(x, y, z);
						callback(x, y, z, index, m_VTScenes[index]);
					}
				}
			}
		}
#if UNITY_EDITOR

		public BakeWork BakeVTSceneWithCustomVoxelSize(float minVoxelSize = 1.0f)
		{
			List<Triangle> triangles = VTSceneUtil.GetAllMeshTrianglesInWorldSpace();

			List<Triangle> terrainTriangles = new List<Triangle>();
			if (Terrain.activeTerrain != null)
			{
				terrainTriangles = VTSceneUtil.GetActiveTerrainTriangleInWorldSpace();
			}
			triangles.AddRange(terrainTriangles);

			Bounds triangleBounds = GeometryUtil.GetTriangleAABB(triangles);

			Bounds sceneBounds = triangleBounds;

			List<Bounds> terrainBounds = new List<Bounds>();

			Vector3 sceneBoundsMin = sceneBounds.min;
			Vector3 sceneBoundsMax = sceneBounds.max;


			m_MinVoxelSizeWS = minVoxelSize;

			float maxTreeSizeInWorldSpace = OCTree.MaxTreeSize * m_MinVoxelSizeWS;

			m_SubSceneXCount = Mathf.CeilToInt(sceneBounds.size.x / maxTreeSizeInWorldSpace);
			m_SubSceneYCount = Mathf.CeilToInt(sceneBounds.size.y / maxTreeSizeInWorldSpace);
			m_SubSceneZCount = Mathf.CeilToInt(sceneBounds.size.z / maxTreeSizeInWorldSpace);

			m_VTScenes = new VTScene[SubSceneCount];

			for (int x = 0; x < SubSceneXCount; x++)
			{
				for (int z = 0; z < SubSceneZCount; z++)
				{
					for (int y = 0; y < SubSceneYCount; y++)
					{
						Vector3 origin = sceneBoundsMin + Vector3.right * x * maxTreeSizeInWorldSpace + Vector3.forward * z * maxTreeSizeInWorldSpace + Vector3.up * y * maxTreeSizeInWorldSpace;
						Vector3 subSceneSize = Vector3.one * maxTreeSizeInWorldSpace;
						Vector3 center = origin + subSceneSize * 0.5f;
						Bounds subSceneBounds = new Bounds(center, subSceneSize);
						int index = GetIndex(x, y, z);
						m_VTScenes[index] = new VTScene(subSceneBounds, m_MinVoxelSizeWS);

						sceneBoundsMax = Vector3.Max(subSceneBounds.max, sceneBoundsMax);
					}
				}
			}

			m_SceneBounds = GeometryUtil.MakeBoundsFromMinMax(sceneBoundsMin, sceneBoundsMax);

			BakeWork work = new BakeWork(this, triangles, terrainBounds);
			return work;
		}

		public List<Bounds> GetAllBlockBounds()
		{
			List<Bounds> blockBounds = new List<Bounds>();
			foreach (VTScene scene in m_VTScenes)
			{
				List<Bounds> boundsInScene = scene.GetBlockBounds();
				blockBounds.AddRange(boundsInScene);
			}
			return blockBounds;
		}

		public void WriteToFile(string outputFolder, string sceneName)
		{
			string groupFilePath = Path.Combine(outputFolder, sceneName + ".vtgroup");
			using (var fs = File.Create(groupFilePath))
			{
				using (var bw = new BinaryWriter(fs))
				{
					IOUtil.WriteInt(bw, m_SubSceneXCount);
					IOUtil.WriteInt(bw, m_SubSceneYCount);
					IOUtil.WriteInt(bw, m_SubSceneZCount);
					IOUtil.WriteFloat(bw, m_MinVoxelSizeWS);
					IOUtil.WriteBounds(bw, m_SceneBounds);
				}
			}
			
			WriteVTSceneToDirectory(outputFolder, sceneName);
		}
		
		public void BlockAllNodesInLayer(int layer)
		{
			ForeachScene((int x, int y, int z, int index, VTScene scene) =>
			{
				scene.BlockAllNodesInLayer(layer);
			});
		}

		public void WriteVTSceneToDirectory(string outputFolder, string sceneName)
		{
			ForeachScene((int x, int y, int z, int index, VTScene scene) =>
			{
				string subSceneFilePath = Path.Combine(outputFolder,  sceneName + $"_{x}_{y}_{z}.vtscene");
				scene.WriteToFile(subSceneFilePath);
			});
		}
#endif

	}
}
