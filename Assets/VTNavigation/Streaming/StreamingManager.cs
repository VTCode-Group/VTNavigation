using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using VTNavigation.Drawer;
using VTNavigation.Geometry;
using VTNavigation.Scene;

namespace VTNavigation.Streaming
{
	public class DrawVoxelHandler
	{
		private ComputeBuffer m_ArgsBuffer;
		private ComputeBuffer m_InstanceMatriesBuffer;
		private Material m_Material;
		private Mesh m_Mesh;

		public DrawVoxelHandler(Mesh mesh, InstanceMatries[] instanceMatries, Material material)
		{
			m_Mesh = mesh;
			m_Material = material;
			m_ArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			uint[] args = new uint[5]
			{
				(uint)m_Mesh.GetIndexCount(0),
				(uint)instanceMatries.Length,
				(uint)m_Mesh.GetIndexStart(0),
				(uint)m_Mesh.GetBaseVertex(0),
				0
			};
			m_ArgsBuffer.SetData(args);

			m_InstanceMatriesBuffer = new ComputeBuffer(instanceMatries.Length, InstanceMatries.Size());
			m_InstanceMatriesBuffer.SetData(instanceMatries);
			m_Material.SetBuffer("_Matries", m_InstanceMatriesBuffer);


		}
		public void Draw(Bounds sceneBounds)
		{
			Graphics.DrawMeshInstancedIndirect(m_Mesh, 0, m_Material, sceneBounds, m_ArgsBuffer);
		}

		public void Dispose()
		{
			m_ArgsBuffer.Dispose();
			m_InstanceMatriesBuffer.Dispose();
		}
	}
	public class StreamingManager : MonoBehaviour
	{
		#region Input Params
		public Transform m_Agent;
		public float m_LoadDistance;
		public float m_UnLoadDistance;
		public string m_SceneDataDirectory;
		public bool m_DrawScene;
		#endregion

		private string m_SceneName;

		public string SceneDataDirectory
		{
			get
			{
				return m_SceneDataDirectory;
			}
		}

		public string SceneName
		{
			get
			{
				return m_SceneName;
			}
		}

		private Bounds m_LoadBounds;
		private Bounds m_UnLoadBounds;

		private string m_GroupFileName;
		private VTSceneGroup m_SceneGroup;

		private SceneLoadingStatus[,,] m_SceneLoadedStatus;

		private ConcurrentQueue<LoadingTaskData> m_LoadingTaskData = new ConcurrentQueue<LoadingTaskData>();

		public VTSceneGroup SceneGroup
		{
			get
			{
				return m_SceneGroup;
			}
		}

		public ConcurrentQueue<LoadingTaskData> LoadingTaskData
		{
			get
			{
				return m_LoadingTaskData;
			}
		}

		private ConcurrentQueue<LoadingTaskResult> m_LoadedBuffer = new ConcurrentQueue<LoadingTaskResult>();
		
		public ConcurrentQueue<LoadingTaskResult> LoadedBuffer
		{
			get
			{
				return m_LoadedBuffer;
			}
		}

		private LoadingTask m_LoadingTask;

		private Coroutine m_LoadedResultProcessor;

		#region Draw Data
		public Material m_Material;

		public Mesh m_Mesh;

		private DrawVoxelHandler[,,] m_DrawVoxelHandlers;


		private RefreshDrawDataTask m_RefreshDrawDataTask;

		private Coroutine m_RefreshDrawDataResultProcessor;

		private ConcurrentQueue<RefreshDrawDataTaskSignal> m_RefreshDrawDataSignal = new ConcurrentQueue<RefreshDrawDataTaskSignal>();

		private ConcurrentQueue<RefreshDrawDataTaskResult> m_RefreshDrawDataResult = new ConcurrentQueue<RefreshDrawDataTaskResult>();

		public ConcurrentQueue<RefreshDrawDataTaskSignal> RefreshDrawDataTaskSignal
		{
			get
			{
				return m_RefreshDrawDataSignal;
			}
		}
		public ConcurrentQueue<RefreshDrawDataTaskResult> RefreshDrawDataTaskResult
		{
			get
			{
				return m_RefreshDrawDataResult;
			}
		}

		#endregion

