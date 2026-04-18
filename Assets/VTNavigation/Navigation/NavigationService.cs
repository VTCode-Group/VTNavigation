using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VTNavigation.Util;

namespace VTNavigation.Navigation
{
    public class NavigationService:INavService
    {
        public Type ServiceType { get; } = typeof(INavService);
        
        public List<Vector3> QueryPath(IMapGroup mapGroup, Vector3 startPosition, Vector3 targetPosition, bool smooth = false)
        {
            var path= NavigationHelper.QueryPath(mapGroup, startPosition, targetPosition);
            if (!smooth)
            {
                return path;
            }
            
            return PathUtil.SmoothPathWithBezierCurve(path, smoothDistance: 4.0f);
        }

        public IEnumerator QueryPathAsync(IMapGroup mapGroup, Vector3 startPosition, Vector3 targetPosition, List<Vector3> path, bool smooth = false)
        {
            Task<List<Vector3>> task = Task<List<Vector3>>.Run(() =>
            {
                try
                {
                    var path = NavigationHelper.QueryPath(mapGroup, startPosition, targetPosition);;

                    if (!smooth)
                    {
                        return NavigationHelper.QueryPath(mapGroup, startPosition, targetPosition);
                    }
                    return PathUtil.SmoothPathWithBezierCurve(path, smoothDistance: 4.0f);
                    
                }
                catch (Exception exception)
                {
					Debug.LogException(exception);
                    return null;
                }
            });

            yield return new WaitUntil(() => task.IsCompleted);
			path.AddRange(task.Result);
        }
    }
}
