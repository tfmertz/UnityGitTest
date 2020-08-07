using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Holds the API for the voxel creation editor.
/// </summary>
public class CreationEditor : MonoBehaviour
{
    Creation currentCreation;
    List<Voxel> currentVoxels;

    GameObject grid;
    Grid gridScript;

    public void CreateNew(string name)
    {
        currentCreation = new Creation
        {
            name = name,
        };

        currentVoxels = new List<Voxel>();
        SetupGrid();
    }

    public void Undo()
    {
        gridScript.Undo();
    }

    public void Load()
    {
        if (File.Exists($"{Application.persistentDataPath}/creation.json"))
        {
            string fileData = File.ReadAllText($"{Application.persistentDataPath}/creation.json");
            currentCreation = JsonUtility.FromJson<Creation>(fileData);
            SetupGrid();
            gridScript.Load(currentCreation);
        } else
        {
            Debug.Log("No file");
        }
    }

    public void Save()
    {
        if (currentCreation == null)
        {
            Debug.Log("No creation to save.");
            return;
        }
        // TODO remove possible duplicates, make sure numbers are int for positions


        // Save our voxel data into the currentcreation
        gridScript.Save(currentCreation);

        string saveData = JsonUtility.ToJson(currentCreation);
        Debug.Log(saveData);

        // Save it into a file
        File.WriteAllText($"{Application.persistentDataPath}/creation.json", saveData);
    }

    /// <summary>
    /// Cleans up the grid and current voxel creation
    /// </summary>
    public void Exit()
    {
        if (grid != null)
        {
            Destroy(grid);
            gridScript = null;
        }
    }

    // Creates the Grid and CreateVoxel objects and scripts
    void SetupGrid()
    {
        // Clean any existing grids
        Exit();

        // Get needed materials
        Material gridMat = Resources.Load<Material>("Grid");
        Material test = Resources.Load<Material>("Test");
        Material preview = Resources.Load<Material>("Preview");

        // Create grid and voxel creator
        GameObject voxelCreator = new GameObject("VoxelCreator");
        CreateVoxel voxelScript = voxelCreator.AddComponent<CreateVoxel>();
        voxelScript.mat = test;

        grid = new GameObject("Grid");
        gridScript = grid.AddComponent<Grid>();
        gridScript.mat = gridMat;
        gridScript.CreateVoxel = voxelScript;

        // Set up hover preview
        GameObject hoverPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hoverPreview.name = "HoverPreview";
        hoverPreview.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        hoverPreview.GetComponent<MeshRenderer>().material = preview;
        Destroy(hoverPreview.GetComponent<SphereCollider>());
        gridScript.hoverPreview = hoverPreview;
    }


}
