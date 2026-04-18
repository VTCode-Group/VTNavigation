using System;
using DataStructures.PriorityQueue;
using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Debugger;
using VTNavigation.Navigation;
using HashCode = VTNavigation.Tree.HashCode;

namespace VTNavigation.Navigation
{
	public struct NavigationPathPoint
	{
		public IMap map;
		public Vector3 point;
	}
	
    public static class NavigationHelper
    {
	    private struct PathSegment
	    {
		    public IMap map;
		    public int  startIndex;
		    public int  endIndex;
	    }
	    
        public static List<Vector3> QueryPath(IMapGroup mapGroup, Vector3 startPositionWS, Vector3 targetPositionWS)
        {
	        // IMap startMap = mapGroup.GetMap(startPositionWS);
	        // IMap targetMap = mapGroup.GetMap(targetPositionWS);
         //    Vector3 startPositionTS = startMap.ToMapSpace(startPositionWS);
         //    Vector3 targetPositionTS = startMap.ToMapSpace(targetPositionWS);
            
            List<NavigationPathPoint> result = QueryPathInternal(mapGroup, startPositionWS, targetPositionWS);

            List<PathSegment> pathSegments = new List<PathSegment>();
            List<Vector3> pathPoints = new List<Vector3>();
            if (result != null && result.Count > 0)
            {
	            pathPoints.Add(result[0].point);
	            IMap currentMap = result[0].map;
	            int currentStartIndex = 0;
	            int currentEndIndex = 0;
	            for (int i = 1; i < result.Count; i++)
	            {
		            Vector3 point = result[i].point;
		            pathPoints.Add(point);

		            IMap map = result[i].map;
		            
		            if (map != currentMap)
		            {
			            pathSegments.Add( new PathSegment(){map = currentMap, startIndex = currentStartIndex, endIndex = currentEndIndex});
			            currentMap = map;
			            currentStartIndex = i;
		            }
		            currentEndIndex = i;
	            }
	            pathSegments.Add(new PathSegment(){ map = currentMap, startIndex = currentStartIndex, endIndex = currentEndIndex});
            }

            List<Vector3> smoothPathPoints = new List<Vector3>();
            for (int i = 0; i < pathSegments.Count; i++)
            {
	            IMap map = pathSegments[i].map;
	            int startIndex = pathSegments[i].startIndex;
	            int endIndex = pathSegments[i].endIndex;
	            for (int j = startIndex; j <= endIndex; j++)
	            {
		            pathPoints[j] = map.ToWorldSpace(pathPoints[j]);
	            }
					
	            if (endIndex - startIndex >= 2)
	            {
		            //	At least 3 points
		            smoothPathPoints.AddRange(SmoothPath(map, pathPoints, startIndex, endIndex));
	            }
	            else
	            {
		            for (int j = startIndex; j <= endIndex;j++)
		            {
			            smoothPathPoints.Add(pathPoints[j]);
		            }
	            }
            }
            
            return smoothPathPoints;
        }

        private static List<Vector3> SmoothPath(IMap map, List<Vector3> path, int startIndex, int endIndex)
        {
	        if (path.Count == 2)
	        {
		        return path;
	        }
	        
	        List<Vector3> res = new List<Vector3>();
	        res.Add(path[startIndex]);

	        int basePointIndex = startIndex;
	        while (basePointIndex < endIndex)
	        {
		        int nextPointIndex = basePointIndex + 1;
		        int targetPointIndex = endIndex + 1;
		        int mid = (nextPointIndex + targetPointIndex) >> 1;
		        
		        Vector3 basePoint = path[basePointIndex];
		        
		        while (mid < targetPointIndex && mid > nextPointIndex)
		        {
			        Vector3 midPoint = path[mid];
			        Ray ray = new Ray(basePoint, (midPoint - basePoint).normalized);
			        float distance = Vector3.Distance(midPoint, basePoint);
			        if (!map.RayCastHit(ray, out float minDistance, 0.0f) || minDistance > distance)
			        {
				        nextPointIndex = mid;
				        mid = (nextPointIndex + targetPointIndex) >> 1;
			        }
			        else
			        {
				        targetPointIndex = mid;
				        mid = (targetPointIndex + nextPointIndex) >> 1;
			        }
		        }
		        res.Add(path[nextPointIndex]);
		        basePointIndex = nextPointIndex;
	        }
	        return res;
        }

