using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Assertions;
using VTNavigation.Drawer;
using VTNavigation.Geometry;
using HashCode = VTNavigation.Tree.HashCode;

namespace VTNavigation.Tree
{
	public class OCTree
	{
		private float m_MinSizeScale;

		private Dictionary<HashCode, Byte> m_TreeNodes;

		private Bounds m_RootBounds;

		private HashCode m_RootCode;

		public Bounds RootBounds
		{
			get
			{
				return m_RootBounds;
			}
		}

		public Bounds RootBoundsInWorldSpace
		{
			get
			{
				return OCTreeUtil.ToWorldSpace(this, m_RootBounds);
			}
		}

		public float MinSizeScale
		{
			get
			{
				return m_MinSizeScale;
			}
		}

		public OCTree(float minSizeScale = 1.0f, bool addRoot = true)
		{
			m_MinSizeScale = minSizeScale;
			m_TreeNodes = new Dictionary<HashCode, Byte>();

			m_RootCode = HashCode.RootNode();
			m_RootBounds = m_RootCode.DecodeBounds();

			if (addRoot)
			{
				m_TreeNodes.Add(m_RootCode, 0);
			}
		}

		public void SplitTreeByTriangle(Triangle trianglePointsInTreeSpace)
		{
			HashCode root = m_RootCode;
			Bounds rootBounds = root.DecodeBounds();
			if(GeometryUtil.OverlapTriangle(rootBounds, trianglePointsInTreeSpace))
			{
				for(int i = 0; i< 8; i++)
				{
					HashCode childCode = root.ToChild(i);
					Bounds childBounds = childCode.DecodeBounds();
					byte status = m_TreeNodes[root];
					if (!OCTreeUtil.IsBlockStatus(status, i) && GeometryUtil.OverlapTriangle(childBounds, trianglePointsInTreeSpace))
					{
						SplitTreeByTriangleInternal(trianglePointsInTreeSpace, root, childCode);
					}
				}
				
			}
		}

		private void SplitTreeByTriangleInternal(Triangle trianglePointsInTreeSpace, HashCode parentCode, HashCode currentCode)
		{
			if(currentCode.Layer == 0)
			{
				int offset = currentCode.DecodeMergedOffset();
				byte status = m_TreeNodes[parentCode];
				OCTreeUtil.SetBlockStatus(ref status, offset);
				m_TreeNodes[parentCode] = status;
				if(status == byte.MaxValue)
				{
					BlockHashCode(parentCode);
				}
				return;
			}
			if (!m_TreeNodes.ContainsKey(currentCode))
			{
				m_TreeNodes.Add(currentCode, 0);
			}
			byte currentStatus = m_TreeNodes[currentCode];
			for(int i = 0; i < 8; i++)
			{
				HashCode childCode = currentCode.ToChild(i);
				Bounds childBounds = childCode.DecodeBounds();
				if(!OCTreeUtil.IsBlockStatus(currentStatus,i) && GeometryUtil.OverlapTriangle(childBounds, trianglePointsInTreeSpace))
				{
					SplitTreeByTriangleInternal(trianglePointsInTreeSpace, currentCode, childCode);
				}
			}
			
		}

		public void SplitTreeByBounds(Bounds boundsInTreeSpace)
		{
			HashCode root = m_RootCode;
			Bounds rootBounds = root.DecodeBounds();
			if(GeometryUtil.BoundsIntersect(rootBounds, boundsInTreeSpace))
			{
				for(int i = 0;i < 8;i++)
				{
					HashCode childCode = root.ToChild(i);
					Bounds childBounds = childCode.DecodeBounds();
					byte status = m_TreeNodes[root];
					if(!OCTreeUtil.IsBlockStatus(status,i) && GeometryUtil.BoundsIntersect(childBounds, boundsInTreeSpace))
					{
						SplitTreeByBoundsInternal(boundsInTreeSpace, root, childCode);
					}
				}
			}
		}

		private void SplitTreeByBoundsInternal(Bounds boundsInTreeSpace, HashCode parentCode, HashCode currentCode)
		{
			if (currentCode.Layer == 0)
			{
				int offset = currentCode.DecodeMergedOffset();
				byte status = m_TreeNodes[parentCode];
				OCTreeUtil.SetBlockStatus(ref status, offset);
				m_TreeNodes[parentCode] = status;
				return;
			}
			if (!m_TreeNodes.ContainsKey(currentCode))
			{
				m_TreeNodes.Add(currentCode, 0);
			}
			byte currentStatus = m_TreeNodes[currentCode];
			for (int i = 0; i < 8; i++)
			{
				HashCode childCode = currentCode.ToChild(i);
				Bounds childBounds = childCode.DecodeBounds();
				if (!OCTreeUtil.IsBlockStatus(currentStatus, i) && GeometryUtil.BoundsIntersect(childBounds, boundsInTreeSpace))
				{
					SplitTreeByBoundsInternal(boundsInTreeSpace, currentCode, childCode);
				}
			}
		}

