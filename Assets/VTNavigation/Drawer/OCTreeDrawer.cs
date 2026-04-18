using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Tree;

namespace VTNavigation.Drawer
{
    [ExecuteInEditMode]
    public class OCTreeDrawer : MonoBehaviour
    {
        private OCTree m_Tree;
        private List<Bounds> m_Bounds;
        public void LoadTree(string path)
        {
            m_Tree = new OCTree(1, false);
            m_Tree.ReadFromFile(path);
            m_Bounds = m_Tree.FindBlockBounds();
            OCTreeUtil.ToWorldSpace(m_Tree, m_Bounds);
        }

        public void SetTree(OCTree tree)
        {
            m_Tree = tree;
            m_Bounds = m_Tree.FindBlockBounds();
            OCTreeUtil.ToWorldSpace(m_Tree, m_Bounds);
        }

        public void SetBounds(List<Bounds> bounds)
        {
            m_Bounds = bounds;
        }

        public void Update()
        {
            if (m_Bounds != null)
            {
                foreach (var bounds in m_Bounds)
                {
                    DrawUtil.DrawBounds(bounds,Color.red);
                }
            }
        }
    }
}