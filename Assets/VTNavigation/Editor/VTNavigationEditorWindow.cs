using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VTNavigation.Editor;
using VTNavigation.Geometry;
using VTNavigation.Scene;
using VTNavigation.Util;
using VTPartition.Scene;

namespace VTPartition.Editor
{
	public class VTNavigationEditorWindow : EditorWindow
	{
		[MenuItem("Tools/VT Navigation/Editor Window")]
		public static void CreateWindow()
		{
			VTNavigationEditorWindow window = EditorWindow.GetWindow<VTNavigationEditorWindow>();
			window.titleContent = new GUIContent("VTNavigation Editor");
			window.Show();
		}

		/********
		 *	配置参数（已保存的值）
		 */
		private Vector3 m_SceneOrigin = Vector3.zero;
		private float m_CustomVoxelSize = 1.0f;
		private int m_XSceneCount = 1;
		private int m_YSceneCount = 1;
		private int m_ZSceneCount = 1;
		private int m_MinBlockLayer = 1;

		/********
		 *	编辑模式临时变量
		 */
		[SerializeField] private bool m_IsEditing = false;
		[SerializeField] private Vector3 m_EditSceneOrigin;
		[SerializeField] private float m_EditCustomVoxelSize;
		[SerializeField] private int m_EditXSceneCount;
		[SerializeField] private int m_EditYSceneCount;
		[SerializeField] private int m_EditZSceneCount;
		[SerializeField] private int m_EditMinBlockLayer;

		/********
		 *	Scene视图面板位置
		 */
		private Rect m_PanelRect = new Rect(20, 20, 320, 260);

		/********
		 *	Bake状态
		 */
		private string m_OutputDirectory = "";
		private string m_SceneName;
		private BakeTreeProgress m_BakeProgress;
		private BakeTask[] m_BakeTasks;
		private VTScene[] m_BakeScenes;
		private int m_TotalPrimitives;

		/********
		 *	AABB可视化颜色
		 */
		private static readonly Color[] k_PartitionColors = new Color[]
		{
			Color.red,
			Color.green,
			Color.blue,
			Color.yellow,
			Color.cyan,
			Color.magenta,
			new Color(1f, 0.5f, 0f),
			new Color(0.5f, 0f, 1f),
			new Color(0f, 1f, 0.5f),
			new Color(1f, 0f, 0.5f)
		};

		/********
		 *	初始化部分
		 */
		private void OnEnable()
		{
			m_OutputDirectory = "";
			m_BakeProgress = new BakeTreeProgress();
			Undo.undoRedoPerformed += OnUndoRedo;
		}

		private void OnDestroy()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;
			if (m_IsEditing)
			{
				SceneView.duringSceneGui -= OnSceneViewGUI;
			}
		}

		private void OnUndoRedo()
		{
			Repaint();
			SceneView.RepaintAll();
		}

		private static class Style
		{
			public static float s_ProgressWidthRatio = 0.8f;
			public static int s_ProgressHeight = 30;
		}

		/********
		 *	辅助方法
		 */
		private static float CalcSceneSize(float voxelSize)
		{
			return 1024f * voxelSize;
		}

		/********
		 *	窗口GUI绘制
		 */
		private void OnGUI()
		{
			if (HasBakeInProgress() && IsBakeComplete())
			{
				OnBakeComplete();
			}

			if (HasBakeInProgress())
			{
				DrawBakeProgress();
			}
			else
			{
				DrawConfiguration();
			}
		}

		private bool HasBakeInProgress()
		{
			return m_BakeTasks != null;
		}

		private bool IsBakeComplete()
		{
			if (m_BakeTasks == null) return false;
			int processed = 0;
			foreach (var task in m_BakeTasks)
			{
				if (task != null)
					processed += task.ProcessedCount;
			}
			return processed >= m_TotalPrimitives && m_TotalPrimitives > 0;
		}

