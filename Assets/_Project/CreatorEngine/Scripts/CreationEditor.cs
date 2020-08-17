using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace Arkh.CreatorEngine
{
    /// <summary>
    /// Holds the API for the voxel creation editor.
    /// </summary>
    public class CreationEditor : MonoBehaviour
    {
        Creation currentCreation;
        List<Voxel> currentVoxels;

        // External references
        public Camera CreatorCamera;

        GameObject grid;
        Grid gridScript;


        TouchController touchController;
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
            // bool not allowed in UI call
            TryLoad();
        }
        public bool TryLoad()
        {
            if (File.Exists($"{Application.persistentDataPath}/creation.json"))
            {
                string fileData = File.ReadAllText($"{Application.persistentDataPath}/creation.json");
                currentCreation = JsonUtility.FromJson<Creation>(fileData);
                SetupGrid();
                gridScript.Load(currentCreation);
                return true;
            }
            else
            {
                Debug.Log("No file");
                return false;
            }
        }

        public void ChangeGridSize(int w, int h)
        {
            Creation creation = currentCreation;
            gridScript.ChangeGridSize(w, h);
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

        public void SwitchTool(string tool)
        {
            if (System.Enum.TryParse(tool, out Tool.Tools enumTool))
            {
                gridScript.SwitchTool(enumTool);
            }
            else
            {
                Debug.LogWarning($"No tool found of type '{tool}'");
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
            Material voxel = Resources.Load<Material>("Voxel");

            // Create grid and voxel creator
            GameObject voxelCreator = new GameObject("VoxelCreator");
            CreateVoxel voxelScript = voxelCreator.AddComponent<CreateVoxel>();
            voxelScript.mat = voxel;

            grid = new GameObject("Grid");
            gridScript = grid.AddComponent<Grid>();
            gridScript.mat = gridMat;
            gridScript.CreateVoxel = voxelScript;

            GameObject gridTransform = new GameObject("GridTransform");
            gridScript.Init(gridTransform);
            grid.transform.SetParent(gridTransform.transform);
            voxelCreator.transform.SetParent(grid.transform);
            gridScript.creatorCamera = CreatorCamera;
            //gridScript.voxelParent = gridTransform;

            var w = gridScript.width;
            var h = gridScript.height;
            gridScript.transform.position = new Vector3(w / 2 * -1, 0, w / 2 * -1);
            gridTransform.transform.position = new Vector3(w / 2, 0, w / 2);

            gridTransform.transform.SetParent(this.transform);
            voxelScript.Grid = gridScript;

            touchController = gameObject.AddComponent<TouchController>();
            touchController.camera = CreatorCamera;
            touchController.theGrid = gridScript;
            touchController.SpinableObjectY = CreatorCamera.transform.parent.gameObject;
            //  touchController.SpinableObjectX = CreatorCamera.transform.parent.gameObject;
            // change this for old way of rotation
            touchController.SpinableObjectX = gridTransform;
            touchController.SpinableObjectY.transform.position = gridTransform.transform.position;

            touchController.camera.transform.localPosition = new Vector3(0, 0, -79);
            touchController.SpinableObjectY.transform.rotation = Quaternion.Euler(30, 45, 0);
        }
    }
}