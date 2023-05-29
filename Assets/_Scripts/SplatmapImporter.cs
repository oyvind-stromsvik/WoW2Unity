using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.Experimental.TerrainAPI;
using System.Linq;

public class SplatmapImporter : MonoBehaviour
{
    public TextAsset exportDataJSON;
    public bool is4x4, importTerrainOnly, clearCache;
    [InspectorButton("OnParseExportData", ButtonWidth = 120f)]
    public bool bParseExportData;

    [InspectorButton("OnImportHeightmap4x4", ButtonWidth = 120f)]
    public bool bImportHeightmap;

    private ImportAdtWorld importADTWorld;
    private TextAsset matDataJSON, heightDataJSON, modelPlacementInfoCSV;
    
    private GameObject adtParent, terrainGameObject;
    private Terrain terrain;
    private TerrainData tData;
    private TerrainToolboxUtilitiesWow terrainToolboxUtilities;
    private Material terrainMaterial;
    private List<string> textureList = new List<string>();

    private string mapName, adtCoords;
    private int adtY, adtX;    

    private float[,] heights; // 2d array of height values
    private float min, max; // Min/max values for chunk Y axis
    private float terrainHeight; // Terrain height value

    /// <summary>
    /// - CreateTerrain needs to be moved into it's own script.
    /// - This 'master' script needs to parse pvpzone02_ExportData.json and get 3 filenames there
    /// - All references to matDataJSON.name must be updated because filenames all changed to 
    /// matdata_    7 = 3, 8 = 4
    /// adt_31....
    /// </summary>
    /// 

    private void ParseExportData() {
        // Need to pull all 3 filenames, ADT_X, ADT_Y, and ZoneName        
        string[,] deserialized = JsonConvert.DeserializeObject<string[,]>(exportDataJSON.text);
        importADTWorld = gameObject.GetComponent<ImportAdtWorld>();
        if (clearCache) { importADTWorld.ClearCache(); }

        for (int i = 0; i < deserialized.GetLength(0); i++) {
            mapName = deserialized[i, 0];           // Mapname
            adtY = Int32.Parse(deserialized[i, 1]); // ADT Y coord
            adtX = Int32.Parse(deserialized[i, 2]); // ADT X coord
            adtCoords = adtY + "_" + adtX;
            string path = deserialized[i, 3]; // Path to 
            int matchIndex = path.IndexOf("Assets");
            path = path.Substring(matchIndex, path.Length - matchIndex);
            modelPlacementInfoCSV = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            path = deserialized[i, 4];
            path = path.Substring(matchIndex, path.Length - matchIndex);
            heightDataJSON = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            path = deserialized[i, 5];            
            path = path.Substring(matchIndex, path.Length - matchIndex);
            matDataJSON = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));

            // Create the parent gameobject
            adtParent = new GameObject("ADT_" + adtCoords);
            adtParent.transform.position = Vector3.zero;
            adtParent.transform.rotation = Quaternion.identity;

            // Now we'll call all the functions below per mapchunk
            CreateTerrain();
            ImportSplatMaps();
            if (is4x4) { ImportHeightmap4x4(); } else { ImportHeightmap(); }
            SetPosition();

