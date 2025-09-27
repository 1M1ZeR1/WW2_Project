using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDataHolder", menuName = "Custom/Object Data Holder")]
public class ObjectDataHolder : ScriptableObject
{
    public List<SavedObject> savedObjects;
}

[Serializable]
public class SavedObject
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public MeshData meshData;
    public Material[] materials;
    public bool hasCollider;
}
[Serializable]
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;

    public static MeshData FromMesh(Mesh mesh)
    {
        return new MeshData
        {
            vertices = mesh.vertices,
            triangles = mesh.triangles,
            uv = mesh.uv
        };
    }

    public Mesh ToMesh()
    {
        return new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };
    }
}
