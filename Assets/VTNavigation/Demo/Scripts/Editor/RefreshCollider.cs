using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RefreshCollider : MonoBehaviour
{
    [MenuItem("Tools/Refresh Collider")]
    public static void RefreshColliders()
    {
        MeshFilter[] meshFilters = Object.FindObjectsOfType<MeshFilter>();
        if (meshFilters == null || meshFilters.Length == 0)
        {
            return;
        }

        foreach (var meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null) continue;
            Transform transform = meshFilter.transform;
            MeshCollider collider = transform.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = transform.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = meshFilter.sharedMesh;
            }
        }
    }
}
