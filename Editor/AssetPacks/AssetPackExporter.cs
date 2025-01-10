
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using GLTFast;
using GLTFast.Export;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Drawing.Printing;
using UnityEditor.Formats.Fbx.Exporter;

public class AssetPackExporter
{
    /// <summary>
    /// Folder path within Unity's Assets folder. If "./Assets/SomeAssets" is where assets
    /// reside in your Unity project, you should use "SomeAssets" here.
    /// </summary>

    public string SourceFolderInAssets { get; set; }

    /// <summary>
    /// This should be a fully qualified path to where the assets should be exported.
    /// </summary>
    public string TargetFolder { get; set; } 

    private AssetPackLibrary assetPackLibrary;
    private string targetImageFolder;
    private string targetMeshFolder;
    private string targetSceneFolder;

    private Dictionary<string, AssetPackMaterial> materialNameToMaterialMap = new Dictionary<string, AssetPackMaterial>();
    private Dictionary<string, string> prefabAssetPathToPrefabNameMap = new Dictionary<string, string>();


    public void ExportAssetPack()
    {
        Debug.Log("AssetPackPackager::ExportAssetPack started.");

        if (SourceFolderInAssets == null)
        {
            throw new Exception("SourceFolderInAssets not set.");
        }

        if (TargetFolder == null)
        {
            throw new Exception("TargetFolder not set.");
        }

        targetImageFolder = Path.Combine(TargetFolder, "img");
        if (!Directory.Exists(targetImageFolder))
        {
            Directory.CreateDirectory(targetImageFolder);
        }

        targetMeshFolder = Path.Combine(TargetFolder, "mesh");
        if (!Directory.Exists(targetMeshFolder))
        {
            Directory.CreateDirectory(targetMeshFolder);
        }

        targetSceneFolder = Path.Combine(TargetFolder, "scn");
        if (!Directory.Exists(targetSceneFolder))
        {
            Directory.CreateDirectory(targetSceneFolder);
        } 

        assetPackLibrary = new AssetPackLibrary();
        assetPackLibrary.SourceEngine = "Unity";
        assetPackLibrary.CoordinateSystem = AssetPackCoordinateSystem.GetUnityCoordinateSystem();

        ExportTextures();
        ExportMaterials();
        ExportMeshes();
        ExportScenes();

        SerializeObjectToJsonFile(assetPackLibrary, Path.Combine(TargetFolder, "asset_pack.json"));
        Debug.Log("AssetPackPackager::ExportAssetPack finished.");
    }

