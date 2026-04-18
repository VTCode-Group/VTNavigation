using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Geometry;

namespace VTNavigation.Drawer
{
	public class EarclippingTest : MonoBehaviour
	{
		public PolygonCollider2D m_PolygonCollider;
		private List<Triangle2D> m_Triangles;

		// Start is called before the first frame update
		void Start()
		{
			Vector2[] points = m_PolygonCollider.points;
			for (int i = 0; i < points.Length; i++)
			{
				Vector2 offset = m_PolygonCollider.transform.position;
				points[i] += offset;
			}
			m_Triangles = GeometryUtil.PolygonToTriangle(points);

		}

		// Update is called once per frame
		void Update()
		{
			for (int i = 0; i < m_Triangles.Count; i++)
			{
				DrawUtil.DrawTriangle(m_Triangles[i], Color.red);
			}
		}
	}
}