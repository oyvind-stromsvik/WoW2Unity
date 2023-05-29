using UnityEngine;
using System.Collections;

public class RandomizeHeights : MonoBehaviour
{

    Terrain terrain;
    TerrainData tData;

    int xRes;
    int yRes;

    float[,] heights;

    void Start() {
        terrain = transform.GetComponent<Terrain>();
        tData = terrain.terrainData;

        xRes = tData.heightmapResolution;
        yRes = tData.heightmapResolution;
        heights = tData.GetHeights(0, 0, xRes, yRes);
        Debug.Log(heights.Length);
    }

    void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 100, 25), "Wrinkle")) {
            randomizePoints(0.1f);
        }

        if (GUI.Button(new Rect(10, 40, 100, 25), "Reset")) {
            resetPoints();
        }
    }

    void randomizePoints(float strength) {
        heights = tData.GetHeights(0, 0, xRes, yRes);

        for (int y = 0; y < yRes; y++) {
            for (int x = 0; x < xRes; x++) {
                heights[x, y] = Random.Range(0.0f, strength) * 0.5f;
            }
        }

        tData.SetHeights(0, 0, heights);
    }

    void resetPoints() {
        var heights = tData.GetHeights(0, 0, xRes, yRes);
        for (int y = 0; y < yRes; y++) {
            for (int x = 0; x < xRes; x++) {
                heights[x, y] = 0;
            }
        }

        tData.SetHeights(0, 0, heights);
    }

    // Update is called once per frame
    void Update() {

    }
}