    private void SerializeObjectToJsonFile(object obj, string file)
    {
        // TODO: Only use Indented formatting when debugging (or make it an option).
        string json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
        {
           NullValueHandling = NullValueHandling.Ignore 
        });
        File.WriteAllText(file, json);
    }


    private void ExportTextures()
    {
        string root = Path.Combine(Application.dataPath, SourceFolderInAssets);
        string assetPackBasePath = root;

        List<AssetPackTexture> textureList = new List<AssetPackTexture>();

        string[] files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
        
        foreach (string file in files)
        {
            string extension = Path.GetExtension(file).ToLower();

            // TODO: Support other texture file types.
            if (extension == ".png" || extension == ".tga" || extension == "jpg" || extension == "jpeg")
            {
                // relPath is the path with "Asset/" prefix.
                string relPath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName, file);
                string packRelPath = Path.GetRelativePath(assetPackBasePath, file);
                string packRelFolderPath = Path.GetDirectoryName(packRelPath);

                string destImageFolder = Path.Combine(this.targetImageFolder, packRelFolderPath);
                if (!Directory.Exists(destImageFolder))
                {
                    Directory.CreateDirectory(destImageFolder);
                }

                string destImageFile = Path.Combine(this.targetImageFolder, packRelPath);
                File.Copy(file, destImageFile, overwrite: true);

                var tex = new AssetPackTexture
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    File = packRelPath
                };

                textureList.Add(tex);
            }
        }

        var texLib = new AssetPackTextureLibrary()
        {
            Textures = textureList
        };
        assetPackLibrary.TextureLibrary = texLib;
    }


    private void ExportMaterials() 
    {
        string root = Path.Combine(Application.dataPath, SourceFolderInAssets);

        List<AssetPackMaterial> matList = new List<AssetPackMaterial>();
       
        string[] files = Directory.GetFiles(root, "*.mat", SearchOption.AllDirectories);
        
        foreach (string file in files)
        {
            string relPath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName, file);

            var mat = AssetDatabase.LoadAssetAtPath(relPath, typeof(Material)) as Material;
            if (mat != null)
            {
                try
                {
                    var matData = new AssetPackMaterial();
                    matData.Name = mat.name;
                    matData.Props = GetMaterialPropertySet(mat);

                    var shader = mat.shader;
                    if (shader != null)
                    {
                        matData.Shader = new AssetPackShaderRef
                        {
                            Name = shader.name
                        };

                        var keywordSpace = shader.keywordSpace;
                        if (keywordSpace != null)
                        {
                            var enabledKeywords = new List<string>();
                            foreach (var keyword in keywordSpace.keywords)
                            {   
                                if (mat.IsKeywordEnabled(keyword))
                                {
                                    enabledKeywords.Add(keyword.name);
                                }
                            }
                            if (enabledKeywords.Count > 0)
                            {
                                matData.Shader.EnabledKeywords = enabledKeywords.ToArray();
                            }
                        }
                    }

                    this.materialNameToMaterialMap[matData.Name] = matData;

                    matList.Add(matData);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

        }

        var matLib = new AssetPackMaterialLibrary()
        {
            Materials = matList
        };
        
        assetPackLibrary.MaterialLibrary = matLib;
    }

    private AssetPackMaterialPropertySet GetMaterialPropertySet(Material mat, AssetPackMaterial assetPackMaterial = null)
    {
        var set = new AssetPackMaterialPropertySet();

        // Texture properties.
        List<AssetPackMaterialTextureProperty> texProps = new List<AssetPackMaterialTextureProperty>();
        foreach (string propName in mat.GetPropertyNames(MaterialPropertyType.Texture))
        {
            var texProp = new AssetPackMaterialTextureProperty();
            texProp.Name = propName;

            int propNameId = Shader.PropertyToID(propName);
            var texture = mat.GetTexture(propNameId);
            if (texture != null)
            {
                // TODO: Do we need to reference in some other way?
                texProp.Value = new AssetPackTextureRef { Name = texture.name };
            }

            bool ignoreProperty = false;
            if (assetPackMaterial == null)
            {
                // This is the base material, so if the texture is not set, just ignore the entire property.
                // If this were a derived material, we'd want to see that the texture had been nulled out.
                if (texProp.Value == null)
                {
                    ignoreProperty = true;
                }
            }

            if (!ignoreProperty)
            {
                texProps.Add(texProp);
            }
        }
        set.Textures = texProps.ToArray();

        // Float properties.
        List<AssetPackMaterialFloatProperty> floatProps = new List<AssetPackMaterialFloatProperty>();
        foreach (string propName in mat.GetPropertyNames(MaterialPropertyType.Float))
        {
            var floatProp = new AssetPackMaterialFloatProperty();

            int propNameId = Shader.PropertyToID(propName);
            var value = mat.GetFloat(propNameId);

            floatProp.Name = propName;
            floatProp.Value = value;
            
            floatProps.Add(floatProp);
        }
        set.Floats = floatProps.ToArray();

        // Int properties.
        List<AssetPackMaterialIntProperty> intProps = new List<AssetPackMaterialIntProperty>();
        foreach (string propName in mat.GetPropertyNames(MaterialPropertyType.Int))
        {
            var intProp = new AssetPackMaterialIntProperty();

            int propNameId = Shader.PropertyToID(propName);
            var value = mat.GetInteger(propNameId);

            intProp.Name = propName;
            intProp.Value = value;
            
            intProps.Add(intProp);
        }
        set.Ints = intProps.ToArray();

        // Vector float properties.
        List<AssetPackMaterialVectorFloatProperty> vectorFloatProps = new List<AssetPackMaterialVectorFloatProperty>();
        foreach (string propName in mat.GetPropertyNames(MaterialPropertyType.Vector))
        {
            var vectorFloatProp = new AssetPackMaterialVectorFloatProperty();

            int propNameId = Shader.PropertyToID(propName);
            var value = mat.GetVector(propNameId);

            vectorFloatProp.Name = propName;
            vectorFloatProp.Value = new float[] { value.x, value.y, value.z, value.w };
            
            vectorFloatProps.Add(vectorFloatProp);
        }
        set.VectorFloats = vectorFloatProps.ToArray();

        if (assetPackMaterial != null)
        {
            return set.GetDiffSetFromBaseSet(assetPackMaterial.Props);
        }
        else
        {
            return set;
        }
    }


    private void ExportMeshes() 
    {
        string root = Path.Combine(Application.dataPath, SourceFolderInAssets);

        List<AssetPackMesh> meshList = new List<AssetPackMesh>();

        string[] files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
        
        foreach (string file in files)
        {
            string extension = Path.GetExtension(file).ToLower();
            // TODO: Support other model file types.
            if (!(extension == ".fbx" || extension == ".glb" || extension == "gltf"))
            {
                continue;
            }
            
            string relPath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName, file);

            // Get path relative to the assets source folder.
            string srcFolderRelativeFilePath = Path.GetRelativePath(Path.Combine("Assets", SourceFolderInAssets), relPath);
            string srcFolderRelativeFolder = Path.GetDirectoryName(srcFolderRelativeFilePath);

            bool useGltfFile = true;

            var go = AssetDatabase.LoadAssetAtPath(relPath, typeof(GameObject)) as GameObject;
            if (go != null)
            {
                if (HasSkinnedMesh(go))
                {
                    // GLTF was causing errors with skinned meshes.
                    useGltfFile = false;
                }
            }
            else
            {
                Debug.Log("FAILED TO LOAD: " + relPath);
            }

            if (useGltfFile)
            {
                string meshFilename = go.name + ".glb";
                string relMeshFilename = Path.Combine(srcFolderRelativeFolder, meshFilename);
                ExportToGltfAsync(new[] { go }, relMeshFilename);

                // Need to add separate entries for child meshes of this game object.
                List<string> allMeshNames = new List<string>();
                CollectAllMeshNames(go, outMeshNames: allMeshNames);
                if (allMeshNames.Count > 0)
                {
                    foreach (var meshName in allMeshNames)
                    {
                        // Add entry for each mesh indicating that it will be present in the given 
                        // output file.
                        var meshInfo = new AssetPackMesh
                        {
                            Name = meshName,
                            PackFile = relMeshFilename,
                            PackFileNode = meshName
                        };

                        meshList.Add(meshInfo);
                    }
                }
            }
            else
            {
                // Standalone file. 

                // Using FBX for skinned meshes. GLB seems to have errors.

                // TODO: Check for duplicate mesh names.
                var meshName = Path.GetFileNameWithoutExtension(file);
                var meshInfo = new AssetPackMesh
                {
                    Name = meshName,
                    PackFile = meshName + ".fbx",
                    PackFileNode = go.name
                };

                string outFile = Path.Combine(targetMeshFolder, meshInfo.PackFile);
                ExportModelOptions exportModelOptions = new ExportModelOptions
                {
                    ExportFormat = ExportFormat.Binary,
                    KeepInstances = false,
                    AnimateSkinnedMesh = true,
                    ModelAnimIncludeOption = Include.ModelAndAnim
                };
                ModelExporter.ExportObject(outFile, go, exportModelOptions);

                meshList.Add(meshInfo);
            }
        }

        var meshLib = new AssetPackMeshLibrary
        {
            Meshes = meshList
        };
        assetPackLibrary.MeshLibrary = meshLib;
    }


    private void CollectAllMeshNames(GameObject go, List<string> outMeshNames)
    {
        if (go == null)
        {
            return;
        }

        if (go.TryGetComponent<MeshFilter>(out var meshFilter))
        {
            if (meshFilter?.sharedMesh != null)
            {
                outMeshNames.Add(meshFilter.sharedMesh.name);
            }
        }

        int childCount = go.transform.childCount;
        for (int co = 0; co < childCount; co++)
        {
            var child = go.transform.GetChild(co).gameObject;
            CollectAllMeshNames(child, outMeshNames);
        }
    }

    private bool HasSkinnedMesh(GameObject go)
    {
        if (go.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
        {
            return true;
        }

        int childCount = go.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = go.transform.GetChild(i);
            if (HasSkinnedMesh(child.gameObject))
            {
                return true;
            }
        }

        return false;
    }


    async void ExportToGltfAsync(GameObject[] gameObjects, string exportFilename) 
    {
        var exportSettings = new ExportSettings {
            Format = GltfFormat.Binary,
            FileConflictResolution = FileConflictResolution.Overwrite
        };

        var gameObjectExportSettings = new GameObjectExportSettings 
        {
            OnlyActiveInHierarchy = false,
            DisabledComponents = true
        };

        var export = new GameObjectExport(exportSettings, gameObjectExportSettings);
        export.AddScene(gameObjects);

        
        string path = Path.Combine(targetMeshFolder, exportFilename);
        EnsureDirectoryExists(Path.GetDirectoryName(path));
        var success = await export.SaveToFileAndDispose(path);
        if(!success) {
            Debug.LogError("Error exporting GLTF");
        }
    }

    private void EnsureDirectoryExists(string dirName) 
    {
        if (!Directory.Exists(dirName))
        {
            EnsureDirectoryExists(Path.GetDirectoryName(dirName));

            Directory.CreateDirectory(dirName);
        }
    }

    private void ExportScenes()
    {
        string root = Path.Combine(Application.dataPath, SourceFolderInAssets);

        List<AssetPackScene> sceneList = new List<AssetPackScene>();

        string[] files = Directory.GetFiles(root, "*.prefab", SearchOption.AllDirectories);
        
        foreach (string file in files)
        {
            string relPath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName, file);

            // Get path relative to the assets source folder.
            string srcFolderRelativeFilePath = Path.GetRelativePath(Path.Combine("Assets", SourceFolderInAssets), relPath);
            string srcFolderRelativeFolder = Path.GetDirectoryName(srcFolderRelativeFilePath);

            var prefab = AssetDatabase.LoadAssetAtPath(relPath, typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                var scene = new AssetPackScene
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Folder = srcFolderRelativeFolder
                };

                this.prefabAssetPathToPrefabNameMap[relPath] = scene.Name;

                scene.Root = GetSceneNode(prefab);

                sceneList.Add(scene);
            }
            else
            {
                Debug.Log("FAILED TO LOAD: " + relPath);
            }
        }

        // Include the level scenes too (which will be added as external files since they tend to be bigger).
        ExportUnityLevelScenes(sceneList);

        var sceneLib = new AssetPackSceneLibrary
        {
            Scenes = sceneList
        };
        assetPackLibrary.SceneLibrary = sceneLib;
    }


    private void ExportUnityLevelScenes(List<AssetPackScene> outSceneList)
    {
        // TODO: Crawl scenes loading them and exporting them instead of just doing active scene as Demo.
        var sceneNode = ExportActiveScene("Demo");
        outSceneList.Add(sceneNode);
    }



    private AssetPackSceneNode GetSceneNode(GameObject go)
    {
        var t = go.transform;

        var node = new AssetPackSceneNode();
        node.Name = go.name;
        node.Position = AssetPackVector3.FromVector3IgnoreDefault(t.localPosition, Vector3.zero);
        node.Rotation = AssetPackQuaternion.FromQuaternionIgnoreIdentity(t.localRotation);
        node.Scale = AssetPackVector3.FromVector3IgnoreDefault(t.localScale, Vector3.one);

        if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Connected)
        {
            // Handle Prefab reference. 
            // TODO: Check for overridden values and pass those along in some fashion.
            //   See PrefabUtility.GetPropertyModifications(go);

            string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            string prefabName = Path.GetFileNameWithoutExtension(prefabAssetPath);
            
            var sceneRef = new AssetPackSceneRef
            {
                // TODO: Consider storing full asset path instead (would need to add that to the scene node as a source path perhaps)
                // TOOD: Might need to do multiple passes depending on how we have these point back and forth.
                Name = prefabName
            };
            node.SceneRef = sceneRef;

            // TODO: Look for added or modified child nodes.
        } 
        else
        {
            bool isSkinnedMesh = false;
            Transform rootBone = null;
            if (go.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                isSkinnedMesh = true;
                rootBone = skinnedMeshRenderer.rootBone;

                node.Mesh = new AssetPackMeshRef
                {
                    Name = skinnedMeshRenderer.sharedMesh.name
                };

                var matList = new List<Material>();
                skinnedMeshRenderer.GetSharedMaterials(matList);
                node.Materials = GetMaterialRefListForNodeMaterials(matList);
            }

            var childCount = t.childCount;
            if (childCount > 0)
            {
                node.ChildNodes = new List<AssetPackSceneNode>();
                for (int i = 0; i < childCount; i++)
                {
                    var childTransform = t.GetChild(i);

                    // TODO: Should we skip bone transforms or model attachments in different way?
                    // TODO: Should we have option to include disabled objects?
                  
                    var child = childTransform.gameObject;
                    if (child.activeSelf)
                    {
                        var childNode = GetSceneNode(child);
                        if (childNode != null)
                        {
                            node.ChildNodes.Add(childNode);
                        }
                    }
                }
            }

            if (go.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                if (meshFilter.sharedMesh != null)
                {
                    node.Mesh = new AssetPackMeshRef
                    {
                        Name = meshFilter.sharedMesh.name
                    };
                }
            }

            if (go.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                var matList = new List<Material>();
                meshRenderer.GetSharedMaterials(matList);
                node.Materials = GetMaterialRefListForNodeMaterials(matList);
            }

            if (go.TryGetComponent<MeshCollider>(out var meshCollider))
            {
                if (meshCollider.sharedMesh != null)
                {
                    node.Collider = new AssetPackMeshRef
                    {
                        Name = meshCollider.sharedMesh.name
                    };
                }
            }
        }

        return node;
    }


    private AssetPackMaterialRef[] GetMaterialRefListForNodeMaterials(List<Material> matList)
    {
        List<AssetPackMaterialRef> matRefList = new List<AssetPackMaterialRef>();

        if (matList.Count > 0)
        {
            List<AssetPackMaterialRef> matRefs = new List<AssetPackMaterialRef>();
            foreach (var mat in matList)
            {
                AssetPackMaterialRef matRef = null;
                if (mat != null)
                {
                    matRef = new AssetPackMaterialRef
                    {
                        Name = mat.name
                    };

                    AssetPackMaterial assetPackMat = null;
                    materialNameToMaterialMap.TryGetValue(matRef.Name, out assetPackMat);

                    matRef.Props = GetMaterialPropertySet(mat, assetPackMat);
                }
                matRefs.Add(matRef);
            }

            return matRefs.ToArray();
        }

        return null;
    }


    private AssetPackScene ExportActiveScene(string sceneName)
    {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
        var rootGameObjects = scene.GetRootGameObjects();

        var assetPackScene = new AssetPackScene();
        assetPackScene.Name = scene.name;
        
        var sceneRoot = new AssetPackSceneNode();
        sceneRoot.Name = scene.name;
        sceneRoot.ChildNodes = new List<AssetPackSceneNode>();

        foreach (var go in rootGameObjects)
        {
            var packSceneNode = GetSceneNode(go);
            sceneRoot.ChildNodes.Add(packSceneNode);
        }
        
        string targetSceneExternalFile = sceneName + ".json";
        SerializeObjectToJsonFile(sceneRoot, Path.Combine(targetSceneFolder, targetSceneExternalFile));

        var sceneNodeForLib = new AssetPackScene();
        sceneNodeForLib.Name = sceneName;
        sceneNodeForLib.IsLevel = true;
        sceneNodeForLib.ExternalFile = targetSceneExternalFile;
        return sceneNodeForLib;
    }
}