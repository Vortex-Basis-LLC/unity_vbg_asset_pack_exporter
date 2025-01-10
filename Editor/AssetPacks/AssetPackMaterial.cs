using System;
using System.Collections.Generic;
using System.Linq;
using GLTFast.Schema;
using Newtonsoft.Json;
using UnityEngine;



public class AssetPackMaterial
{
    public string Name { get; set; }

    public AssetPackShaderRef Shader { get; set; }

    public AssetPackMaterialPropertySet Props { get; set; }
}

public class AssetPackMaterialLibrary
{
    public List<AssetPackMaterial> Materials { get; set; }
}


// Unity Material Properties for Standard Shader.
// Textures:
//  _MainTex
//  _BumpMap
//  _EmissionMap
// Floats:
//  _SrcBlend
//  _DstBlend
//  _Cutoff
//  _ZWrite
//  _Glossiness
//  _GlossMapScale
//  _SmoothnessTextureChannel
//  _Metallic
//  _SpecularHighlights
//  _GlossyReflections
//  _BumpScale
//  _Parallax
//  _OcclusionStrength
//  _DetailNormalMapScale
//  _UVSec
//  _Mode
// VectorFloats:
//  _MainTex_ST
//  _MainTex_TexelSize
//  _MainTex_HDR
//  _EmissionColor
//  _Color
//  _MetallicGlossMap_ST
//  _MetallicGlossMap_HDR
//  _BumpMap_ST
//  _BumpMap_TexelSize
//  _BumpMap_HDR
//  _ParallaxMap_ST
//  _ParallaxMap_TexelSize
//  _ParallaxMap_HDR
//  _OcclusionMap_ST
//  _OcclusionMap_TexelSize
//  _OcclusionMap_HDR
//  _EmissionMap_ST
//  _EmissionMap_TexelSize
//  _EmissionMap_HDR
//  _DetailMask_ST
//  _DetailMask_TexelSize
//  _DetailMask_HDR
//  _DetailAlbedoMap_ST
//  _DetailAlbedoMap_TexelSize
//  _DetailAlbedoMap_HDR
//  _DetailNormalMap_ST
//  _DetailNormalMap_TexelSize
//  _DetailNormalMap_HDR


public class AssetPackMaterialPropertySet
{
    public AssetPackMaterialTextureProperty[] Textures { get; set; }
    public AssetPackMaterialFloatProperty[] Floats { get; set; }
    public AssetPackMaterialIntProperty[] Ints { get; set; }
    public AssetPackMaterialVectorFloatProperty[] VectorFloats { get; set; }


