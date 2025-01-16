using System;
using System.Collections.Generic;
using System.IO;
using GLTFast.Export;
using UnityEngine;


/// <summary>
/// Collects multiple objects to export into mesh collections.
/// </summary>
public class MeshExportAccumulator
{
	public string TargetFolder { get; set; }
	public string FilePrefix { get; set; } = "meshes_";
	public int MaxMeshes { get; set; } = 500;

	private int _nextIndex = 1;
	private List<GameObject> _goList = new List<GameObject>();

	public string AddGameObject(GameObject go)
	{
		_goList.Add(go);

		string file = Path.Combine(TargetFolder, FilePrefix) + _nextIndex + ".glb";
		if (_goList.Count >= MaxMeshes)
		{
			Flush();
		}

		return file;
	}


	public void Flush()
	{
		if (_goList.Count == 0)
		{
			return;
		}

		string file = Path.Combine(TargetFolder, FilePrefix) + _nextIndex + ".glb";
		ExportToGltf(_goList.ToArray(), file);

		_goList = new List<GameObject>();
		_nextIndex++;
	}

	void ExportToGltf(GameObject[] gameObjects, string exportFilename) 
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

        EnsureDirectoryExists(Path.GetDirectoryName(exportFilename));
		var task = export.SaveToFileAndDispose(exportFilename);

		// TODO: Decide how we want to check and monitor the result.
		// task.Wait();
        // var success = task.Result;
        // if(!success) {
        //     Debug.LogError("Error exporting GLTF");
        // }
    }

	private void EnsureDirectoryExists(string dirName) 
    {
        if (!Directory.Exists(dirName))
        {
            EnsureDirectoryExists(Path.GetDirectoryName(dirName));

            Directory.CreateDirectory(dirName);
        }
    }
}