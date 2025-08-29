

using System;
using UnityEngine;

namespace VTNavigation.Geometry
{
	public struct Triangle
	{
		private Vector3[] m_Points;

		public Vector3 P0
		{
			get
			{
				return m_Points[0];
			}
		}

		public Vector3 P1
		{
			get
			{
				return m_Points[1];
			}
		}

		public Vector3 P2
		{
			get
			{
				return m_Points[2];
			}
		}

		public Triangle(Vector3[] trianglePoints)
		{
			m_Points = new Vector3[trianglePoints.Length];
			Array.Copy(trianglePoints, m_Points, m_Points.Length);
		}

		public Vector3 this[int index]
		{
			get
			{
				if (index < 0) index = 0;
				if(index >= m_Points.Length) index = m_Points.Length - 1;
				return m_Points[index];
			}
			set
			{
				if (index < 0) index = 0;
				if (index >= m_Points.Length) index = m_Points.Length - 1;
				m_Points[index] = value;
			}
		}

		public Triangle Clone()
		{
			Vector3[] points = new Vector3[3]
			{
				m_Points[0],
				m_Points[1],
				m_Points[2]
			};
			return new Triangle(points);
		}
	}
}
