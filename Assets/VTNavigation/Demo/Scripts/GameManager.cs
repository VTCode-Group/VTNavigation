using System.IO;
using UnityEngine;
using VTNavigation.Navigation;
using VTNavigation.Scene;
using VTNavigation.Serivces;

namespace VTNavigation.Demo
{
	public class GameManager : MonoBehaviour
	{
		private static GameManager instance;
		public static GameManager Instance { get { return instance; } }

		private NavigationService m_NavigationService;
		private VTSceneGroup m_SceneGroup;

		public IMapGroup Map
		{
			get
			{
				return m_SceneGroup;
			}
		}

		private void Awake()
		{
			instance = this;
			LoadDemoScene();

			m_NavigationService = new NavigationService();
			ServiceLocator.Instance.AddService<INavService>(m_NavigationService);
		}

		private void LoadDemoScene()
		{
			byte[] sceneData = File.ReadAllBytes("Assets/Resources/Demo.vtgroup");
			if (sceneData == null || sceneData.Length <= 0)
			{
				Debug.LogError($"Read Scene Data Failed.");
				return;
			}
			m_SceneGroup = new VTSceneGroup();
			m_SceneGroup.ReadAllFromFile("Assets/Resources/Demo.vtgroup");
		}
	}
}