            if (modelPlacementInfoCSV != null) { // The ModelPlacementInfo.csv could be missing                
                importADTWorld.csvFiles = new TextAsset[1];
                importADTWorld.csvFiles[0] = modelPlacementInfoCSV;                
                if (!importTerrainOnly) {
                    importADTWorld.RunImport(adtParent);
                }
            }            
        }
    }

    private void CreateTerrain() {        
        terrainGameObject = null;
        tData = (TerrainData)AssetDatabase.LoadAssetAtPath("Assets/Terrain/TerrainData/TerrainData_" + adtCoords + ".asset", typeof(TerrainData));
        int heightResolution = is4x4 ? 1025 : 257;
        int splatResolution = is4x4 ? 4096 : 1024;
        if (tData == null) {
            tData = new TerrainData {
                heightmapResolution = heightResolution,
                baseMapResolution = splatResolution,
                size = new Vector3(533.3333f, 600f, 533.3333f) // 600 will be changed later to max-min (terrainHeight var)
            };
            tData.SetDetailResolution(1024, 16);
            AssetDatabase.CreateAsset(tData, "Assets/Terrain/TerrainData/TerrainData_" + adtCoords + ".asset");
            terrainGameObject = Terrain.CreateTerrainGameObject(tData);
            terrainGameObject.name = "Terrain_" + adtCoords;
            terrain = terrainGameObject.GetComponent<Terrain>();
            terrain.transform.parent = adtParent.transform;
        }
        else {            
            tData.heightmapResolution = heightResolution;
            tData.baseMapResolution = splatResolution;
            tData.size = new Vector3(533.3333f, 600f, 533.3333f); // 600 will be changed later to max-min (terrainHeight var)
            tData.SetDetailResolution(1024, 16);
            terrainGameObject = Terrain.CreateTerrainGameObject(tData);
            terrainGameObject.name = "Terrain_" + adtCoords;
            terrain = terrainGameObject.GetComponent<Terrain>();
            terrain.transform.parent = adtParent.transform;
        }
        
        terrainMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/_Materials/Material_WowTerrain.mat", typeof(Material));
        if (terrainMaterial == null) { Debug.LogError("Could not find terrain material at: Assets/_Materials/Material_WowTerrain.mat"); }
        terrain.materialTemplate = terrainMaterial;
    }

    private void SetPosition() { // ImportHeightmap() must be ran first to popoulate 'min'        
        float x = adtY;
        float y = adtX;
        x = (x - 32) * 533.33333333f;
        y = (y - 31) * -533.33333333f;
        terrainGameObject.transform.position = new Vector3(x, min, y);
    }

    private void ImportSplatMaps() {
        textureList.Clear();
        terrain = terrainGameObject.transform.GetComponent<Terrain>();
        tData = terrain.terrainData;
        //heights = tData.GetHeights(0, 0, xRes, yRes);

        //string splatmapFilename = matDataJSON.name.Substring(7, matDataJSON.name.Length - 7);
        string splatmapFilename = "splatmap_" + mapName + "_" + adtCoords + "_"; //+ i + "*.tga"        
        string[] splatmapguids = AssetDatabase.FindAssets(splatmapFilename, new[] {"Assets/Exports/splatmaps"});
        List<Texture2D> splatmaps = new List<Texture2D>();

        // Import JSON Data
        string filename = "";
        JArray ja = JArray.Parse(matDataJSON.text);
        foreach (JObject i in ja) {
            string textureID = i["id"].ToString();
            textureList.Add(textureID);
            filename += textureID;
        }
                
        /*
        JObject jo = JObject.Parse(matDataJSON.text);
        JObject joSplatmapData = jo["splatmapData"] as JObject;
        string filename = "";
        foreach (var x in joSplatmapData) {
            string textureID = x.Value.ToString();
            textureList.Add(textureID);
            filename += textureID;
        }*/

        TerrainPalette terrainPaletteAsset = (TerrainPalette)AssetDatabase.LoadAssetAtPath("Assets/Terrain/TerrainPalettePresets/" + filename + ".asset", typeof(TerrainPalette));
        if (terrainPaletteAsset == null) { // No Palette exists, create a new one
            Debug.Log("No Palette exists, create a new one");
            terrainPaletteAsset = ScriptableObject.CreateInstance<TerrainPalette>();
            AssetDatabase.CreateAsset(terrainPaletteAsset, "Assets/Terrain/TerrainPalettePresets/" + filename + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            foreach (string i in textureList) {
                TerrainLayer tlayer = (TerrainLayer)AssetDatabase.LoadAssetAtPath("Assets/Terrain/TerrainLayers/" + i + ".terrainlayer", typeof(TerrainLayer));
                if (tlayer == null) {
                    tlayer = new TerrainLayer();
                    tlayer.diffuseTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Exports/textures/terrain/" + i + ".png", typeof(Texture2D));
                    AssetDatabase.CreateAsset(tlayer, "Assets/Terrain/TerrainLayers/" + i + ".terrainlayer");
                    terrainPaletteAsset.PaletteLayers.Add(tlayer);
                }
                else {
                    terrainPaletteAsset.PaletteLayers.Add(tlayer);
                }
            }
        }
        else if (terrainPaletteAsset.PaletteLayers.Count() < textureList.Count()) { // Palette exists, but size is incorrect
            Debug.Log("Palette exists, but size is incorrect");
            terrainPaletteAsset.PaletteLayers.Clear();
            foreach (string i in textureList) {
                TerrainLayer tlayer = (TerrainLayer)AssetDatabase.LoadAssetAtPath("Assets/Terrain/TerrainLayers/" + i + ".terrainlayer", typeof(TerrainLayer));
                if (tlayer == null) {
                    tlayer = new TerrainLayer();
                    tlayer.diffuseTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Exports/textures/terrain/" + i + ".png", typeof(Texture2D));
                    AssetDatabase.CreateAsset(tlayer, "Assets/Terrain/TerrainLayers/" + i + ".terrainlayer");
                    terrainPaletteAsset.PaletteLayers.Add(tlayer);
                }
                else {
                    terrainPaletteAsset.PaletteLayers.Add(tlayer);
                }
            }
        }
        else { // Palette already exists, size is correct
            // This loop is just making sure all layers are present and creating them if they went missing.
            Debug.Log("Palette already exists, size is correct");
            for (int i = 0; i < terrainPaletteAsset.PaletteLayers.Count(); i++) {                
                if (terrainPaletteAsset.PaletteLayers[i] == null) {
                    TerrainLayer tlayer = (TerrainLayer)AssetDatabase.LoadAssetAtPath("Assets/Terrain/TerrainLayers/" + textureList[i] + ".terrainlayer", typeof(TerrainLayer));
                    if (tlayer == null) {
                        tlayer = new TerrainLayer();
                        tlayer.diffuseTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Exports/textures/terrain/" + textureList[i] + ".png", typeof(Texture2D));
                        AssetDatabase.CreateAsset(tlayer, "Assets/Terrain/TerrainLayers/" + textureList[i] + ".terrainlayer");
                        terrainPaletteAsset.PaletteLayers[i] = tlayer;
                    }
                    else {
                        terrainPaletteAsset.PaletteLayers[i] = tlayer;
                    }
                }
            }
        }        
        
        for (int i = 0; i < splatmapguids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath(splatmapguids[i]);
            splatmaps.Add((Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)));
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        // Now find or create the matching PalettePreset object for this specific texture list.
        terrainToolboxUtilities = new TerrainToolboxUtilitiesWow();
        terrainToolboxUtilities.m_Splatmaps = splatmaps;
        terrainToolboxUtilities.m_SelectedLayerPalette = terrainPaletteAsset;
        terrainToolboxUtilities.LoadPalette();
        terrainToolboxUtilities.AddLayersToTerrain(terrain);
        //terrain.Flush();        
        terrainToolboxUtilities.ExportSplatmapsToTerrain(terrain);
        
    }

    public void ImportHeightmap4x4() {
        terrainGameObject = gameObject;
        terrain = terrainGameObject.transform.GetComponent<Terrain>();
        tData = terrain.terrainData;
        min = 0f; max = 0f;
        string[,] deserialized = JsonConvert.DeserializeObject<string[,]>(exportDataJSON.text); //heightDataJSON.text
        float[,] jsonHeights = new float[1025, 1025];
        for (int i = 0; i < deserialized.GetLength(0); i++) {
            for (int j = 0; j < deserialized.GetLength(1); j++) {
                string sValue = deserialized[i, j];
                if (sValue == null) { sValue = "0"; }
                float value = float.Parse(sValue);
                if (value > max) { max = value; }
                if (value < min) { min = value; }
                jsonHeights[i, j] = value;
            }
        }
        terrainHeight = max - min;
        tData.size = new Vector3(tData.size.x, terrainHeight, tData.size.z);
        //Debug.Log(max + ", " + min + " = " + (max-min));

        heights = new float[1025, 1025];
        for (int i = 0; i < jsonHeights.GetLength(0); i++) {
            for (int j = 0; j < jsonHeights.GetLength(1); j++) {
                float value = jsonHeights[i, j];
                heights[i, j] = (value - min) / (max - min);
            }
        }
        // I don't know why the height data is rotated 180°, but fix it here.
        //heights = RotateMatrix(heights, 1025);
        //heights = RotateMatrix(heights, 1025);
        tData.SetHeights(0, 0, heights);
    }

    public void ImportHeightmap() {
        terrain = terrainGameObject.transform.GetComponent<Terrain>();
        tData = terrain.terrainData;
        min = 0f; max = 0f;
        string[,] deserialized = JsonConvert.DeserializeObject<string[,]>(heightDataJSON.text);
        float[,] jsonHeights = new float[257, 257];
        for (int i = 0; i < deserialized.GetLength(0); i++) {
            for (int j = 0; j < deserialized.GetLength(1); j++) {
                string sValue = deserialized[i, j]; 
                if (sValue == null) {
                    sValue = "0";
                }
                float value = float.Parse(sValue);
                if (value > max) { max = value; }
                if (value < min) { min = value; }
                jsonHeights[i, j] = value;
            }
        }
        terrainHeight = max - min;
        tData.size = new Vector3(tData.size.x, terrainHeight, tData.size.z);
        //Debug.Log(max + ", " + min + " = " + (max-min));

        heights = new float[257, 257];
        for (int i = 0; i < jsonHeights.GetLength(0); i++) {
            for (int j = 0; j < jsonHeights.GetLength(1); j++) {                
                float value = jsonHeights[i, j];
                heights[i, j] = (value - min) / (max - min);
            }
        }
        // I don't know why the height data is rotated 180°, but fix it here.
        heights = RotateMatrix(heights, 257);
        heights = RotateMatrix(heights, 257);
        tData.SetHeights(0, 0, heights);        
    }

    public void OnParseExportData() { ParseExportData(); }
    public void OnImportHeightmap4x4() { if (is4x4) { ImportHeightmap4x4(); } else { ImportHeightmap(); } }

    public float[,] RotateMatrix(float[,] matrix, int n) {
        float[,] ret = new float[n, n];

        for (int i = 0; i < n; ++i) {
            for (int j = 0; j < n; ++j) {
                ret[i, j] = matrix[n - j - 1, i];
            }
        }
        return ret;
    }
}