		public bool IsBlock(HashCode code)
		{
			if (code.Equals(m_RootCode))
			{
				return m_TreeNodes[code] == byte.MaxValue;
			}
			HashCode parentCode = code.ToParent();
			HashCode childCode = code;
			while (!m_TreeNodes.ContainsKey(parentCode))
			{
				childCode = parentCode;
				parentCode = parentCode.ToParent();
			}
			byte status = m_TreeNodes[parentCode];
			int offset = childCode.DecodeMergedOffset();
			return OCTreeUtil.IsBlockStatus(status, offset);
		}

		public bool IsSparse(HashCode code)
		{
			if (m_TreeNodes.ContainsKey(code))
			{
				return true;
			}
			return false;
		}

		public bool IsFreeSpace(HashCode code)
		{
			return !IsSparse(code) && !IsBlock(code);
		}

		private void BlockHashCode(HashCode code)
		{
			HashCode parentCode = code.ToParent();
			Stack<HashCode> visited = new Stack<HashCode>();

			if (m_TreeNodes.ContainsKey(parentCode))
			{
				byte status = m_TreeNodes[parentCode];
				int offset = code.DecodeMergedOffset();
				status |= (byte)(1 << offset);
				m_TreeNodes[parentCode] = status;
				if (status == byte.MaxValue)
				{
					BlockHashCode(parentCode);
				}
			}
			else
			{
				Stack<HashCode> visitedCodes = new Stack<HashCode>();
				do
				{
					visitedCodes.Push(parentCode);
					parentCode = parentCode.ToParent();
				} while (!m_TreeNodes.ContainsKey(parentCode));

				while (visitedCodes.Count > 0)
				{
					HashCode childCode = visitedCodes.Pop();
					m_TreeNodes.Add(childCode, 0);
				}

				parentCode = code.ToParent();
				byte status = m_TreeNodes[parentCode];
				int offset = code.DecodeMergedOffset();
				status |= (byte)(1 << offset);
				m_TreeNodes[parentCode] = status;
			}

			if (m_TreeNodes.ContainsKey(code))
			{
				m_TreeNodes.Remove(code);
			}
		}

		public void WriteToFile(string path)
		{
			using (var fs = File.Create(path))
			{
				using (var bw = new BinaryWriter(fs))
				{
					bw.Write(m_MinSizeScale);
					bw.Write(m_TreeNodes.Count);
					WriteFileInternal(m_RootCode, bw);
				}
			}
		}
		private HashSet<HashCode> visited = new HashSet<HashCode>();
		public void WriteWithBinaryWriter(BinaryWriter bw)
		{
			bw.Write(m_MinSizeScale);
			bw.Write(m_TreeNodes.Count);
			WriteFileInternal(m_RootCode, bw);
		}

		private void SetBlock(ref byte status, int offset)
		{
			status |= (byte)(1 << offset);
		}

		private void WriteFileInternal(HashCode currentCode, BinaryWriter bw)
		{
			visited.Add(currentCode);
			byte status = m_TreeNodes[currentCode];
			bw.Write(currentCode.Code);
			bw.Write(status);
			for (int i = 0; i < 8; i++)
			{
				HashCode childCode = currentCode.ToChild(i);
				if (!OCTreeUtil.IsBlockStatus(status, i) && m_TreeNodes.ContainsKey(childCode))
				{
					WriteFileInternal(childCode, bw);
				}
			}
		}

		public void ReadFromFile(string path)
		{
			m_TreeNodes.Clear();
			using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
			{
				m_MinSizeScale = br.ReadSingle();
				int count = br.ReadInt32();
				for (int i = 0; i < count; i++)
				{
					UInt32 code = br.ReadUInt32();
					byte status = br.ReadByte();
					m_TreeNodes.Add(new HashCode(code), status);
				}
			}
		}

		public void ReadFromBinaryReader(BinaryReader br)
		{
			m_TreeNodes.Clear();

			m_MinSizeScale = br.ReadSingle();
			int count = br.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				UInt32 code = br.ReadUInt32();
				byte status = br.ReadByte();
				m_TreeNodes.Add(new HashCode(code), status);
			}
		}

		public List<Bounds> FindBlockBounds()
		{
			List<Bounds> res = new List<Bounds>();

			if (IsBlock(m_RootCode))
			{
				res.Add(m_RootCode.DecodeBounds());
			}
			else
			{
				List<HashCode> blockHashCodes = new List<HashCode>();
				FindBlockHashCodeInternal(m_RootCode, blockHashCodes);
				foreach (HashCode code in blockHashCodes)
				{
					res.Add(code.DecodeBounds());
				}
			}
			return res;
		}

