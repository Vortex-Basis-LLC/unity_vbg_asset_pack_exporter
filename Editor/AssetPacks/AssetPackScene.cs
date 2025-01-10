using System.Collections.Generic;
using UnityEngine;

class AssetPackScene 
{
    public string Name { get; set; }

    /// <summary>
    /// Folder for further qualifying the scene name.
    /// </summary>
    public string Folder { get; set; }


    public AssetPackSceneNode Root { get; set; }

    /// <summary>
    /// For engines such as Unity that distinguish between prefabs and levels, this can be set to true to indicate a full level file.
    /// </summary>
    public bool IsLevel { get; set; }

    /// <summary>
    /// Path to JSON file within the exported folder for scenes (scn).
    /// </summary>
    public string ExternalFile { get; set; }
}


class AssetPackSceneRef
{
    public string Name { get; set; }
}


class AssetPackSceneLibrary
{
    public List<AssetPackScene> Scenes { get; set; }
}

class AssetPackSceneNode
{
    public string Name { get; set; }
    public List<AssetPackSceneNode> ChildNodes { get; set; }

    public AssetPackVector3 Position { get; set; }
    public AssetPackVector3 Scale { get; set; }
    public AssetPackQuaternion Rotation { get; set; }

    /// <summary>
    /// Prefab nodes will reference an existing scene instead of respecifying the values.
    /// TODO: Provide way to show overridden values.
    /// </summary>
    public AssetPackSceneRef SceneRef { get; set; }

    public AssetPackMeshRef Mesh { get; set; }
    public AssetPackMeshRef Collider { get; set; }
    public AssetPackMaterialRef[] Materials { get; set; }
}

class AssetPackVector3
{
    public AssetPackVector3(){}

    public AssetPackVector3(Vector3 v)
    {
        X = v.x; Y = v.y; Z = v.z;
    }

    public static AssetPackVector3 FromVector3IgnoreDefault(Vector3 value, Vector3 defaultValue)
    {
        if (value == defaultValue)
        {
            return null;
        }
        else
        {
            return new AssetPackVector3(value);
        }
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

class AssetPackBasis
{
    public AssetPackBasis()
    {
    }

    public static AssetPackBasis FromTransform(Transform t)
    {
        var basis = new AssetPackBasis();
        basis.X = new AssetPackVector3(t.right);
        basis.Y = new AssetPackVector3(t.up);
        basis.Z = new AssetPackVector3(t.forward);
        return basis;
    }

    public AssetPackVector3 X { get; set; }
    public AssetPackVector3 Y { get; set; }
    public AssetPackVector3 Z { get; set; }
}

class AssetPackQuaternion
{
    public AssetPackQuaternion(){}

    public AssetPackQuaternion(Quaternion q)
    {
        X = q.x; Y = q.y; Z = q.z; W = q.w;
    }

    public static AssetPackQuaternion FromQuaternionIgnoreIdentity(Quaternion value)
    {
        if (value == Quaternion.identity)
        {
            return null;
        }
        else
        {
            return new AssetPackQuaternion(value);
        }
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }
}