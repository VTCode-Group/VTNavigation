using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Util;

namespace VTNavigation.Geometry
{
    public class BezierCurve
    {
        public List<Vector3> m_ControlPoints;
        private List<float> m_BezierFactors;

        private int m_Steps;

        public BezierCurve(List<Vector3> controlPoints)
        {
            m_ControlPoints = controlPoints;
            m_Steps = m_ControlPoints.Count - 1;
            m_BezierFactors = new List<float>();
            int pointCount = m_ControlPoints.Count;
            for (int i = 0; i<=m_Steps; i++)
            {
                m_BezierFactors.Add(CalculateFactor(m_Steps, i));
            }
        }

        public Vector3 Evaluate(float t)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i <= m_Steps; i++)
            {
                sum += m_BezierFactors[i] *Mathf.Pow(t,i)*Mathf.Pow(1-t,m_Steps-i)* m_ControlPoints[i];
            }
            return sum;
        }

        private float CalculateFactor(int n, int i)
        {
            float stepN = MathHelper.Step(n);
            float stepI = MathHelper.Step(i);
            float stepNI = MathHelper.Step(n - i);
            return stepN / (stepI * stepNI);
        }
    }
}