using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

public class ImportAdtWorld : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector]
    public TextAsset[] csvFiles;
    private float maxSize;
    private float mapSize;
    private float adtSize;
    private bool adtCsv, wmoCsv;
    private List<string> missingModelsList = new List<string>();
    private List<string> importedModels = new List<string>();

    // Start is called before the first frame update
    void Start() { }

    public void RunImport(GameObject parent) {
        maxSize = 51200f / 3f;
        mapSize = maxSize * 2f;
        adtSize = mapSize / 64f;
        foreach (TextAsset i in csvFiles) {
            ParseCsvFile(AssetDatabase.GetAssetPath(i), parent);
        }
    }

    public void ClearCache() {
        missingModelsList.Clear();
        importedModels.Clear();
    }

    // Update is called once per frame
    void Update() { }

    private void ParseCsvFile(string csvPath, GameObject parentObject = null) {
        string filename = System.IO.Path.GetFileName(csvPath);
        string csvDir = System.IO.Path.GetDirectoryName(csvPath);        
        //string csvPath = AssetDatabase.GetAssetPath(file);
        //Debug.Log("Starting: " + csvPath);
        string fileData = System.IO.File.ReadAllText(csvPath);
        string[] lines = fileData.Split("\n"[0]);
        string[] lineData = (lines[0].Trim()).Split(";"[0]);
        string adtName = filename.Replace("_ModelPlacementInformation.csv", "");

        Transform parent = null, wmoParent = null;
        if (adtName.Contains("adt_") && parentObject == null) {
            parent = GameObject.Find(adtName).transform;
            GameObject wmoParentGO = new GameObject();
            wmoParentGO.name = "WMOs";
            wmoParent = wmoParentGO.transform;
            wmoParent.parent = parent;
            wmoParent.SetSiblingIndex(0);
            wmoParent.transform.localPosition = Vector3.zero;
            wmoParent.transform.localEulerAngles = Vector3.zero;
        }
        else {            
            parent = parentObject.transform;
            GameObject wmoParentGO = new GameObject();
            wmoParentGO.name = "WMOs";
            wmoParent = wmoParentGO.transform;
            wmoParent.parent = parent;
            wmoParent.SetSiblingIndex(0);
            wmoParent.transform.localPosition = Vector3.zero;
            wmoParent.transform.localEulerAngles = Vector3.zero;
        }

        if (lineData[9] == "Type") {
            adtCsv = true; wmoCsv = false;
        } else if (lineData[9] == "DoodadSet") {
            wmoCsv = true; adtCsv = false;
        }

        GameObject doodadParentGO = new GameObject();
        doodadParentGO.name = "Doodads";
        Transform doodadParent = doodadParentGO.transform;
        doodadParent.parent = parent;
        doodadParent.SetSiblingIndex(0);
        doodadParent.transform.localPosition = Vector3.zero;
        doodadParent.transform.localEulerAngles = Vector3.zero;

        for (int i = 1; i < lines.Length; i++) {
            // Temp Vars
            float posX = 0f, posY = 0f, posZ = 0f, rotW = 0f, rotX = 0f, rotY = 0f, rotZ = 0f, scale = 1f;
            int modelID = 0;
            string type = "unassigned";
            // Read a line from the CSV file
            lineData = (lines[i].Trim()).Split(";"[0]);
            // Instantiate Gameobject
            string objPath = @lineData[0]; //path from csv
            objPath = System.IO.Path.Combine(@"../" + objPath);
            string objPathFull = System.IO.Path.GetFullPath(csvDir + objPath);
            objPath = "Assets" + objPathFull.Substring(Application.dataPath.Length);

            GameObject iPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(objPath, typeof(GameObject));
            if (iPrefab == null) {
                missingModelsList.Add(objPath);
                Debug.LogError("Could not find model: " + objPath);
            }            

            GameObject iGO = GameObject.Instantiate(iPrefab);            
            WowModel wowModel = iGO.AddComponent(typeof(WowModel)) as WowModel;            
            Transform iTransform = iGO.transform;

            // Populate local vars from CSV file
            if (adtCsv == true) {
                posX = float.Parse(lineData[1], CultureInfo.InvariantCulture.NumberFormat); // x pos
                posY = float.Parse(lineData[2], CultureInfo.InvariantCulture.NumberFormat); // y pos
                posZ = float.Parse(lineData[3], CultureInfo.InvariantCulture.NumberFormat); // z pos
                rotX = float.Parse(lineData[4], CultureInfo.InvariantCulture.NumberFormat); // x rot
                rotY = float.Parse(lineData[5], CultureInfo.InvariantCulture.NumberFormat); // y rot                
                rotZ = float.Parse(lineData[6], CultureInfo.InvariantCulture.NumberFormat); // z rot
                scale = float.Parse(lineData[7], CultureInfo.InvariantCulture.NumberFormat); // scale
                modelID = int.Parse(lineData[8], CultureInfo.InvariantCulture.NumberFormat); // Model ID
                type = lineData[9]; // Type of file
            }
            else if (wmoCsv == true) {
                posX = float.Parse(lineData[1], CultureInfo.InvariantCulture.NumberFormat); // x pos
                posY = float.Parse(lineData[2], CultureInfo.InvariantCulture.NumberFormat); // y pos
                posZ = float.Parse(lineData[3], CultureInfo.InvariantCulture.NumberFormat); // z pos
                rotW = float.Parse(lineData[4], CultureInfo.InvariantCulture.NumberFormat); // w rot
                rotX = float.Parse(lineData[5], CultureInfo.InvariantCulture.NumberFormat); // x rot
                rotY = float.Parse(lineData[6], CultureInfo.InvariantCulture.NumberFormat); // y rot
                rotZ = float.Parse(lineData[7], CultureInfo.InvariantCulture.NumberFormat); // z rot
                scale = float.Parse(lineData[8], CultureInfo.InvariantCulture.NumberFormat); // scale
                type = lineData[9]; // Type of file
            }
            wowModel.modelID = modelID;
            wowModel.type = type;

            // Set Position & Rotation
            if (type == "wmo") {
                iTransform.parent = wmoParent;
                iTransform.localPosition = new Vector3((maxSize - posX) * -1f, posY, maxSize - posZ);
                Vector3 ang = new Vector3(rotX / Mathf.Rad2Deg, (rotY + 90f) / Mathf.Rad2Deg, rotZ / Mathf.Rad2Deg);
                Vector3 ang2 = new Vector3(Mathf.Rad2Deg * ang.x, Mathf.Rad2Deg * -ang.y, Mathf.Rad2Deg * ang.z);
                iTransform.localEulerAngles = ang2;
                iTransform.localScale = new Vector3(scale, scale, scale);
            }
            else if (type == "m2") {
                iTransform.parent = doodadParent;
                iTransform.localPosition = new Vector3((maxSize - posX) * -1f, posY, maxSize - posZ);
                Vector3 ang = new Vector3(rotX / Mathf.Rad2Deg, (rotY + 90f) / Mathf.Rad2Deg, rotZ / Mathf.Rad2Deg);
                Vector3 ang2 = new Vector3(Mathf.Rad2Deg * ang.x, Mathf.Rad2Deg * -ang.y, Mathf.Rad2Deg * ang.z);
                iTransform.localEulerAngles = ang2;
                iTransform.localScale = new Vector3(scale, scale, scale);
            }
            else {
                iTransform.parent = doodadParent;
                iTransform.localPosition = new Vector3(-posX, posZ, -posY);
                iTransform.localRotation = new Quaternion(rotX, -rotZ, rotY, rotW);
                iTransform.localScale = new Vector3(scale, scale, scale);
            }

            // Save a unique model name so we can check for duplicates
            string uniqueModelName = iGO.name + iTransform.localPosition.ToString() + iTransform.localRotation.ToString();
            if (importedModels.Contains(uniqueModelName)) {
                Debug.Log("Found duplicate: " + uniqueModelName);
                DestroyImmediate(iGO);
            }
            else {
                importedModels.Add(uniqueModelName);

                // Define vars for materials and their paths
                string materialPath = @lineData[0]; //path from csv
                materialPath = materialPath.Substring(0, materialPath.Length - 4) + ".mtl";
                materialPath = System.IO.Path.Combine(@"../" + materialPath);
                string materialPathFull = System.IO.Path.GetFullPath(csvDir + materialPath);
                materialPath = "Assets" + materialPathFull.Substring(Application.dataPath.Length);
                string materialFileName = System.IO.Path.GetFileName(materialPath);
                string materialPathOnly = materialPathFull.Replace(materialFileName, "");
                //Debug.Log("Material pathfull: " + materialPathFull);
                
                // Transfer values from MTL file into list
                List<string> materialID = new List<string>();
                List<float> illumValue = new List<float>();
                List<string> texturePath = new List<string>();

                // Apply Texture To Material
                string mtlData = System.IO.File.ReadAllText(materialPathFull);
                string[] mtlLines = mtlData.Split("\n"[0]);
                for (int j = 0; j < mtlLines.Length; j += 3) {
                    //int.TryParse(mtlLines[j].Substring(7), out int matid);
                    //materialID.Add(matid);
                    materialID.Add(mtlLines[j].Substring(7));
                }
                for (int j = 1; j < mtlLines.Length; j += 3) {
                    int.TryParse(mtlLines[j].Substring(6), out int illum);
                    illumValue.Add(illum);
                }
                for (int j = 2; j < mtlLines.Length; j += 3) {
                    // Now it's going every 3rd line in the mtl file to pull texture filename and path
                    string texturePathCurrent = mtlLines[j];
                    texturePathCurrent = texturePathCurrent.Replace("map_Kd ", "");
                    string texturePathFull = System.IO.Path.GetFullPath(materialPathOnly + @texturePathCurrent);
                    //Debug.Log("MaterialPathFull: " + materialPathOnly + " , " + texturePathCurrent);
                    texturePathCurrent = "Assets" + texturePathFull.Substring(Application.dataPath.Length);
                    texturePathCurrent = texturePathCurrent.Substring(0, texturePathCurrent.Length - 4) + ".png";
                    texturePath.Add(texturePathCurrent);
                }

                foreach (Transform child in iTransform) {
                    // Set Material
                    MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                    Material material = meshRenderer.sharedMaterial; // Need to get all materials?
                    string materialName = material.name;
                    materialName = materialName.Replace(" (Instance)", "");
                    
                    //matint = materialID[0];
                    
                    //materialName = materialID[0].ToString();
                    
                    Material dbMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material));
                    if (dbMat == null) {
                        // Create a material file
                        Material newMaterial = new Material(material);
                        AssetDatabase.CreateAsset(newMaterial, "Assets/Materials/" + materialName + ".mat");

                        dbMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material));
                        // Apply Texture to material
                        Texture texture = (Texture)AssetDatabase.LoadAssetAtPath(texturePath[materialID.IndexOf(materialName)], typeof(Texture));
                        //Debug.Log("Texture path: " + texturePath[materialID.IndexOf(matint)]);

                        if (texture == null) {
                            Debug.LogError("Could not find texture at: " + texturePath[materialID.IndexOf(materialName)]);
                        }
                        else {
                            dbMat.SetTexture("_MainTex", texture);
                        }
                    }
                    meshRenderer.material = dbMat;

                }

                // Final check
                if (type == "wmo") {
                    string mpiPath = objPath.Replace(".obj", "_ModelPlacementInformation.csv");
                    if (System.IO.File.Exists(mpiPath)) {
                        //Debug.Log("Found mpi file at: " + mpiPath);
                        StartCoroutine(StartParse(mpiPath, 1f, iGO));
                    }
                }
                else if (type == "m2") {
                    // m2 doodad
                }
                else {
                    //wmo csv

                }
            }
        }
        
        foreach (string i in missingModelsList) {
            Debug.LogError("Missing Model: " + i);
        }
    }

    public IEnumerator StartParse(string path, float delay, GameObject parentObject) {
        yield return new WaitForSeconds(delay);
        ParseCsvFile(path, parentObject);
    }
   
#endif
}
