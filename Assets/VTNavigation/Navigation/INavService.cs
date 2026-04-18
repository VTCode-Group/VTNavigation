using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Serivces;

namespace VTNavigation.Navigation
{
    public interface INavService : IService
    {
        public List<Vector3> QueryPath(IMapGroup map, Vector3 startPosition, Vector3 targetPosition, bool smooth = false);

		public IEnumerator QueryPathAsync(IMapGroup map, Vector3 startPosition, Vector3 targetPosition, List<Vector3> path, bool smooth = false);
    }
}