using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Tree;

namespace VTNavigation.Navigation
{
    public interface IMap
    {
        public Bounds SceneBounds { get; }
        
        public (int, int, int) GetSubSceneIndex();
        
        public bool IsBlock(HashCode hashCode);
        
        public bool IsFreeSpace(HashCode hashCode);
        
        public bool IsSparse(HashCode hashCode);

        public HashCode GetHashCode(Vector3 positionWS, int layer = 0);
        
		public HashCode ToMaxWalkableArea(HashCode walkableHashCode);
        
        public Vector3 ToMapSpace(Vector3 positionWS);
        
        public Vector3 ToWorldSpace(Vector3 positionTS);
        
        public Bounds ToWorldSpace(Bounds boundsTS);

        public Bounds ToMapSpace(Bounds boundsWS);
        
        public void GetEdgeFreeSpace(HashCode hashCode, List<HashCode> result,
            int filterXMask, int filterYMask, int filterZMask);

        public bool FastRayCastHit(Ray ray, out float minDistance,float edageError = 0.2f);
        
        public bool RayCastHit(Ray ray, out float minDistance, float edageError = 0.2f);
    }
}