		private static float EvaluateWeight(Vector3 currentPosition, Vector3 startPosition, Vector3 targetPosition, float boxSize)
		{
			float from = Vector3.Distance(currentPosition, startPosition);
			float to = Vector3.Distance(currentPosition, targetPosition);
			return from + 2.0f*to - 4.0f*boxSize;
		}
		
        private static List<NavigationPathPoint> QueryPathInternal(IMapGroup mapGroup, Vector3 startPositionWS, Vector3 targetPositionWS)
        {
	        IMap startMap = mapGroup.GetMap(startPositionWS);
	        if (startMap == null)
	        {
		        return null;
	        }
	        Vector3 startPositionTS = startMap.ToMapSpace(startPositionWS);
			HashCode startHashCode = HashCode.PositionToHashCode(startPositionTS);

			if (startMap.IsBlock(startHashCode))
			{
				return null;
			}

			IMap targetMap = mapGroup.GetMap(targetPositionWS);
			if (targetMap == null)
			{
				return null;
			}
			
			Vector3 targetPositionTS = targetMap.ToMapSpace(targetPositionWS);
			HashCode targetHashCode = HashCode.PositionToHashCode(targetPositionTS);

			if (targetMap.IsBlock(targetHashCode))
			{
				return null;
			}

			startHashCode = startMap.ToMaxWalkableArea(startHashCode);
			targetHashCode = targetMap.ToMaxWalkableArea(targetHashCode);

			HashSet<MapHashCode> visited = new HashSet<MapHashCode>();
			HeapQueue<AStarNode> queue = new HeapQueue<AStarNode>(100);

			queue.Enqueue(new AStarNode(EvaluateWeight(startPositionWS, startPositionWS, targetPositionWS, 1), null, startMap, startHashCode));
			visited.Add(new MapHashCode(){map = startMap, hashCode = startHashCode});
			
			AStarNode targetNode = null;
			while (queue.Count > 0)
			{
				AStarNode currentNode = queue.Dequeue();
				IMap currentMap = currentNode.element.map;
				HashCode currentHashCode = currentNode.element.hashCode;
				Bounds currentBox = currentHashCode.DecodeBounds();
				if (currentMap == targetMap && (currentHashCode.Equals(targetHashCode) || currentBox.Contains(targetPositionTS)))
				{
					//	Find Target
					targetNode = currentNode;
					break;
				}
				
				mapGroup.GetWalkableAreas(currentNode.element, visited, out List<MapHashCode> walkableAreas);

				for(int i = 0; i < walkableAreas.Count; i++)
				{
					if (!visited.Contains(walkableAreas[i]))
					{
						IMap nextMap = walkableAreas[i].map;
						HashCode nextHashCode = walkableAreas[i].hashCode;
						Bounds nextBox = nextHashCode.DecodeBounds();
						Vector3 centerWS = nextMap.ToWorldSpace(nextBox.center);
						float weight = EvaluateWeight(centerWS, startPositionWS, targetPositionWS, nextBox.size.x);
						queue.Enqueue(new AStarNode(weight, currentNode,nextMap, nextHashCode));
						visited.Add(walkableAreas[i]);
					}
				}
			}

			if(targetNode == null)
			{
				return null;
			}

			List<NavigationPathPoint> path = new List<NavigationPathPoint>();

			Stack<NavigationPathPoint> pathPointsStack = new Stack<NavigationPathPoint>();
			pathPointsStack.Push(new NavigationPathPoint(){map = targetMap, point = targetPositionTS});
			AStarNode pathNode = targetNode;
			do
			{
				HashCode hashCode = pathNode.element.hashCode;
				Bounds bounds = hashCode.DecodeBounds();
				pathPointsStack.Push( new NavigationPathPoint(){map = pathNode.element.map, point = bounds.center});
				pathNode = pathNode.prevNode;
			}
			while (pathNode.prevNode != null);

			pathPointsStack.Push( new NavigationPathPoint(){map = startMap, point = startPositionTS});

			while (pathPointsStack.Count > 0)
			{
				path.Add(pathPointsStack.Pop());
			}
			return path;
        }
    }
}