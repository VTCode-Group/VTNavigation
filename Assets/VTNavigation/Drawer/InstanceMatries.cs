using UnityEngine;

namespace VTNavigation.Drawer
{
	public struct InstanceMatries
	{
		public Matrix4x4 objectToWorldMatrix;

		public static int Size()
		{
			return sizeof(float) * 4 * 4;
		}
	}
}
