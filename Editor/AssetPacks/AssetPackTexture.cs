using System.Collections.Generic;

public class AssetPackTexture
{
    public string Name { get; set; }
    public string File { get; set;}
}

public class AssetPackTextureLibrary
{
    public List<AssetPackTexture> Textures { get; set; }
}

public class AssetPackTextureRef
{
    public string Name { get; set; }
}