    /// <summary>
    /// Retrieves set of properties that differ from the given baseSet, or it returns null if their are no differences.
    /// </summary>
    /// <param name="baseSet"></param>
    /// <returns></returns>
    public AssetPackMaterialPropertySet GetDiffSetFromBaseSet(AssetPackMaterialPropertySet baseSet)
    {
        if (baseSet == null)
        {
            return this;
        }

        bool diffFound = false;

        AssetPackMaterialPropertySet diffSet = new AssetPackMaterialPropertySet();

        if (diffFound)
        {
            // Check texture properties.
            if (baseSet.Textures != null)
            {
                List<AssetPackMaterialTextureProperty> newMatProps = new List<AssetPackMaterialTextureProperty>();
                if (this.Textures != null)
                {
                    foreach (var matProp in this.Textures)
                    {
                        if (matProp != null)
                        {
                            var baseProp = baseSet.Textures.FirstOrDefault(t => t.Name == matProp.Name);
                            if (baseProp != null)
                            {
                                if (!baseProp.Value.Equals(matProp.Value))
                                {
                                    diffFound = true;
                                    newMatProps.Add(matProp);
                                }
                            }
                            else
                            {
                                diffFound = true;
                                newMatProps.Add(matProp);
                            }
                        }
                    }
                }
                if (newMatProps.Count > 0)
                {
                    diffSet.Textures = newMatProps.ToArray();
                }
            }
            else
            {
                if (this.Textures != null && this.Textures.Length > 0)
                {
                    diffFound = true;
                    diffSet.Textures = this.Textures;
                }
            }


            // Check float properties.
            if (baseSet.Floats != null)
            {
                List<AssetPackMaterialFloatProperty> newMatProps = new List<AssetPackMaterialFloatProperty>();
                if (this.Floats != null)
                {
                    foreach (var matProp in this.Floats)
                    {
                        if (matProp != null)
                        {
                            var baseProp = baseSet.Floats.FirstOrDefault(t => t.Name == matProp.Name);
                            if (baseProp != null)
                            {
                                if (!baseProp.Value.Equals(matProp.Value))
                                {
                                    diffFound = true;
                                    newMatProps.Add(matProp);
                                }
                            }
                            else
                            {
                                diffFound = true;
                                newMatProps.Add(matProp);
                            }
                        }
                    }
                }
                if (newMatProps.Count > 0)
                {
                    diffSet.Floats = newMatProps.ToArray();
                }
            }
            else
            {
                if (this.Floats != null && this.Floats.Length > 0)
                {
                    diffFound = true;
                    diffSet.Floats = this.Floats;
                }
            }


            // Check float properties.
            if (baseSet.Ints != null)
            {
                List<AssetPackMaterialIntProperty> newMatProps = new List<AssetPackMaterialIntProperty>();
                if (this.Ints != null)
                {
                    foreach (var matProp in this.Ints)
                    {
                        if (matProp != null)
                        {
                            var baseProp = baseSet.Ints.FirstOrDefault(t => t.Name == matProp.Name);
                            if (baseProp != null)
                            {
                                if (!baseProp.Value.Equals(matProp.Value))
                                {
                                    diffFound = true;
                                    newMatProps.Add(matProp);
                                }
                            }
                            else
                            {
                                diffFound = true;
                                newMatProps.Add(matProp);
                            }
                        }
                    }
                }
                if (newMatProps.Count > 0)
                {
                    diffSet.Ints = newMatProps.ToArray();
                }
            }
            else
            {
                if (this.Ints != null && this.Ints.Length > 0)
                {
                    diffFound = true;
                    diffSet.Ints = this.Ints;
                }
            }


            // Check vector properties.
            if (baseSet.VectorFloats != null)
            {
                List<AssetPackMaterialVectorFloatProperty> newMatProps = new List<AssetPackMaterialVectorFloatProperty>();
                if (this.VectorFloats != null)
                {
                    foreach (var matProp in this.VectorFloats)
                    {
                        if (matProp != null)
                        {
                            var baseProp = baseSet.VectorFloats.FirstOrDefault(t => t.Name == matProp.Name);
                            if (baseProp != null)
                            {
                                if (!baseProp.Value.Equals(matProp.Value))
                                {
                                    diffFound = true;
                                    newMatProps.Add(matProp);
                                }
                            }
                            else
                            {
                                diffFound = true;
                                newMatProps.Add(matProp);
                            }
                        }
                    }
                }
                if (newMatProps.Count > 0)
                {
                    diffSet.VectorFloats = newMatProps.ToArray();
                }
            }
            else
            {
                if (this.VectorFloats != null && this.VectorFloats.Length > 0)
                {
                    diffFound = true;
                    diffSet.VectorFloats = this.VectorFloats;
                }
            }


            return diffSet;
        }
        else
        {
            return null;
        }
    }
}

public class AssetPackMaterialTextureProperty
{
    public string Name { get; set; }
    public AssetPackTextureRef Value { get; set; }


    public override bool Equals(object obj)
    {
        var other = obj as AssetPackMaterialTextureProperty;
        if (other != null)
        {
            return Value?.Name == other.Value?.Name;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return Value?.Name?.GetHashCode() ?? 0;
    }
}

public class AssetPackMaterialFloatProperty
{
    public string Name { get; set; }
    public float Value { get; set; }


    public override bool Equals(object obj)
    {
        var other = obj as AssetPackMaterialFloatProperty;
        if (other != null)
            return Value == other.Value;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public class AssetPackMaterialIntProperty
{
    public string Name { get; set; }
    public int Value { get; set; }


    public override bool Equals(object obj)
    {
        var other = obj as AssetPackMaterialIntProperty;
        if (other != null)
            return Value == other.Value;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public class AssetPackMaterialVectorFloatProperty
{
    public string Name { get; set; }
    public float[] Value { get; set; }


    public override bool Equals(object obj)
    {
        var other = obj as AssetPackMaterialVectorFloatProperty;
        if (other != null)
        {
            int len = Value?.Length ?? 0;
            int len2 = Value?.Length ?? 0;
            if (len != len2)
            {
                return false;
            }
            if (len == 0)
            {
                return true;
            }
            for (int co = 0; co < len; co++)
            {
                if (Value[co] != other.Value[co])
                {
                    return false;
                }
            }
            return true;
        }
        else
            return false;
    }

    public override int GetHashCode()
    {
        int hashTotal = 0;
        if (Value != null)
        {
            foreach (float val in Value)
            {
                hashTotal += val.GetHashCode();
            }
        }
        return hashTotal;
    }
}


public class AssetPackColor
{
    public AssetPackColor()
    {
    }

    public AssetPackColor(Color color)
    {
        R = color.r;
        G = color.g;
        B = color.b;
        A = color.a;
    }

    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }
}

public class AssetPackMaterialRef
{
    public string Name { get; set; }

    /// <summary>
    /// This should be a list of overrides for the target material that are different
    /// than the base material.
    /// </summary>
    public AssetPackMaterialPropertySet Props { get; set; }
}