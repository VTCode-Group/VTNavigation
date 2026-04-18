
using System;
using HashCode = VTNavigation.Tree.HashCode;

namespace VTNavigation.Navigation
{
	public struct MapHashCode:IEquatable<MapHashCode>
	{
		public IMap map;
		public HashCode hashCode;

		public bool Equals(MapHashCode other)
		{
			return map == other.map && hashCode.Equals(other.hashCode);
		}
	}
	
	public class AStarNode : IComparable<AStarNode>
	{
		public float weight;
		public AStarNode prevNode;
		public MapHashCode element;

		public AStarNode(float weight, AStarNode prevNode, IMap curMap, HashCode currentHashCode)
		{
			this.weight = weight;
			this.prevNode = prevNode;
			element = new MapHashCode(){map = curMap, hashCode = currentHashCode};
		}

		public int CompareTo(AStarNode other)
		{
			if(weight > other.weight)
			{
				return 1;
			}
			return -1;
		}
	}
}
