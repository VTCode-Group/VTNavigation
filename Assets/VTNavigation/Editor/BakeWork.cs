using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTNavigation.Geometry;
using VTNavigation.Scene;
using VTNavigation.Tree;

namespace VTNavigation.Editor
{
	public class BakeWork
	{
		private ConcurrentQueue<List<Vector3[]>> m_TriangleGroups;

		private ConcurrentQueue<List<Bounds>> m_TerrainBoundsGroups;

		private BakeTask[] m_Tasks;

		private int m_SplitPremitiveCount;

		public List<OCTree> BakeResults
		{
			get
			{
				List<OCTree> results = new List<OCTree>();
				foreach (var task in m_Tasks)
				{
					results.Add(task.Tree);
				}

				return results;
			}
		}

		public bool IsDone
		{
			get { return ProcessedCount == SplitPremitiveTaskCount; }
		}

		public int SplitPremitiveTaskCount
		{
			get { return m_SplitPremitiveCount; }
			set { m_SplitPremitiveCount = value; }
		}

		public int ProcessedCount
		{
			get
			{
				int sum = 0;
				foreach (var task in m_Tasks)
				{
					sum += task.ProcessedCount;
				}

				return sum;
			}
		}

		public BakeWork(VTSceneGroup sceneGroup, List<Triangle> trianglesWS, List<Bounds> boundsWS)
		{
			m_Tasks = new BakeTask[sceneGroup.SubSceneCount];
			for(int x = 0;x<sceneGroup.SubSceneXCount;x++)
			{
				for(int z = 0; z < sceneGroup.SubSceneZCount; z++)
				{
					for(int y = 0; y < sceneGroup.SubSceneYCount; y++)
					{
						VTScene subScene = sceneGroup[x, y, z];
						int index = sceneGroup.GetIndex(x, y, z);
						m_Tasks[index] = new BakeTask(subScene, this);

						Bounds sceneBounds = subScene.SceneBounds;
						
						foreach(Triangle triangle in trianglesWS)
						{
							if(GeometryUtil.OverlapTriangle(sceneBounds, triangle))
							{
								m_Tasks[index].Add(triangle.Clone());
							}
						}

						foreach(Bounds bounds in boundsWS)
						{
							if(GeometryUtil.BoundsIntersect(sceneBounds, bounds))
							{
								m_Tasks[index].Add(bounds);
							}
						}
					}
				}
			}
			m_SplitPremitiveCount = 0;
			for(int i = 0;i<m_Tasks.Length;i++)
			{
				m_SplitPremitiveCount += m_Tasks[i].PremitiveCount;
			}
		}

		public void DoWork()
		{
			for (int i = 0; i <m_Tasks.Length; i++)
			{
				m_Tasks[i].Start();
			}
		}
	}
}
