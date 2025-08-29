using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VTNavigation.Scene;

namespace VTNavigation.Streaming
{
	public class LoadingTask
	{
		private StreamingManager m_StreamingManager;
		private Thread m_Thread;
		private bool m_Close;
		public LoadingTask(StreamingManager manager)
		{
			m_StreamingManager = manager;
			m_Close = false;
			m_Thread = new Thread(Run);
		}

		public void Start()
		{
			m_Thread.Start();
		}
		
		private void Run()
		{
			string directory = m_StreamingManager.SceneDataDirectory;
			string sceneName = m_StreamingManager.SceneName;
			while(!m_Close)
			{
				while(m_StreamingManager.LoadingTaskData.TryDequeue(out LoadingTaskData loadingData))
				{
					int x = loadingData.x;
					int y = loadingData.y;
					int z = loadingData.z;
					string fileName = $"{sceneName}_{x}_{y}_{z}.vtscene";
					using(var fs = File.OpenRead(directory + "/" + fileName))
					{
						using (var br = new BinaryReader(fs))
						{
							VTScene scene = new VTScene();
							scene.ReadFromMemory(br);
							LoadingTaskResult result = new LoadingTaskResult();
							result.x = x;
							result.y = y;
							result.z = z;
							result.scene = scene;
							m_StreamingManager.LoadedBuffer.Enqueue(result);
						}
					}
				}
				Thread.Sleep(500);
			}
		}

		public void Close()
		{
			m_Close = true;
		}
	}
}
