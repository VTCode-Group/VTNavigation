using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Drawer;

namespace VTNavigation.Navigation
{
	public class NavigationAgent : MonoBehaviour
	{
		public float m_StopDistance;

		public float m_MoveSpeed;
		
		private Vector3 m_Destination;
		
		public bool m_IsTraceState;

		public float m_RotatePower;

		public bool IsTraceState
		{
			get {  return m_IsTraceState; }
		}

#if UNITY_EDITOR
		public List<Vector3> m_PathToDraw;
#endif

		public void SetDestination(Vector3 destination)
		{
			m_Destination = destination;

			m_IsTraceState = true;
		}

		private void Update()
		{
			if (m_IsTraceState)
			{
				Vector3 currentPosition = transform.position;
				float distance = Vector3.Distance(currentPosition, m_Destination);
				if(distance <= m_StopDistance)
				{
					m_IsTraceState = false;
					transform.position = m_Destination;
					return;
				}

				Vector3 moveDirection = (m_Destination - currentPosition).normalized;
				Vector3 nextPosition = currentPosition + moveDirection * Time.deltaTime * m_MoveSpeed;

				float moveDistance = Vector3.Distance(nextPosition, currentPosition);
				if(moveDistance > distance)
				{
					transform.position = m_Destination;
					m_IsTraceState = false;
				}
				else
				{
					transform.position = nextPosition;
				}
				transform.forward = Vector3.Lerp(transform.forward,moveDirection,m_RotatePower*Time.deltaTime);
			}

#if UNITY_EDITOR
			DrawPath();
#endif
		}

#if UNITY_EDITOR
		public void SetPathToDraw(List<Vector3> pathToDraw)
		{
			m_PathToDraw = pathToDraw;
		}
		private void DrawPath()
		{
			if(m_PathToDraw != null)
			{
				for(int i = 1;i< m_PathToDraw.Count;i++)
				{
					DrawUtil.DrawLine(m_PathToDraw[i], m_PathToDraw[i - 1], Color.blue);
				}
			}
		}
#endif
	}
}
