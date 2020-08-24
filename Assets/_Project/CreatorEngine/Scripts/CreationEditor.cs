﻿using System.Collections;
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

        // Temp Variables
        Quaternion tempRotation;
        Vector3 tempZoom;
        int ToggleViewType = 0;

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
            UndoAction.Undo();
            //gridScript.Undo();
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
                // Reset the undo history
                UndoAction.ClearHistory();
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
            //ResetGridTransform(w, h);
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
                touchController.theGrid = null;
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

            // Create Grid
            grid = new GameObject("Grid");
            grid.transform.SetParent(transform);
            gridScript = grid.AddComponent<Grid>();

            // Create voxel Layer
            CreateVoxelLayer();

            gridScript.creatorCamera = CreatorCamera;

            // Either find, or add our touch controller
            touchController = gameObject.GetComponent<TouchController>();
            if (touchController == null)
            {
                touchController = gameObject.AddComponent<TouchController>();
            }
            touchController.TheCamera = CreatorCamera;
            touchController.theGrid = gridScript;
            touchController.SpinableObject = CreatorCamera.transform.parent.gameObject;
            //touchController.SpinableObject.transform.position = gridTransform.transform.position;

            UndoAction.CurrentCameraPosition = touchController.TheCamera.transform.localPosition = touchController.StartZoomPosition;
            UndoAction.CurrentGimbalRotation = touchController.SpinableObject.transform.rotation = touchController.StartSpinPosition;
            Debug.Log("cam start:" + UndoAction.CurrentCameraPosition);
            //ResetGridTransform(w, h);
        }

        public void ChangeLayer()
        {
            LayerManager manager = GetComponent<LayerManager>();
            CreateVoxel voxelScript = LayerManager.SelectedLayer.Voxel;
            SetEditableVoxelLayer(voxelScript);
        }

        public void DeleteLayer()
        {
            Debug.Log("Delete Voxel");
            LayerManager manager = GetComponent<LayerManager>();
            CreateVoxel voxelScript = LayerManager.SelectedLayer.Voxel;
            // delete VoxelScript
            manager.RemoveLayer(LayerManager.SelectedLayer.ID);
            Destroy(voxelScript.gameObject);
        }

        private void SetEditableVoxelLayer(CreateVoxel voxelScript)
        {
            gridScript.CreateVoxel = voxelScript;
        }

        public void CreateVoxelLayer(string vName = "Layer 1")
        {
            LayerManager manager = GetComponent<LayerManager>();
            // Get needed materials
            Material voxel = Resources.Load<Material>("Voxel");

            // Create grid and voxel creator
            GameObject voxelCreator = new GameObject("VoxelCreator");

            CreateVoxel voxelScript = voxelCreator.AddComponent<CreateVoxel>();
            voxelScript.mat = voxel;
            voxelScript.Grid = gridScript;
            voxelCreator.transform.SetParent(grid.transform);
            voxelCreator.transform.localPosition = new Vector3(0, 0, 0);
            SetEditableVoxelLayer(voxelScript);
            manager.AddLayer(vName, voxelScript);
        }
        public void RenameLayer(string name)
        {
            Debug.Log("renaming to:" + name);
            LayerManager manager = GetComponent<LayerManager>();
            manager.RenameLayer(name);

        }
        private void ResetGridTransform(int w, int h)
        {
            gridScript.transform.localPosition = new Vector3(w / 2 * -1, 0, h / 2 * -1);
            gridScript.transform.parent.transform.position = new Vector3(w / 2, 0, h / 2);
            if (touchController)
                touchController.SpinableObject.transform.localPosition = gridScript.transform.parent.transform.position;
        }
        public void ResetGridToCenter()
        {
            if (touchController)
            {
                tempZoom = touchController.TheCamera.transform.localPosition = new Vector3(0, 0, -79);
                tempRotation = touchController.SpinableObject.transform.rotation = Quaternion.Euler(30, 45, 0);
            }
        }
        public void ToggleView()
        {
            int type = ToggleViewType;
            if (type == 0) // Change to 2D view
            {
                ToggleViewType = 1;
                CreatorCamera.orthographic = true;
                // save old zoom and rotation
                tempRotation = touchController.SpinableObject.transform.rotation;
                tempZoom = touchController.TheCamera.transform.localPosition;
                // reorient Camera
                touchController.SpinableObject.transform.rotation = Quaternion.Euler(90, 0, 0);
                touchController.TheCamera.transform.localPosition = new Vector3(0, 0, -79);
                Grid g = grid.GetComponent<Grid>();
                CreatorCamera.orthographicSize = g.height;
                if (g.width > g.height) CreatorCamera.orthographicSize = g.width;
            }
            else // 3D View
            {
                ToggleViewType = 0;
                CreatorCamera.orthographic = false;
                // reorient Rotation
                touchController.SpinableObject.transform.rotation = tempRotation;
                // reorient Camera
                touchController.TheCamera.transform.localPosition = tempZoom;
            }
        }
    }
}
