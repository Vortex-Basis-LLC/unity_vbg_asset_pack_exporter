using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ExportAssetPackEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private TextField textFieldTargetExportFolder;
    private TextField textFieldAssetFolder;


    [MenuItem("VBG/Export Asset Pack")]
    public static void ShowExample()
    {
        ExportAssetPackEditorWindow wnd = GetWindow<ExportAssetPackEditorWindow>();
        wnd.titleContent = new GUIContent("VBG Export Asset Pack");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        var exportButton = root.Query<Button>("export_button").First();
        exportButton.RegisterCallback<ClickEvent>(HandleExportClick);

        textFieldTargetExportFolder = root.Query<TextField>("target_export_folder").First();
        textFieldAssetFolder = root.Query<TextField>("asset_folder").First();

        // Default to the currently selected asset folder for the source folder.
        if (String.IsNullOrWhiteSpace(textFieldAssetFolder.text) && TryGetActiveFolderPath(out var activeFolderPath))
        {
            if (activeFolderPath.StartsWith("Assets/"))
            {
                activeFolderPath = activeFolderPath.Substring("Assets/".Length);
            }
            textFieldAssetFolder.value = activeFolderPath;
        }
    }

    private static bool TryGetActiveFolderPath(out string activeFolderPath)
	{
		var tryGetActiveFolderPathMethod = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

		object[] args = new object[] { null };
		bool found = (bool)tryGetActiveFolderPathMethod.Invoke(null, args );
		activeFolderPath = (string)args[0];

		return found;
	}

    private void HandleExportClick(ClickEvent evt)
    {
        string targetExportFolder = textFieldTargetExportFolder.text;
        string assetFolder = textFieldAssetFolder.text;

        if (String.IsNullOrWhiteSpace(targetExportFolder) || !Path.IsPathFullyQualified(targetExportFolder))
        {
            Debug.LogError("Target Export Folder should be fully qualified: " + targetExportFolder);
            return;
        }

        if (String.IsNullOrWhiteSpace(targetExportFolder) || !Directory.Exists(targetExportFolder))
        {
            Debug.LogError("Target Export Folder not found: " + targetExportFolder);
            return;
        }

        string fullAssetFolderPath = Path.Combine(Application.dataPath, assetFolder);
        if (String.IsNullOrWhiteSpace(assetFolder) || !Directory.Exists(fullAssetFolderPath))
        {
            Debug.LogError("Asset Folder not found: " + fullAssetFolderPath);
            return;
        }

        AssetPackExporter exporter = new AssetPackExporter
        {
            TargetFolder = targetExportFolder,
            SourceFolderInAssets = assetFolder
        };

        exporter.ExportAssetPack();
    }
}