		// Start is called before the first frame update
		void Start()
		{
			if (m_Agent != null)
			{
				m_SceneName = SceneManager.GetActiveScene().name;
				m_GroupFileName = m_SceneName + ".vtgroup";
				m_LoadBounds = new Bounds(Vector3.zero, Vector3.one * m_LoadDistance);
				m_UnLoadBounds = new Bounds(Vector3.zero, Vector3.one * m_UnLoadDistance);

				using (var fs = File.OpenRead(m_SceneDataDirectory + "/" + m_GroupFileName))
				{
					using (var br = new BinaryReader(fs))
					{
						m_SceneGroup = new VTSceneGroup();
						m_SceneGroup.ReadGroupDataFromMemory(br);
						m_SceneLoadedStatus = new SceneLoadingStatus[m_SceneGroup.SubSceneXCount, m_SceneGroup.SubSceneYCount, m_SceneGroup.SubSceneZCount];
						if (m_DrawScene)
						{
							m_DrawVoxelHandlers = new DrawVoxelHandler[m_SceneGroup.SubSceneXCount, m_SceneGroup.SubSceneYCount, m_SceneGroup.SubSceneZCount];
						}
					}
				}

				Init();
			}
		}

		public void Init()
		{
			for (int x = 0; x < m_SceneGroup.SubSceneXCount; x++)
			{
				for (int z = 0; z < m_SceneGroup.SubSceneZCount; z++)
				{
					for (int y = 0; y < m_SceneGroup.SubSceneYCount; y++)
					{
						m_SceneLoadedStatus[x, y, z] = SceneLoadingStatus.UNLOAD;
						if (m_DrawScene)
						{
							m_DrawVoxelHandlers[x, y, z] = null;
						}
					}
				}
			}

			if (m_DrawScene)
			{
				m_RefreshDrawDataTask = new RefreshDrawDataTask(this);
				m_RefreshDrawDataTask.Start();
				m_RefreshDrawDataResultProcessor = StartCoroutine(RefreshDrawDataResultProcessor());
			}

			m_LoadedResultProcessor = StartCoroutine(LoadedResultProcesor());

			m_LoadingTask = new LoadingTask(this);
			m_LoadingTask.Start();
			(int curX, int curY, int curZ) = m_SceneGroup.GetLocatedSceneIndices(m_Agent.position);
			LoadingTaskData data = new LoadingTaskData()
			{
				x = curX,
				y = curY,
				z = curZ,
			};
			m_SceneLoadedStatus[curX, curY, curZ] = SceneLoadingStatus.LOADING;
			m_LoadingTaskData.Enqueue(data);
		}

		public IEnumerator LoadedResultProcesor()
		{
			while (true)
			{
				yield return new WaitUntil(() => LoadedBuffer.Count > 0);
				while(m_LoadedBuffer.TryDequeue(out LoadingTaskResult result))
				{
					int x = result.x;
					int y = result.y;
					int z = result.z;
					VTScene scene = result.scene;
					m_SceneGroup.SetLoadedScene(x, y, z, scene);
					if (m_DrawScene && m_RefreshDrawDataTask != null)
					{
						m_RefreshDrawDataSignal.Enqueue(new RefreshDrawDataTaskSignal()
						{
							x = x,
							y = y,
							z = z,
							scene = scene
						});
						m_SceneLoadedStatus[x, y, z] = SceneLoadingStatus.LOCK;
					}
					else
					{
						m_SceneLoadedStatus[x, y, z] = SceneLoadingStatus.LOADED;
					}
					
				}

				
			}
		}

		public IEnumerator RefreshDrawDataResultProcessor()
		{
			while (true)
			{
				yield return new WaitUntil(() => RefreshDrawDataTaskResult.Count > 0);

				while(RefreshDrawDataTaskResult.TryDequeue(out RefreshDrawDataTaskResult result))
				{
					RefreshDrawData(ref result);
					m_SceneLoadedStatus[result.x,result.y,result.z] = SceneLoadingStatus.LOADED;
					Debug.Log($"Refresh : {result.x}, {result.y}, {result.z}");
				}
			}
		}

