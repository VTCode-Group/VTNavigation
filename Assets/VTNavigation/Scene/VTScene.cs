using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTNavigation.Editor;
using VTNavigation.Geometry;
using VTNavigation.Tree;
using VTNavigation.Util;

namespace VTNavigation.Scene
{
	public class VTScene
	{
		private OCTree m_Tree;
		private float m_MinSizeScale;
		private Vector3 m_SceneOrigin;
		private Vector3 m_SceneSize;
		private Bounds m_SceneBounds;

		public float MinSizeScale
		{
			get
			{
				return m_MinSizeScale;
			}
		}

		public OCTree Tree
		{
			get
			{
				return m_Tree;
			}
		}

		public Vector3 SceneOrigin
		{
			get
			{
				return m_SceneOrigin;
			}
		}

		public Vector3 SceneSize
		{
			get { return m_SceneSize; }
		}

		public Bounds SceneBounds
		{
			get
			{
				return m_SceneBounds;
			}
		}


		public VTScene()
		{
		}

		public VTScene(Bounds sceneBoundsWS, float minVoxelSize = 1.0f)
		{
			m_SceneOrigin = sceneBoundsWS.min;
			m_SceneBounds = sceneBoundsWS;
			m_SceneSize = sceneBoundsWS.size;
			m_MinSizeScale = minVoxelSize;
			m_Tree = new OCTree(minVoxelSize, true);
		}

		public void WriteToFile(string path)
		{
			if (!path.EndsWith(".vtscene"))
			{
				path += ".vtscene";
			}
			using (var fs = File.Create(path))
			{
				using (var bw = new BinaryWriter(fs))
				{
					IOUtil.WriteFloat(bw, m_MinSizeScale);
					IOUtil.WriteVector3(bw, m_SceneOrigin);
					IOUtil.WriteVector3(bw, m_SceneSize);
					m_Tree.WriteWithBinaryWriter(bw);
				}
			}
		}
		
		public void ReadFromMemory(BinaryReader br)
		{
			m_MinSizeScale = IOUtil.ReadFloat(br);
			m_SceneOrigin = IOUtil.ReadVector3(br);
			m_SceneSize = IOUtil.ReadVector3(br);
			if (m_Tree == null)
			{
				m_Tree = new OCTree(m_MinSizeScale, false);
			}
			m_Tree.ReadFromBinaryReader(br);
			m_SceneBounds = new Bounds(m_SceneOrigin + m_SceneSize * 0.5f, m_SceneSize);
		}

		public void ReadFromFile(string path)
		{
			using (var fs = File.OpenRead(path))
			{
				using (var br = new BinaryReader(fs))
				{
					m_MinSizeScale = IOUtil.ReadFloat(br);
					m_SceneOrigin = IOUtil.ReadVector3(br);
					m_SceneSize = IOUtil.ReadVector3(br);
					if (m_Tree == null)
					{
						m_Tree = new OCTree(m_MinSizeScale, false);
					}
					m_Tree.ReadFromBinaryReader(br);
					m_SceneBounds = new Bounds(m_SceneOrigin + m_SceneSize * 0.5f, m_SceneSize);
				}
			}
		}

		public List<Bounds> GetBlockBounds()
		{
			List<Bounds> boundsListInTreeSpace = m_Tree.FindBlockBounds();
			VTSceneUtil.ToWorldSpace(this, boundsListInTreeSpace);
			return boundsListInTreeSpace;
		}

		public void Reset(float minVoxelSize = 1.0f)
		{
			m_Tree.Clear();
			m_MinSizeScale = minVoxelSize;
			m_Tree = new OCTree(MinSizeScale, true);
		}
	}
}
