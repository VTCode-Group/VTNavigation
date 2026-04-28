using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Tree;

namespace VTNavigation.Navigation
{
    public interface IMapGroup
    {
        public int SubSceneXCount
        {
            get;
        }

        public int SubSceneYCount
        {
            get;
        }

        public int SubSceneZCount
        {
            get;
        }
        
        IMap GetMap(int x, int y, int z);

        IMap GetMap(Vector3 position);

        MapHashCode GetHashCode(Vector3 position, int layer = 0);

        MapHashCode ToNextHashCode(MapHashCode mapHashCode, int xoffset, int yoffset, int zoffset);

        public void GetWalkableAreas(MapHashCode mapHashCode, HashSet<MapHashCode> ignoreSet,
            out List<MapHashCode> result);

        public bool FastRayCastHit(Ray rayWS, out float minDistance, float edageError = 0.2f);
    }
}