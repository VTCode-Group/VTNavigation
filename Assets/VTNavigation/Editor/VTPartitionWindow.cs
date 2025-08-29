using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VTNavigation.Drawer;
using VTNavigation.Geometry;
using VTNavigation.Scene;
using VTNavigation.Tree;

namespace VTNavigation.Editor
{
	public class VTPartitionWindow : EditorWindow
	{
		[MenuItem("VT Partition/Editor Window")]
		public static void CreateWindow()
		{
			VTPartitionWindow window = EditorWindow.GetWindow<VTPartitionWindow>();
			window.titleContent = new GUIContent("VTPartition Window");
			window.Show();
		}

		private string m_OutputDirectory;
		private string m_SceneName;

		private float m_CustomVoxelSize = 1.0f;

		private BakeWork m_Work;
		private VTSceneGroup m_VTSceneGroup;
		private BakeTreeProgress m_BakeProgress;

		/********
		 *	初始化部分
		 */
		private void OnEnable()
		{
			m_OutputDirectory = "";
			m_BakeProgress = new BakeTreeProgress();
		}

		private static class Style
		{
			public static float s_ProgressWidthRatio = 0.8f;
			public static int s_ProgressHeight = 30;
		}

		/********
		 *	绘制部分
		 */
		private void OnGUI()
		{
			if (m_Work == null)
			{
				if (Terrain.activeTerrain != null)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Terrain", GUILayout.Width(100));
					EditorGUILayout.ObjectField(Terrain.activeTerrain, typeof(Terrain), true, GUILayout.ExpandWidth(true));
					EditorGUILayout.EndHorizontal();
				}
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Output Directory",GUILayout.Width(100));
				EditorGUILayout.TextField(m_OutputDirectory);
				if (GUILayout.Button("Select"))
				{
					m_OutputDirectory = EditorUtility.OpenFolderPanel("Select Output Directory", "./", "./");
				}
				EditorGUILayout.EndHorizontal();

				m_CustomVoxelSize = EditorGUILayout.Slider("Custom Voxel Size", m_CustomVoxelSize, 0.2f, 10.0f);
				
				if (GUILayout.Button("Bake"))
				{
					if (!string.IsNullOrEmpty(m_OutputDirectory))
					{
						m_SceneName = SceneManager.GetActiveScene().name;
						m_VTSceneGroup = new VTSceneGroup();

						m_Work = m_VTSceneGroup.BakeVTSceneWithCustomVoxelSize(m_CustomVoxelSize);
						m_Work.DoWork();
					}
				}
			}
			else
			{
				Rect rect = EditorGUILayout.GetControlRect();
				Rect progressRect = rect;
				float offset = rect.width * ((1.0f - Style.s_ProgressWidthRatio) * 0.5f);
				progressRect.width = rect.width * Style.s_ProgressWidthRatio;
				progressRect.height = Style.s_ProgressHeight;
				progressRect.x = offset;
				m_BakeProgress.OnDraw(progressRect, m_Work.SplitPremitiveTaskCount, m_Work.ProcessedCount);
				if (m_Work.IsDone)
				{
					OnBakeComplete(m_VTSceneGroup);
				}
			}
		}

		private void OnBakeComplete(VTSceneGroup sceneGroup)
		{ 
			string path = Path.Combine(m_OutputDirectory, m_SceneName + ".vtgroup");

			sceneGroup.WriteToFile(path);
			sceneGroup.WriteVTSceneToDirectory(m_OutputDirectory, m_SceneName);
			m_Work = null;
		}
	}
}