		private void FindBlockHashCodeInternal(HashCode currentHashCode, List<HashCode> blockHashCodes)
		{
			byte status = m_TreeNodes[currentHashCode];
			for (int i = 0; i < 8; i++)
			{
				HashCode childCode = currentHashCode.ToChild(i);
				if (OCTreeUtil.IsBlockStatus(status, i))
				{
					blockHashCodes.Add(childCode);
				}
				else if (m_TreeNodes.ContainsKey(childCode))
				{
					FindBlockHashCodeInternal(childCode, blockHashCodes);
				}
			}
		}

		public void MergeTree(OCTree treeFrom, bool clearEnd = false)
		{
			if (!CanMerge(treeFrom))
			{
				return;
			}
			MergeTreeInternal(treeFrom, treeFrom.m_RootCode);

			if (clearEnd)
			{
				treeFrom.Clear(false);
			}
		}

		public bool CanMerge(OCTree tree)
		{
			return m_MinSizeScale == tree.MinSizeScale;
		}

		public bool IsEmpty()
		{
			return m_TreeNodes.Count == 0 || m_TreeNodes.Count == 1 && m_TreeNodes[m_RootCode] == 0;
		}

		public void Clear(bool keepRoot = true)
		{
			m_TreeNodes.Clear();
			if (keepRoot)
			{
				m_TreeNodes.Add(HashCode.RootNode(), 0);
			}
		}

		private void RemoveSubTree(HashCode code)
		{
			if (m_TreeNodes.ContainsKey(code))
			{
				byte status = m_TreeNodes[code];
				for (int offset = 0; offset < 8; offset++)
				{
					if (!OCTreeUtil.IsBlockStatus(status, offset))
					{
						HashCode childCode = code.ToChild(offset);
						if (IsSparse(childCode))
						{
							RemoveSubTree(childCode);
						}
					}
				}
				m_TreeNodes.Remove(code);
			}
		}

		public List<KeyValuePair<HashCode,byte>> GetSubTreeNodes(HashCode code)
		{
			List<KeyValuePair<HashCode, byte>> res = new List<KeyValuePair<HashCode, byte>>();
			if (m_TreeNodes.ContainsKey(code))
			{
				GetSubTreeNodesInterval(code, res);
			}
			return res;
		}

		private void GetSubTreeNodesInterval(HashCode currentCode, List<KeyValuePair<HashCode, byte>> result)
		{
			result.Add(new KeyValuePair<HashCode, byte>(currentCode, m_TreeNodes[currentCode]));
			for(int i = 0; i < 8; i++)
			{
				HashCode childCode = currentCode.ToChild(i);
				if(IsSparse(childCode))
				{
					GetSubTreeNodesInterval(childCode, result);
				}
			}
		}

		private void MergeTreeInternal(OCTree treeFrom, HashCode keyFrom)
		{
			var dictFrom = treeFrom.m_TreeNodes;
			var dictTo = m_TreeNodes;
			
			if (!dictTo.ContainsKey(keyFrom) && !IsBlock(keyFrom))
			{
				HashCode parent = keyFrom.ToParent();
				HashCode child = keyFrom;
				while (!dictTo.ContainsKey(parent))
				{
					child = parent;
					parent = parent.ToParent();
				}
				List<KeyValuePair<HashCode, byte>> subTree = treeFrom.GetSubTreeNodes(child);
				foreach(var node in subTree)
				{
					dictTo.Add(node.Key,node.Value);
				}
			}
			else
			{
				byte statusTo = dictTo[keyFrom];
				byte statusFrom = dictFrom[keyFrom];
				if (statusTo != statusFrom)
				{
					for (int offset = 0; offset < 8; offset++)
					{
						HashCode childCode = keyFrom.ToChild(offset);
						if (OCTreeUtil.IsBlockStatus(statusFrom, offset))
						{
							if (!OCTreeUtil.IsBlockStatus(statusTo, offset))
							{
								if (IsSparse(childCode))
								{
									RemoveSubTree(childCode);
								}
								SetBlock(ref statusTo, offset);
								dictTo[keyFrom] = statusTo;
							}
						}
						else if (treeFrom.IsSparse(childCode) && IsFreeSpace(childCode))
						{
							List<KeyValuePair<HashCode, byte>> subTree = treeFrom.GetSubTreeNodes(childCode);
							foreach (var node in subTree)
							{
								dictTo.Add(node.Key, node.Value);
							}
						}
					}
				}
			}
			//	Recursive
			{
				for (int offset = 0; offset < 8; offset++)
				{
					HashCode childCode = keyFrom.ToChild(offset);
					if (treeFrom.IsSparse(childCode) && !IsBlock(childCode))
					{
						MergeTreeInternal(treeFrom, childCode);
					}
				}
			}
		}

		public static float MaxTreeSize
		{
			get
			{
				return HashCode.MaxSize;
			}
		}
	}
}