		public void Update()
		{
			if(m_Agent != null)
			{
				Vector3 position = m_Agent.position;
				m_LoadBounds.center = position;
				m_UnLoadBounds.center = position;
				(int curX, int curY, int curZ) = m_SceneGroup.GetLocatedSceneIndices(position);
				for(int ox = -1;ox <= 1;ox++)
				{
					for(int oy = -1;oy <= 1;oy++)
					{
						for(int oz = -1;oz <= 1; oz++)
						{
							int x = curX + ox;
							int y = curY + oy;
							int z = curZ + oz;
							if(x < 0 || x >= m_SceneGroup.SubSceneXCount || y < 0 || y >= m_SceneGroup.SubSceneYCount || z <0 || z >= m_SceneGroup.SubSceneZCount)
							{
								continue;
							}
							if (m_SceneLoadedStatus[x,y,z] == SceneLoadingStatus.LOADED)
							{
								Bounds subSceneBounds = m_SceneGroup[x, y, z].SceneBounds;
								if (!GeometryUtil.BoundsIntersect(subSceneBounds, m_UnLoadBounds))
								{
									Debug.Log($"Unload : {x}, {y}, {z}");
									m_SceneGroup.ClearScene(x, y, z);
									m_SceneLoadedStatus[x, y, z] = SceneLoadingStatus.UNLOAD;
									if (m_DrawScene && m_DrawVoxelHandlers[x, y, z] != null)
									{
										m_DrawVoxelHandlers[x, y, z].Dispose();
										m_DrawVoxelHandlers[x, y, z] = null;
									}
								}
							}else if (m_SceneLoadedStatus[x,y,z] == SceneLoadingStatus.UNLOAD)
							{
								Bounds subSceneBounds = m_SceneGroup.GetSubSceneBounds(x, y, z);
								if(GeometryUtil.BoundsIntersect(m_LoadBounds, subSceneBounds))
								{
									m_LoadingTaskData.Enqueue(new LoadingTaskData()
									{
										x = x,
										y = y,
										z = z
									});
									m_SceneLoadedStatus[x, y, z] = SceneLoadingStatus.LOADING;
								}
							}
						}
					}
				}

				if (m_DrawScene && m_RefreshDrawDataTask != null)
				{
					if (m_SceneGroup != null)
					{
						for (int x = 0; x < m_SceneGroup.SubSceneXCount; x++)
						{
							for (int z = 0; z < m_SceneGroup.SubSceneZCount; z++)
							{
								for (int y = 0; y < m_SceneGroup.SubSceneYCount; y++)
								{
									Bounds sceneBounds = m_SceneGroup.GetSubSceneBounds(x, y, z);
									DrawUtil.DrawBounds(sceneBounds, Color.green);
								}
							}
						}
						
						DrawUtil.DrawBounds(m_UnLoadBounds, Color.red);
						DrawUtil.DrawBounds(m_LoadBounds, Color.blue);
						
						for (int x = 0; x < m_SceneGroup.SubSceneXCount; x++)
						{
							for (int y = 0; y < m_SceneGroup.SubSceneYCount; y++)
							{
								for (int z = 0; z < m_SceneGroup.SubSceneZCount; z++)
								{
									if (m_DrawVoxelHandlers[x, y, z] != null)
									{
										m_DrawVoxelHandlers[x, y, z].Draw(m_SceneGroup.SceneBounds);
									}
								}
							}
						}
					}
				}
			}
		}

		public void RefreshDrawData(ref RefreshDrawDataTaskResult result)
		{
			if(m_DrawVoxelHandlers[result.x, result.y, result.z] != null)
			{
				m_DrawVoxelHandlers[result.x, result.y, result.z].Dispose();
			}
			m_DrawVoxelHandlers[result.x, result.y, result.z] = new DrawVoxelHandler(m_Mesh, result.instanceMatries, new Material(m_Material));
		}

		private void OnDestroy()
		{
			m_LoadingTask.Close();
			StopCoroutine(m_LoadedResultProcessor);

			if (m_DrawScene)
			{
				if(m_RefreshDrawDataTask != null)
				{
					m_RefreshDrawDataTask.Close();
				}
				if (m_RefreshDrawDataResultProcessor != null)
				{
					StopCoroutine(m_RefreshDrawDataResultProcessor);
				}
				for(int x = 0; x< m_SceneGroup.SubSceneXCount; x++)
				{
					for(int y = 0;y< m_SceneGroup.SubSceneYCount; y++)
					{
						for(int z = 0;z< m_SceneGroup.SubSceneZCount; z++)
						{
							if (m_DrawVoxelHandlers[x, y, z] != null)
							{
								m_DrawVoxelHandlers[x,y,z].Dispose();
								m_DrawVoxelHandlers[x, y, z] = null;
							}
						}
					}
				}
			}
		}
	}
}
