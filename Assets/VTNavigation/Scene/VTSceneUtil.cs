using System.Collections.Generic;
using UnityEngine;
using VTNavigation.Geometry;
using VTNavigation.Scene;
using VTNavigation.Tree;

namespace VTPartition.Scene
{
	public static class VTSceneUtil
	{
		public static float ToTreeSpace(this VTScene scene, float value)
		{
			return OCTreeUtil.ToTreeSpace(scene.Tree, value);
		}

		public static float ToWorldSpace(this VTScene scene, float value)
		{
			return OCTreeUtil.ToWorldSpace(scene.Tree, value);
		}

		public static Vector3 SizeToTreeSpace(this VTScene scene, Vector3 value)
		{
			return OCTreeUtil.ToTreeSpace(scene.Tree, value);
		}

		public static Vector3 SizeToWorldSpace(this VTScene scene, Vector3 value)
		{
			return OCTreeUtil.ToWorldSpace(scene.Tree, value);
		}

		public static Vector3 PointToTreeSpace(this VTScene scene, Vector3 value)
		{
			Vector3 offset = value - scene.SceneOrigin;
			return OCTreeUtil.ToTreeSpace(scene.Tree, offset);
		}

		public static Vector3 PointToWorldSpace(this VTScene scene, Vector3 value)
		{
			Vector3 offset = OCTreeUtil.ToWorldSpace(scene.Tree, value);
			return offset + scene.SceneOrigin;
		}

		public static Ray ToTreeSpace(this VTScene scene, Ray ray)
		{
			Vector3 origin = ray.origin;
			Vector3 direction = ray.direction;
			return new Ray(scene.PointToTreeSpace(origin), direction);
		}

		public static Ray ToWorldSpace(this VTScene scene, Ray ray)
		{
			Vector3 origin = ray.origin;
			Vector3 direction = ray.direction;
			return new Ray(scene.PointToWorldSpace(origin), direction);
		}

		public static void PointsToTreeSpace(this VTScene scene, List<Vector3> points)
		{
			for (int i = 0; i < points.Count; i++)
			{
				points[i] = PointToTreeSpace(scene, points[i]);
			}
		}

		public static Bounds ToTreeSpace(this VTScene scene, Bounds bounds)
		{
			Vector3 minInTreeSpace = PointToTreeSpace(scene, bounds.min);
			Vector3 sizeInTreeSpace = SizeToTreeSpace(scene, bounds.size);
			Vector3 center = minInTreeSpace + sizeInTreeSpace * 0.5f;
			return new Bounds(center, sizeInTreeSpace);
		}

		public static Bounds ToWorldSpace(this VTScene scene, Bounds bounds)
		{
			Vector3 minInWorldSpace = PointToWorldSpace(scene, bounds.min);
			Vector3 sizeInWorldSpace = SizeToWorldSpace(scene, bounds.size);
			Vector3 center = minInWorldSpace + sizeInWorldSpace * 0.5f;
			return new Bounds(center, sizeInWorldSpace);
		}

		public static void ToWorldSpace(this VTScene scene, List<Bounds> boundsList)
		{
			for (int i = 0; i < boundsList.Count; ++i)
			{
				boundsList[i] = ToWorldSpace(scene, boundsList[i]);
			}
		}

		public static void ToTreeSpace(this VTScene scene, List<Bounds> boundsList)
		{
			for (int i = 0; i < boundsList.Count; ++i)
			{
				boundsList[i] = ToTreeSpace(scene, boundsList[i]);
			}
		}

		public static Triangle ToTreeSpace(this VTScene scene,  Triangle tri)
		{
			Triangle res = tri.Clone();
			res[0] = PointToTreeSpace(scene, res[0]);
			res[1] = PointToTreeSpace(scene, res[1]);
			res[2] = PointToTreeSpace(scene, res[2]);
			return res;
		}

		public static Triangle ToWorldSpace(this VTScene scene, Triangle tri)
		{
			Triangle res = tri.Clone();
			res[0] = PointToWorldSpace(scene, res[0]);
			res[1] = PointToWorldSpace(scene, res[1]);
			res[2] = PointToWorldSpace(scene, res[2]);
			return res;
		}

		public static void ToWorldSpace(this VTScene scene, List<Triangle> triangles)
		{
			for (int i = 0; i < triangles.Count; ++i)
			{
				Triangle triangle = triangles[i];
				triangle[0] = PointToWorldSpace(scene, triangles[i][0]);
				triangle[1] = PointToWorldSpace(scene, triangles[i][1]);
				triangle[2] = PointToWorldSpace(scene, triangles[i][2]);

				triangles[i] = triangle;
			}
		}

		public static void ToTreeSpace(this VTScene scene, List<Triangle> triangles)
		{
			for (int i = 0; i < triangles.Count; ++i)
			{
				Triangle triangle = triangles[i];
				triangle[0] = PointToTreeSpace(scene, triangle[0]);
				triangle[1] = PointToTreeSpace(scene, triangle[1]);
				triangle[2] = PointToTreeSpace(scene, triangle[2]);
				triangles[i] = triangle;
			}
		}

		public static List<Triangle> GetAllMeshTrianglesInWorldSpace()
		{
			List<Triangle> triangles = new List<Triangle>();
			MeshFilter[] filters = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
			foreach (MeshFilter filter in filters)
			{
				Mesh mesh = filter.sharedMesh;
				Matrix4x4 objectToWorld = filter.transform.localToWorldMatrix;
				for (int i = 0; i < mesh.triangles.Length; i += 3)
				{
					Vector3 p0 = mesh.vertices[mesh.triangles[i + 0]];
					Vector3 p1 = mesh.vertices[mesh.triangles[i + 1]];
					Vector3 p2 = mesh.vertices[mesh.triangles[i + 2]];
					p0 = objectToWorld.MultiplyPoint(p0);
					p1 = objectToWorld.MultiplyPoint(p1);
					p2 = objectToWorld.MultiplyPoint(p2);
					triangles.Add(new Triangle(new Vector3[] { p0, p1, p2 }));
				}
			}
			return triangles;
		}

