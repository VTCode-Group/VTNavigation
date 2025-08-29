using VTNavigation.Drawer;
using VTNavigation.Scene;

namespace VTNavigation.Streaming
{
	public struct RefreshDrawDataTaskSignal
	{
		public int x;
		public int y;
		public int z;
		public VTScene scene;
	}

	public struct RefreshDrawDataTaskResult
	{
		public int x;
		public int y;
		public int z;
		public InstanceMatries[] instanceMatries;

		public uint count;
	}
}
