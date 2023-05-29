using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Presets;

public class SplatmapImporter : AssetPostprocessor
{
    void OnPreprocessAsset() {
        // Make sure we are applying presets the first time an asset is imported (no meta exists).
        System.Type type = assetImporter.GetType();
        if (assetImporter.importSettingsMissing && type.Name == "TextureImporter") {
            TextureImporter importer = (TextureImporter)assetImporter;
            if (importer != null) {
                // Get the current imported asset folder.
                var path = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(path)) {
                    // Find all Preset assets in this folder.
                    var presetGuids = AssetDatabase.FindAssets("t:Preset", new[] { path });
                    foreach (var presetGuid in presetGuids) {
                        // Make sure we are not testing Presets in a subfolder.
                        string presetPath = AssetDatabase.GUIDToAssetPath(presetGuid);
                        if (Path.GetDirectoryName(presetPath) == path) {
                            // Load the Preset and try to apply it to the importer.
                            var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                            if (preset.ApplyTo(importer))
                                return;
                        }
                    }
                }
            }
        }
    }
}