		public static List<Triangle> GetAllCollidersTrianglesInWorldSpace()
		{
			List<Triangle> triangles = new List<Triangle>();
			List<Triangle> boxCollidersTri = GetAllBoxCollidersTrianglesInWorldSpace();
			List<Triangle> meshCollidersTri = GetAllMeshCollidersTrianglesInWorldSpace();
			triangles.AddRange(boxCollidersTri);
			triangles.AddRange(meshCollidersTri);
			return triangles;
		}

		public static List<Triangle> GetAllBoxCollidersTrianglesInWorldSpace()
		{
			List<Triangle> triangles = new List<Triangle>();
			BoxCollider[] boxColliders = UnityEngine.Object.FindObjectsOfType<BoxCollider>();
			foreach (BoxCollider collider in boxColliders)
			{
				Vector3 localCenter = collider.center;
				Vector3 localExtents = collider.size * 0.5f;
				Vector3[] localCorners = new Vector3[8];
				localCorners[0] = localCenter + new Vector3(-localExtents.x, -localExtents.y, -localExtents.z);
				localCorners[1] = localCenter + new Vector3(-localExtents.x, -localExtents.y,  localExtents.z);
				localCorners[2] = localCenter + new Vector3( localExtents.x, -localExtents.y,  localExtents.z);
				localCorners[3] = localCenter + new Vector3( localExtents.x, -localExtents.y, -localExtents.z);
				localCorners[4] = localCenter + new Vector3(-localExtents.x,  localExtents.y, -localExtents.z);
				localCorners[5] = localCenter + new Vector3(-localExtents.x,  localExtents.y,  localExtents.z);
				localCorners[6] = localCenter + new Vector3( localExtents.x,  localExtents.y,  localExtents.z);
				localCorners[7] = localCenter + new Vector3( localExtents.x,  localExtents.y, -localExtents.z);

				Transform transform = collider.transform;
				Vector3[] worldCorners = new Vector3[8];
				for (int i = 0; i < 8; i++)
				{
					worldCorners[i] = transform.TransformPoint(localCorners[i]);
				}
				triangles.AddRange(GeometryUtil.BoxCornersToTriangles(worldCorners));
			}
			return triangles;
		}

		public static List<Triangle> GetAllMeshCollidersTrianglesInWorldSpace()
		{
			List<Triangle> triangles = new List<Triangle>();
			MeshCollider[] meshColliders = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
			foreach (MeshCollider collider in meshColliders)
			{
				if (!collider.enabled) continue;
				MeshFilter meshFilter = collider.gameObject.GetComponent<MeshFilter>();
				if (meshFilter == null) continue;
				Mesh mesh = meshFilter.sharedMesh;
				if (mesh == null) continue;
				Matrix4x4 objectToWorld = collider.transform.localToWorldMatrix;
				for (int i = 0; i < mesh.triangles.Length; i += 3)
				{
					Vector3 p0 = mesh.vertices[mesh.triangles[i + 0]];
					Vector3 p1 = mesh.vertices[mesh.triangles[i + 1]];
					Vector3 p2 = mesh.vertices[mesh.triangles[i + 2]];
					p0 = objectToWorld.MultiplyPoint(p0);
					p1 = objectToWorld.MultiplyPoint(p1);
					p2 = objectToWorld.MultiplyPoint(p2);
					triangles.Add(new Triangle(new Vector3[] { p0, p1, p2 }));
				}
			}
			return triangles;
		}

		public static List<Triangle> GetActiveTerrainTriangleInWorldSpace()
		{
			List<Triangle> res = new List<Triangle>();
			TerrainData terrainData = Terrain.activeTerrain.terrainData;
			int pointCount = terrainData.heightmapResolution;
			Vector3 size = terrainData.size;
			Vector3 position = Terrain.activeTerrain.transform.position;
			float vertexDistanceX = size.x / (pointCount - 1);
			float vertexDistanceZ = size.z / (pointCount - 1);

			for (int z = 0; z < pointCount - 1; z++)
			{
				for (int x = 0; x < pointCount - 1; x++)
				{
					Vector3[] quadPoints = new Vector3[4];
					quadPoints[0] = new Vector3(x * vertexDistanceX, terrainData.GetHeight(x, z), z * vertexDistanceZ);
					quadPoints[1] = new Vector3(x * vertexDistanceX, terrainData.GetHeight(x, z + 1), (z + 1) * vertexDistanceZ);
					quadPoints[2] = new Vector3((x + 1) * vertexDistanceX, terrainData.GetHeight(x + 1, z), z * vertexDistanceZ);
					quadPoints[3] = new Vector3((x + 1) * vertexDistanceX, terrainData.GetHeight(x + 1, z + 1), (z + 1) * vertexDistanceZ);
					Triangle[] triangles = new Triangle[2];
					triangles[0] = new Triangle(new Vector3[3]
					{
						quadPoints[0],quadPoints[1], quadPoints[2]
					});
					triangles[1] = new Triangle(new Vector3[3]
					{
						quadPoints[2],quadPoints[1],quadPoints[3]
					});
					res.AddRange(triangles);

				}
			}
			for (int i = 0; i < res.Count; i++)
			{
				Triangle triangle = res[i];
				triangle[0] += position;
				triangle[1] += position;
				triangle[2] += position;
				res[i] = triangle;
			}
			return res;
		}
	}
}
