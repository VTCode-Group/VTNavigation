using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace VTNavigation.Editor
{
	public class BakeTreeProgress
	{
		private Texture2D m_Background;
		private Texture2D m_Process;

		private void GenerateBackground()
		{
			m_Background = new Texture2D(256, 20, TextureFormat.RGBAFloat, false);
			for (int x = 0; x < m_Background.width; x++)
			{
				for(int y = 0;y< m_Background.height;y++)
				{
					m_Background.SetPixel(x, y, Color.white);
				}
			}
			for(int x = 1; x < m_Background.width - 1; x++)
			{
				for(int y = 1; y < m_Background.height -1; y++)
				{
					m_Background.SetPixel(x, y, Color.black);
				}
			}
			m_Background.Apply();
		}

		private void GenerateProcess()
		{
			m_Process = new Texture2D(256, 20);
			for (int x = 0; x < m_Process.width; x++)
			{
				for (int y = 0; y < m_Process.height; y++)
				{
					m_Process.SetPixel(x, y, Color.white);
				}
			}
			for (int x = 1; x < m_Process.width - 1; x++)
			{
				for (int y = 1; y < m_Process.height - 1; y++)
				{
					m_Process.SetPixel(x, y, Color.green);
				}
			}
			m_Process.Apply();
		}

		public void OnDraw(Rect rect, int triangleCount, int processCount)
		{
			if(m_Background == null)
			{
				GenerateBackground();
			}
			if(m_Process == null)
			{
				GenerateProcess();
			}
			EditorGUI.DrawPreviewTexture(rect, m_Background);
			Rect processRect = rect;
			processRect.x += 1;
			processRect.width = (rect.width - 2) * ((float)processCount /(float)triangleCount);
			EditorGUI.DrawPreviewTexture(processRect, m_Process);

			string content = $"{processCount}/{triangleCount}";
			float contentWidth = content.Length * 20;
			float contentHeight = 15;
			float centerX = rect.x + rect.width / 2;
			float centerY = rect.y + rect.height / 2;
			Rect contentDrawRect = new Rect(centerX - contentWidth / 2, centerY - contentHeight / 2, contentWidth, contentHeight);

			TextAnchor old = GUI.skin.label.alignment;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			EditorGUI.LabelField(contentDrawRect, content, GUI.skin.label);
			GUI.skin.label.alignment = old;
		}
	}
}
