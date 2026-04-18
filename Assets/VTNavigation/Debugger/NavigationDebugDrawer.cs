using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Drawer;

namespace VTNavigation.Debugger
{
    public class NavigationDebugDrawer:MonoBehaviour
    {
        public static NavigationDebugDrawer Instance { get; private set; }
        
        private List<Bounds> m_Bounds;

        public void Awake()
        {
            Instance = this;
            m_Bounds = new List<Bounds>();
        }

        public void AddBoundsToDraw(Bounds bounds)
        {
            m_Bounds.Add(bounds);
        }
        
        public void Update()
        {
            if (m_Bounds.Count > 1)
            {
                foreach (var bounds in m_Bounds)
                {
                    DrawUtil.DrawBounds(bounds, Color.green);
                }
            }
        }
    }
}