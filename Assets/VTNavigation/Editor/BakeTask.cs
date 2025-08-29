using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTNavigation.Geometry;
using VTNavigation.Scene;
using VTNavigation.Tree;

namespace VTNavigation.Editor
{
	public class BakeTask
	{
		private BakeWork m_Owner;
		private VTScene m_Scene;
		private OCTree m_Tree;
		
		private Thread m_Thread;

		private ReaderWriterLock m_Mutex;
		private int m_ProcessedCount;
		
		public int ProcessedCount {
			get
			{
				m_Mutex.AcquireReaderLock(3000);
				int value = m_ProcessedCount;
				m_Mutex.ReleaseReaderLock();
				return value;
			}
			private set
			{
				m_Mutex.AcquireWriterLock(3000);
				m_ProcessedCount = value;
				m_Mutex.ReleaseWriterLock();
			}
		}

		private List<Triangle> m_Triangles;

		private List<Bounds> m_Bounds;

		public int PremitiveCount
		{
			get
			{
				return m_Triangles.Count + m_Bounds.Count;
			}
		}
		
		public OCTree Tree { get { return m_Tree; } }
		
		public BakeTask(VTScene scene, BakeWork owner)
		{
			m_Owner = owner;
			m_Scene = scene;
			m_Tree = scene.Tree;
			m_Thread = new Thread(Run);
			m_Mutex = new ReaderWriterLock();
			m_Triangles = new List<Triangle>();
			m_Bounds = new List<Bounds>();
		}

		public void Start()
		{
			m_Thread.Start();
		}
		
		public void Run()
		{
			VTSceneUtil.ToTreeSpace(m_Scene, m_Triangles);
			VTSceneUtil.ToTreeSpace(m_Scene, m_Bounds);

			int processCountInterval = 20;
			for(int i = 0;i<m_Triangles.Count;i++)
			{
				m_Tree.SplitTreeByTriangle(m_Triangles[i]);

				processCountInterval--;
				if(processCountInterval <= 0)
				{
					ProcessedCount = ProcessedCount + 20;
					processCountInterval = 20;
				}
			}
			
			for(int i = 0;i<m_Bounds.Count;i++)
			{
				m_Tree.SplitTreeByBounds(m_Bounds[i]);

				processCountInterval--;
				if (processCountInterval <= 0)
				{
					ProcessedCount = ProcessedCount + 20;
					processCountInterval = 20;
				}
			}

			ProcessedCount = ProcessedCount + 20 - processCountInterval;
		}

		public void Add(Bounds bounds)
		{
			m_Bounds.Add(bounds);
		}

		public void Add(Triangle triangle)
		{
			m_Triangles.Add(triangle);
		}
	}

	
}
