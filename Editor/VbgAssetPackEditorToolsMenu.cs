using UnityEditor;
using UnityEngine;
using System.IO;
using System.Drawing.Printing;
using System.Collections.Generic;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.Linq.Expressions;
using UnityEditor.SceneManagement;
using System.Runtime.InteropServices;


#if UNITY_EDITOR

// Install package by name under PackageManager: 
//  com.unity.cloud.gltfast
//  com.unity.nuget.newtonsoft-json
using GLTFast.Export;


public class VbgAssetPackEditorToolsMenu
{
    [MenuItem("VBG/Log Active Object Info")]
    private static async void PrintActiveObjectInfo()
    {
        var go = UnityEditor.Selection.activeGameObject;
        if (go != null)
        {
            Debug.Log("Name: " + go.name);
            Debug.Log("PrefabInstanceStatus: " + PrefabUtility.GetPrefabInstanceStatus(go).ToString());
            Debug.Log("NearestPrefabInstanceRoot: " + PrefabUtility.GetNearestPrefabInstanceRoot(go)?.name ?? "NONE");
            Debug.Log("IsOutermostPrefabInstanceRoot: " + PrefabUtility.IsOutermostPrefabInstanceRoot(go).ToString());
            Debug.Log("IsAddedGameObjectOverride: " + PrefabUtility.IsAddedGameObjectOverride(go));

            List<ObjectOverride> overrides = PrefabUtility.GetObjectOverrides(go, false);
            foreach (ObjectOverride o in overrides)
            {
            }

            var addedComponents = PrefabUtility.GetAddedComponents(go);
            var addedGameObjects = PrefabUtility.GetAddedGameObjects(go);
            Debug.Log("AddedComponents: " + addedComponents.Count + ", AddedGameObjects:" + addedGameObjects.Count + ", Overrides: " + overrides.Count);

            //if (PrefabUtility.IsOutermostPrefabInstanceRoot(go))
            {
                var prefabParent = EditorUtility.GetPrefabParent(go);
                string prefabPath = AssetDatabase.GetAssetPath(prefabParent);
                Debug.Log("Prefab Path: " + prefabPath);

                var o = PrefabUtility.GetPrefabAssetType(go);
                Debug.Log("PrefabAssetType: " + o);

                var sourceObject = PrefabUtility.GetCorrespondingObjectFromSource(go);
                string sourceObjectPath = AssetDatabase.GetAssetPath(sourceObject);
                Debug.Log("CorrespondingSource: " + sourceObjectPath);

                var sourceObjectPrefabAssetType = PrefabUtility.GetPrefabAssetType(sourceObject);
                Debug.Log("sourceObjectPrefabAssetType: " + sourceObjectPrefabAssetType);
            }
        }
    }

}

#endif