		private void DrawBakeProgress()
		{
			int processedCount = 0;
			foreach (var task in m_BakeTasks)
			{
				if (task != null)
					processedCount += task.ProcessedCount;
			}

			Rect rect = EditorGUILayout.GetControlRect();
			Rect progressRect = rect;
			float offset = rect.width * ((1.0f - Style.s_ProgressWidthRatio) * 0.5f);
			progressRect.width = rect.width * Style.s_ProgressWidthRatio;
			progressRect.height = Style.s_ProgressHeight;
			progressRect.x = offset;
			m_BakeProgress.OnDraw(progressRect, m_TotalPrimitives, processedCount);
		}

		private void DrawConfiguration()
		{
			EditorGUILayout.LabelField("Scene Configuration", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Vector3Field("Scene Origin", m_SceneOrigin);
			EditorGUILayout.FloatField("Custom Voxel Size", m_CustomVoxelSize);

			float sceneSize = CalcSceneSize(m_CustomVoxelSize);
			EditorGUILayout.FloatField("Scene Size", sceneSize);

			EditorGUILayout.IntField("X Scene Count", m_XSceneCount);
			EditorGUILayout.IntField("Y Scene Count", m_YSceneCount);
			EditorGUILayout.IntField("Z Scene Count", m_ZSceneCount);
			EditorGUILayout.IntField("Min Block Layer", m_MinBlockLayer);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();

			if (GUILayout.Button("Load Bake Config"))
			{
				LoadBakeConfig();
			}

			EditorGUILayout.Space();

			if (!m_IsEditing)
			{
				if (GUILayout.Button("Enter Edit Mode"))
				{
					EnterEditMode();
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Editing in Scene view. Use the panel in Scene view to modify parameters.", MessageType.Info);
				if (GUILayout.Button("Exit Edit Mode"))
				{
					ExitEditMode(false);
				}
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Output Directory", GUILayout.Width(100));
			EditorGUILayout.TextField(m_OutputDirectory);
			if (GUILayout.Button("Select"))
			{
				m_OutputDirectory = EditorUtility.OpenFolderPanel("Select Output Directory", "./", "./");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(m_IsEditing);
			if (GUILayout.Button("Bake"))
			{
				if (!string.IsNullOrEmpty(m_OutputDirectory))
				{
					DoBake();
				}
				else
				{
					EditorUtility.DisplayDialog("Error", "Please select an output directory first.", "OK");
				}
			}
			EditorGUI.EndDisabledGroup();
		}

		/********
		 *	编辑模式管理
		 */
		private void EnterEditMode()
		{
			m_IsEditing = true;
			m_EditSceneOrigin = m_SceneOrigin;
			m_EditCustomVoxelSize = m_CustomVoxelSize;
			m_EditXSceneCount = m_XSceneCount;
			m_EditYSceneCount = m_YSceneCount;
			m_EditZSceneCount = m_ZSceneCount;
			m_EditMinBlockLayer = m_MinBlockLayer;
			SceneView.duringSceneGui += OnSceneViewGUI;
			SceneView.RepaintAll();
		}

		private void ExitEditMode(bool save)
		{
			m_IsEditing = false;
			SceneView.duringSceneGui -= OnSceneViewGUI;

			if (save)
			{
				m_SceneOrigin = m_EditSceneOrigin;
				m_CustomVoxelSize = m_EditCustomVoxelSize;
				m_XSceneCount = Mathf.Max(1, m_EditXSceneCount);
				m_YSceneCount = Mathf.Max(1, m_EditYSceneCount);
				m_ZSceneCount = Mathf.Max(1, m_EditZSceneCount);
				m_MinBlockLayer = m_EditMinBlockLayer;
			}

			SceneView.RepaintAll();
			Repaint();
		}

		/********
		 *	Scene视图GUI
		 */
		private void OnSceneViewGUI(SceneView sceneView)
		{
			if (!m_IsEditing) return;

			float handleSize = HandleUtility.GetHandleSize(m_EditSceneOrigin) * 0.3f;
			Handles.color = Color.white;
			EditorGUI.BeginChangeCheck();
			Vector3 newHandlePos = Handles.FreeMoveHandle(
				m_EditSceneOrigin,
				handleSize,
				Vector3.zero,
				Handles.SphereHandleCap);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Move Scene Origin");
				m_EditSceneOrigin = newHandlePos;
			}

			DrawSceneAABBs();

			Handles.BeginGUI();
			m_PanelRect = GUILayout.Window(
				"VTNavigationEditorPanel".GetHashCode(),
				m_PanelRect,
				DrawEditPanel,
				"Navigation Scene Editor");
			Handles.EndGUI();
		}

		private void DrawEditPanel(int windowId)
		{
			EditorGUILayout.Space();

			// 第一行：SceneOrigin
			EditorGUI.BeginChangeCheck();
			Vector3 newOrigin = EditorGUILayout.Vector3Field("SceneOrigin", m_EditSceneOrigin);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Edit SceneOrigin");
				m_EditSceneOrigin = newOrigin;
			}

			// 第二行：CustomVoxelSize Slider（范围0.2-2.0）
			EditorGUI.BeginChangeCheck();
			float newVoxelSize = EditorGUILayout.Slider("CustomVoxelSize", m_EditCustomVoxelSize, 0.2f, 2.0f);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Edit CustomVoxelSize");
				m_EditCustomVoxelSize = newVoxelSize;
			}

			// 第三行：只读SceneSize = 1024 * CustomVoxelSize
			float sceneSize = CalcSceneSize(m_EditCustomVoxelSize);
			EditorGUILayout.FloatField("SceneSize", sceneSize);

			// 第四行：XSceneCount
			EditorGUI.BeginChangeCheck();
			int newX = EditorGUILayout.IntField("XSceneCount", m_EditXSceneCount);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Edit XSceneCount");
				m_EditXSceneCount = Mathf.Max(1, newX);
			}

			// 第五行：YSceneCount
			EditorGUI.BeginChangeCheck();
			int newY = EditorGUILayout.IntField("YSceneCount", m_EditYSceneCount);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Edit YSceneCount");
				m_EditYSceneCount = Mathf.Max(1, newY);
			}

