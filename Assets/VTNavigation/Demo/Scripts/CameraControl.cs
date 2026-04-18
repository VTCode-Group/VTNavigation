using UnityEngine;
namespace VTNavigation2D.Demo
{
	public class CameraControl : MonoBehaviour
	{
		public Vector2 m_SceneMin;
		public Vector2 m_SceneMax;

		public Transform m_Player;

		private float m_SizeX;
		private float m_SizeY;


		private void Awake()
		{
			Camera camera = GetComponent<Camera>();
			m_SizeY = camera.orthographicSize;
			float aspect = camera.aspect;
			m_SizeX = m_SizeY * aspect;
		}

		private void LateUpdate()
		{
			Vector2 position = m_Player.position;

			float posZ = transform.position.z;

			if (position.x - m_SizeX <= m_SceneMin.x)
			{
				position.x = m_SceneMin.x + m_SizeX;
			}
			if (position.x + m_SizeX >= m_SceneMax.x)
			{
				position.x = m_SceneMax.x - m_SizeX;
			}

			if (position.y - m_SizeY <= m_SceneMin.y)
			{
				position.y = m_SceneMin.y + m_SizeY;
			}

			if (position.y + m_SizeY >= m_SceneMax.y)
			{
				position.y = m_SceneMax.y - m_SizeY;
			}
			transform.position = new Vector3(position.x, position.y, posZ);
		}
	}
}