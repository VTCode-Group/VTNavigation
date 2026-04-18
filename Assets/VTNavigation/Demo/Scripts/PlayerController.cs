using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Navigation;
using VTNavigation.Serivces;

namespace VTNavigation.Demo
{
	public class PlayerController : MonoBehaviour
	{
		public Transform m_Target;

		private Vector3 m_Destination;

		private IEnumerator TraceTargetProcedure()
		{
			if (m_Target != null)
			{
				Vector3 targetPosition = m_Target.position;

				INavService navigationService = ServiceLocator.Instance.GetService<INavService>();
				if (navigationService != null)
				{
					// List<Vector3> path = navigationService.QueryPath(GameManager.Instance.Map, transform.position, targetPosition, true);

					List<Vector3> path = new List<Vector3>();
					yield return navigationService.QueryPathAsync(GameManager.Instance.Map, transform.position, targetPosition, path, true);
					
					if (path.Count > 0)
					{
						NavigationAgent agent = GetComponent<NavigationAgent>();
#if UNITY_EDITOR
						agent.SetPathToDraw(path);
#endif
						for (int i = 1; i < path.Count; i++)
						{
							Vector3 currentTarget = path[i];
							agent.SetDestination(currentTarget);
							yield return new WaitUntil(() => !agent.IsTraceState);
						}
					}
					else
					{
						Debug.LogError($"Find Path Failed. Start Position: ({transform.position.x},{transform.position.y}).  Target Position: ({targetPosition.x},{targetPosition.y}).");
					}
				}
			}
		}

		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(TraceTargetProcedure());
		}
	}
}