			// 第六行：ZSceneCount
			EditorGUI.BeginChangeCheck();
			int newZ = EditorGUILayout.IntField("ZSceneCount", m_EditZSceneCount);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Edit ZSceneCount");
				m_EditZSceneCount = Mathf.Max(1, newZ);
			}

			// 第七行：MinBlockLayer Slider（范围1-4）
			EditorGUI.BeginChangeCheck();
			int newMinBlockLayer = EditorGUILayout.IntSlider("MinBlockLayer", m_EditMinBlockLayer, 1, 4);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(this, "Edit MinBlockLayer");
				m_EditMinBlockLayer = newMinBlockLayer;
			}

			EditorGUILayout.Space();

			// 第八行：退出和保存按钮
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Exit"))
			{
				ExitEditMode(false);
			}
			if (GUILayout.Button("Save"))
			{
				ExitEditMode(true);
			}
			EditorGUILayout.EndHorizontal();

			GUI.DragWindow();
		}

		private void DrawSceneAABBs()
		{
			float sceneSize = CalcSceneSize(m_EditCustomVoxelSize);

			for (int x = 0; x < m_EditXSceneCount; x++)
			{
				for (int y = 0; y < m_EditYSceneCount; y++)
				{
					for (int z = 0; z < m_EditZSceneCount; z++)
					{
						int colorIndex = (x + y + z) % k_PartitionColors.Length;
						Handles.color = k_PartitionColors[colorIndex];

						Vector3 center = m_EditSceneOrigin + new Vector3(
							(x + 0.5f) * sceneSize,
							(y + 0.5f) * sceneSize,
							(z + 0.5f) * sceneSize);

						Handles.DrawWireCube(center, Vector3.one * sceneSize);
					}
				}
			}
		}

	/********
	 *	加载烘焙配置
	 */
	private void LoadBakeConfig()
	{
		string filePath = EditorUtility.OpenFilePanel("Select Bake Config", "./", "vtgroup");
		if (string.IsNullOrEmpty(filePath)) return;

		try
		{
			using (var fs = File.OpenRead(filePath))
			using (var br = new BinaryReader(fs))
			{
				m_XSceneCount = IOUtil.ReadInt(br);
				m_YSceneCount = IOUtil.ReadInt(br);
				m_ZSceneCount = IOUtil.ReadInt(br);
				m_CustomVoxelSize = IOUtil.ReadFloat(br);
				Bounds sceneBounds = IOUtil.ReadBounds(br);
				m_SceneOrigin = sceneBounds.min;
			}

			Repaint();
		}
		catch (System.Exception e)
		{
			EditorUtility.DisplayDialog("Load Error", $"Failed to load bake config:\n{e.Message}", "OK");
		}
	}

	/********
	 *	Bake逻辑
	 */
		private void DoBake()
		{
			m_SceneName = SceneManager.GetActiveScene().name;

			List<Triangle> triangles = VTSceneUtil.GetAllCollidersTrianglesInWorldSpace();
			if (Terrain.activeTerrain != null)
			{
				triangles.AddRange(VTSceneUtil.GetActiveTerrainTriangleInWorldSpace());
			}

			float sceneSize = CalcSceneSize(m_CustomVoxelSize);

			int totalScenes = m_XSceneCount * m_YSceneCount * m_ZSceneCount;
			m_BakeScenes = new VTScene[totalScenes];

			for (int x = 0; x < m_XSceneCount; x++)
			{
				for (int z = 0; z < m_ZSceneCount; z++)
				{
					for (int y = 0; y < m_YSceneCount; y++)
					{
						int index = y * m_XSceneCount * m_ZSceneCount + z * m_XSceneCount + x;
						Vector3 subOrigin = m_SceneOrigin + new Vector3(
							x * sceneSize,
							y * sceneSize,
							z * sceneSize);
						Vector3 subSize = Vector3.one * sceneSize;
						Bounds subBounds = new Bounds(subOrigin + subSize * 0.5f, subSize);
						m_BakeScenes[index] = new VTScene(subBounds, m_CustomVoxelSize);
					}
				}
			}

			m_BakeTasks = new BakeTask[totalScenes];
			m_TotalPrimitives = 0;

			for (int x = 0; x < m_XSceneCount; x++)
			{
				for (int z = 0; z < m_ZSceneCount; z++)
				{
					for (int y = 0; y < m_YSceneCount; y++)
					{
						int index = y * m_XSceneCount * m_ZSceneCount + z * m_XSceneCount + x;
						Bounds sceneBounds = m_BakeScenes[index].SceneBounds;
						m_BakeTasks[index] = new BakeTask(m_BakeScenes[index], null);

						foreach (Triangle tri in triangles)
						{
							if (GeometryUtil.OverlapTriangle(sceneBounds, tri))
							{
								m_BakeTasks[index].Add(tri.Clone());
							}
						}

						m_TotalPrimitives += m_BakeTasks[index].PremitiveCount;
					}
				}
			}

			for (int i = 0; i < m_BakeTasks.Length; i++)
			{
				if (m_BakeTasks[i] != null)
				{
					m_BakeTasks[i].Start();
				}
			}
		}

		private void OnBakeComplete()
		{
			VTSceneGroup group = new VTSceneGroup(m_XSceneCount, m_YSceneCount, m_ZSceneCount, m_CustomVoxelSize, m_SceneOrigin);
			
			for (int x = 0; x < m_XSceneCount; x++)
			{
				for (int z = 0; z < m_ZSceneCount; z++)
				{
					for (int y = 0; y < m_YSceneCount; y++)
					{
						int index = y * m_XSceneCount * m_ZSceneCount + z * m_XSceneCount + x;
						group.SetSubScene(x,y,z, m_BakeScenes[index]);
					}
				}
			}
			
			group.BlockAllNodesInLayer(m_MinBlockLayer);

			group.WriteToFile(m_OutputDirectory, m_SceneName);

			m_BakeTasks = null;
			m_BakeScenes = null;
		}
	}
}
