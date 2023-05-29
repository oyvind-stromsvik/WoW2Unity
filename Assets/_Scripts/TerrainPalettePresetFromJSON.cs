using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public class TerrainPalettePresetFromJSON : MonoBehaviour
{

    public TerrainPalette palette;
    public TextAsset materialJSON;

    [InspectorButton("OnUpdatePalettePreset", ButtonWidth = 120f)]
    public bool bUpdatePalettePreset;

    private List<TerrainLayer> layers = new List<TerrainLayer>();

    // Start is called before the first frame update
    void Start()
    {        
    }

    public void OnUpdatePalettePreset() {
        layers.Clear();
        JArray ja = JArray.Parse(materialJSON.text);
        
        foreach (JValue i in ja) {
            Debug.Log(i.Value);
            TerrainLayer tlayer = (TerrainLayer)AssetDatabase.LoadAssetAtPath("Assets/Terrain/TerrainLayers/" + i + ".terrainlayer", typeof(TerrainLayer));
            if (tlayer == null) {
                tlayer = new TerrainLayer();
                tlayer.diffuseTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Exports/textures/terrain/" + i + ".png", typeof(Texture2D));
                AssetDatabase.CreateAsset(tlayer, "Assets/Terrain/TerrainLayers/" + i + ".terrainlayer");
                layers.Add(tlayer);
            }
            else {
                layers.Add(tlayer);
            }            
        }
        palette.PaletteLayers = layers;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
