using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTNavigation.Scene;

namespace VTNavigation.Drawer
{
	public class VTSceneDrawer : MonoBehaviour
	{
		private VTSceneGroup m_SceneGroup;

		private List<Bounds> m_BlockBounds;

		public Material m_Material;

		public Mesh m_Mesh;

		private ComputeBuffer m_ArgsBuffer;

		private ComputeBuffer m_InstanceMatBuffer;

		public string m_VTSceneGroupFilePath;

		public bool m_DrawPreviewScene = true;


		private void Start()
		{
			LoadSceneGroup();
		}

		public void SetScene(VTSceneGroup scene)
		{
			m_SceneGroup = scene;
			m_BlockBounds = m_SceneGroup.GetAllBlockBounds();
			m_ArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			uint[] args = new uint[5]
			{
				(uint)m_Mesh.GetIndexCount(0),
				(uint)m_BlockBounds.Count,
				(uint)m_Mesh.GetIndexStart(0),
				(uint)m_Mesh.GetBaseVertex(0),
				0
			};
			m_ArgsBuffer.SetData(args);

			InstanceMatries[] mats = new InstanceMatries[m_BlockBounds.Count];
			for (int i = 0; i < mats.Length; i++)
			{
				mats[i] = new InstanceMatries();
				mats[i].objectToWorldMatrix = Matrix4x4.TRS(m_BlockBounds[i].center, Quaternion.identity, m_BlockBounds[i].size);
			}
			m_InstanceMatBuffer = new ComputeBuffer(mats.Length, InstanceMatries.Size());
			m_InstanceMatBuffer.SetData(mats);
			m_Material.SetBuffer("_Matries", m_InstanceMatBuffer);
		}

		public void LoadSceneGroup()
		{
			string path = m_VTSceneGroupFilePath;
			if (File.Exists(path))
			{
				m_SceneGroup = new VTSceneGroup();
				m_SceneGroup.ReadAllFromFile(m_VTSceneGroupFilePath);
				SetScene(m_SceneGroup);
			}
			else
			{
				Debug.LogError($"File {path} not exists.");
			}
		}

		private void Update()
		{
			if (m_SceneGroup != null && m_DrawPreviewScene)
			{
				for (int i = 0; i < m_SceneGroup.SubSceneCount; i++)
				{
					Bounds sceneBounds = m_SceneGroup[i].SceneBounds;
					DrawUtil.DrawBounds(sceneBounds, Color.green);
				}
				Graphics.DrawMeshInstancedIndirect(m_Mesh, 0, m_Material, m_SceneGroup.SceneBounds, m_ArgsBuffer);
			}
		}

		private void OnDestroy()
		{
			m_ArgsBuffer?.Dispose();
			m_InstanceMatBuffer?.Dispose();
		}
	}
}
