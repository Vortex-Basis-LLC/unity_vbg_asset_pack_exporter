using System.Collections.Generic;

class AssetPackMesh
{
    public string Name { get; set; }
    public string PackFile { get; set; }
    public string PackFileNode { get; set; }
}

class AssetPackMeshLibrary
{
    public List<AssetPackMesh> Meshes { get; set; }
}

public class AssetPackMeshRef
{
    public string Name { get; set; }
}