using UnityEngine;
using VTNavigation.Geometry;

namespace VTNavigation.Drawer
{
	[ExecuteInEditMode]
	public class ScenePreview : MonoBehaviour
	{
		public Vector2 m_SceneOrigin;

		public Vector2 m_SceneSize;

		public void SetSceneOrigin(Vector2 sceneOrigin)
		{
			m_SceneOrigin = sceneOrigin;
		}

		public void SetSceneSize(Vector2 sceneSize)
		{
			m_SceneSize = sceneSize;
		}

		public void Update()
		{
			Box2D sceneArea = new Box2D(m_SceneOrigin + m_SceneSize*0.5f, m_SceneSize);
			DrawUtil.DrawBox2D(sceneArea, Color.red);
		}
	}
}
