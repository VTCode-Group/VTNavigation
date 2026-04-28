using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTNavigation.Geometry;
using VTNavigation.Navigation;
using VTNavigation.Tree;
using VTNavigation.Util;
using VTPartition.Scene;
using HashCode = VTNavigation.Tree.HashCode;

namespace VTNavigation.Scene
{
    public class VTScene:IMap
    {
        private OCTree m_Tree;
		private float m_MinSizeScale;
		private Vector3 m_SceneOrigin;
		private Vector3 m_SceneSize;
		private Bounds m_SceneBounds;
		private int m_XIndex;
		private int m_YIndex;
		private int m_ZIndex;

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

		public int XIndex
		{
			get { return m_XIndex; }
			set { m_XIndex = value;}
		}

		public int YIndex
		{
			get { return m_YIndex; }
			set { m_YIndex = value;}
		}

		public int ZIndex
		{
			get { return m_ZIndex; }
			set { m_ZIndex = value;}
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

		public void SplitTreeByBounds(Bounds boundsInWorldSpace)
		{
			Bounds boundsInTreeSpace = this.ToTreeSpace(boundsInWorldSpace);
			m_Tree.SplitTreeByBounds(boundsInTreeSpace);
		}

		public void SplitTreeByTriangle(Triangle triangleInWorldSpace)
		{
			Triangle triangleInTreeSpace = this.ToTreeSpace(triangleInWorldSpace);
			m_Tree.SplitTreeByTriangle(triangleInTreeSpace);
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

		public void ReadFromBytes(byte[] rawData)
		{
			using (var ms = new MemoryStream(rawData))
			{
				using (var br = new BinaryReader(ms))
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

		public (int, int, int) GetSubSceneIndex()
		{
			return (m_XIndex, m_YIndex, m_ZIndex);
		}

		public bool IsBlock(Tree.HashCode hashCode)
		{
			return m_Tree.IsBlock(hashCode);
		}

		public bool IsFreeSpace(Tree.HashCode hashCode)
		{
			return m_Tree.IsFreeSpace(hashCode);
		}

		public bool IsSparse(Tree.HashCode hashCode)
		{
			return m_Tree.IsSparse(hashCode);
		}

		public HashCode GetHashCode(Vector3 positionWS, int layer = 0)
		{
			Vector3 positionTS = this.PointToTreeSpace(positionWS);
			float layerSize = HashCode.LayerToSize(layer);
			int layerCoordinateCount = HashCode.LayerCoordinateCount(layer);
			int x = (int)(positionTS.x / layerSize);
			int y = (int)(positionTS.y / layerSize);
			int z = (int)(positionTS.z / layerSize);

			if (x < 0 || y < 0 || z < 0 || x >= layerCoordinateCount || y >= layerCoordinateCount ||
			    z >= layerCoordinateCount)
			{
				return HashCode.INVALID_CODE;
			}

			return HashCode.Encode((UInt32)x, (UInt32)y, (UInt32)z, layer);
		}

		public Vector2 ToMapSpace(Vector2 positionWS)
		{ 
			return VTSceneUtil.PointToTreeSpace(this, positionWS);
		}

		public Vector2 ToWorldSpace(Vector2 positionTS)
		{
			return VTSceneUtil.PointToWorldSpace(this, positionTS);
		}

		public Bounds ToMapSpace(Bounds boundsWS)
		{
			return this.ToMapSpace(boundsWS);
		}

		// public void GetWalkableAreas(HashCode hashCode, HashSet<HashCode> ignoreSet, out List<HashCode> result)
		// {
		// 	result = new List<HashCode>();
		// 	m_Tree.GetAdjFreeSpace(hashCode, result);
		// }

		public void GetEdgeFreeSpace(HashCode hashCode, List<HashCode> result,
			int filterXMask, int filterYMask, int filterZMask)
		{
			m_Tree.GetEdgeFreeSpace(hashCode, result, filterXMask, filterYMask, filterZMask);
		}

		public bool FastRayCastHit(Ray rayWS, out float minDistance,float edageError = 0.2f)
		{
			edageError = this.ToTreeSpace(edageError);
			var rayTS = this.ToTreeSpace(rayWS);
			m_Tree.FastRaycastHit(rayTS, out RayCastResult result, edageError);
			if (result.hit)
			{
				result.distance = this.ToWorldSpace(result.distance);
				result.blockerBounds = this.ToWorldSpace(result.blockerBounds);
				result.origin = this.PointToWorldSpace(result.origin);
			}
			minDistance = result.hit? result.distance:float.MaxValue;
			return result.hit;
		}
		
		public bool RayCastHit(Ray rayWS, out float minDistance, float edageError = 0.2f)
		{
			edageError = this.ToTreeSpace(edageError);
			var rayTS = this.ToTreeSpace(rayWS);
			m_Tree.RayCastHit(rayTS, out RayCastResult result, edageError);
			if (result.hit)
			{
				result.distance = this.ToWorldSpace(result.distance);
				result.blockerBounds = this.ToWorldSpace(result.blockerBounds);
				result.origin = this.PointToWorldSpace(result.origin);
			}
			minDistance = result.hit? result.distance:float.MaxValue;
			return result.hit;
		}

		public HashCode ToMaxWalkableArea(HashCode walkableHashCode)
		{
			return m_Tree.ToMaxFreeSpace(walkableHashCode);
		}

		public Vector3 ToMapSpace(Vector3 positionWS)
		{
			return this.PointToTreeSpace(positionWS);
		}
		
		public Vector3 ToWorldSpace(Vector3 positionTS)
		{
			return this.PointToWorldSpace(positionTS);
		}

		public Bounds ToWorldSpace(Bounds boundsTS)
		{
			return VTSceneUtil.ToWorldSpace(this, boundsTS);
		}

		public void BlockAllNodesInLayer(int layer)
		{
			m_Tree.BlockAllNodesInLayer(layer);
		}

		public static VTScene CreateSceneFromBytes(byte[] rawData)
		{
			VTScene scene = new VTScene();
			scene.ReadFromBytes(rawData);
			return scene;
		}
	}
}