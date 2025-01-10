using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

class AssetPackLibrary
{
    public string SourceEngine { get; set; }
    public AssetPackCoordinateSystem CoordinateSystem { get; set; }

    public AssetPackTextureLibrary TextureLibrary { get; set; } 
    public AssetPackMaterialLibrary MaterialLibrary { get; set; }
    public AssetPackMeshLibrary MeshLibrary { get; set; }
    public AssetPackSceneLibrary SceneLibrary { get; set; }
}


public class AssetPackCoordinateSystem
{
    public bool RightHanded { get; set; } = false;
    public AssetPackAxisEnum? UpAxis { get; set; }
    public AssetPackAxisEnum? ForwardAxis { get; set; }

    public float? UnitsInOneMeter { get; set; }

    public static AssetPackCoordinateSystem GetUnityCoordinateSystem()
    {
        return new AssetPackCoordinateSystem
        {
            RightHanded = false,
            UpAxis = AssetPackAxisEnum.Y,
            ForwardAxis = AssetPackAxisEnum.Z,
            UnitsInOneMeter = 1.0f
        };
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum AssetPackAxisEnum
{
    X = 2,
    Y = 4,
    Z = 8,
    NegX = 3,
    NegY = 5,
    NegZ = 9
}