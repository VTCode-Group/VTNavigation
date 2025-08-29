using PlasticGui.WorkspaceWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTNavigation.Editor;
using VTNavigation.Geometry;
using VTNavigation.Tree;
using VTNavigation.Util;

namespace VTNavigation.Scene
{
	public class VTSceneGroup
	{
		private int m_SubSceneXCount;
		private int m_SubSceneYCount;
		private int m_SubSceneZCount;
		private float m_MinVoxelSizeWS; //	World Size : Tree Size
		private VTScene[] m_VTScenes;
		private Bounds m_SceneBounds;

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

		public VTScene this[int x, int y, int z]
		{
			get
			{
				int index = GetIndex(x, y, z);
				if (index < 0 || index >= SubSceneCount)
					return null;
				return m_VTScenes[index];
			}
			private set
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

		public void WriteToFile(string fileName)
		{
			using (var fs = File.Create(fileName))
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
		}

		public void WriteVTSceneToDirectory(string directory, string sceneName)
		{
			for (int x = 0; x < m_SubSceneXCount; x++)
			{
				for (int z = 0; z < m_SubSceneZCount; z++)
				{
					for (int y = 0; y < m_SubSceneYCount; y++)
					{
						VTScene scene = this[x, y, z];
						string path = Path.Combine(directory, sceneName + $"_{x}_{y}_{z}.vtscene");
						scene.WriteToFile(path);
					}
				}
			}
		}
#endif

	}
}
