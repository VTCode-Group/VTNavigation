using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VTNavigation.Drawer;
using VTNavigation.Scene;

namespace VTNavigation.Streaming
{
	public class RefreshDrawDataTask
	{
		private StreamingManager m_StreamingManager;
		private Thread m_Thread;
		private bool m_Close;

		public RefreshDrawDataTask(StreamingManager manager)
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
			while(!m_Close)
			{
				while (m_StreamingManager.RefreshDrawDataTaskSignal.TryDequeue(out RefreshDrawDataTaskSignal signal))
				{
					VTScene scene = signal.scene;
					List<Bounds> blockBounds = scene.GetBlockBounds();

					RefreshDrawDataTaskResult result = new RefreshDrawDataTaskResult()
					{
						x = signal.x,
						y = signal.y,
						z = signal.z,
						instanceMatries = null,
						count = 0
					};

					if (blockBounds.Count > 0)
					{
						InstanceMatries[] mats = new InstanceMatries[blockBounds.Count];
						for (int i = 0; i < mats.Length; i++)
						{
							mats[i] = new InstanceMatries();
							mats[i].objectToWorldMatrix = Matrix4x4.TRS(blockBounds[i].center, Quaternion.identity, blockBounds[i].size);
						}
						result.instanceMatries = mats;
						result.count = (uint)mats.Length;
					}
					m_StreamingManager.RefreshDrawDataTaskResult.Enqueue(result);

				}

				Thread.Sleep(1000);
			}
		}

		public void Close()
		{
			m_Close = true;
		}
	}
}
