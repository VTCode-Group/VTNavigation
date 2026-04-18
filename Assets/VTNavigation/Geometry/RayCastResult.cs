using UnityEngine;

namespace VTNavigation.Geometry
{
    public struct RayCastResult
    {
        public Vector3 origin;
        public Vector3 position;
        public bool hit;
        public float distance;
        public Bounds blockerBounds;

        public static RayCastResult Create()
        {
            RayCastResult res = new RayCastResult();
            res.hit = false;
            res.origin = Vector3.zero;
            res.distance = 99999999.0f;
            res.position = Vector3.zero;
            return res;
        }
    }
}