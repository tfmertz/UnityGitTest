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
            UndoAction.UndoList[0].Undo();
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
            ResetGridTransform(w, h);
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


            gridTransform.transform.SetParent(this.transform);
            voxelScript.Grid = gridScript;

            touchController = gameObject.AddComponent<TouchController>();
            touchController.camera = CreatorCamera;
            touchController.theGrid = gridScript;
            touchController.SpinableObject = CreatorCamera.transform.parent.gameObject;
            touchController.SpinableObject.transform.position = gridTransform.transform.position;

            touchController.camera.transform.localPosition = new Vector3(0, 0, -79);
            touchController.SpinableObject.transform.rotation = Quaternion.Euler(30, 45, 0);
            ResetGridTransform(w, h);
        }

        private void ResetGridTransform(int w, int h)
        {
            gridScript.transform.localPosition = new Vector3(w / 2 * -1, 0, h / 2 * -1);
            gridScript.transform.parent.transform.position = new Vector3(w / 2, 0, h / 2);
            if (touchController)
                touchController.SpinableObject.transform.localPosition = gridScript.transform.parent.transform.position;

            // TODO: Clean up Voxels that exist outside.
        }
        public void ResetGridToCenter()
        {
            if (touchController)
            {
                touchController.camera.transform.localPosition = new Vector3(0, 0, -79);
                touchController.SpinableObject.transform.rotation = Quaternion.Euler(30, 45, 0);
            }
        }
    }
    public class UndoAction {
        public static List<UndoAction> UndoList;
        public static bool isBuilt = false;
        //
        private Type action;
        public Camera Camera;
        public Transform CameraTransform;
        public GameObject Gimbal;
        public Quaternion GimbalTransform;
        public delegate void CallDelegate();
        public delegate void CallDelegateTranslate(Transform t, Transform r);
        public CallDelegate MethodCall;

        public CallDelegateTranslate MethodCallTranslate;

        public enum Type {ROTATE, ADD, DELETE, COLOR};
        
        public UndoAction(Type CameraAction, Camera Camera, GameObject Gimbal)
        {
            action = CameraAction;

            this.Camera = Camera;
            CameraTransform = Camera.transform;
            this.Gimbal = Gimbal;
            GimbalTransform = Gimbal.transform.rotation;
            AddToList();
        }
        public UndoAction(Type CreationAction, CallDelegate Method)
        {
            action = CreationAction;
            this.MethodCall = Method;
            AddToList();
        }
        public void Undo()
        {
            Debug.Log(action);
            switch (action) {
                case Type.ADD:
                    MethodCall();
                    break;
                case Type.ROTATE:
                    Gimbal.transform.rotation = GimbalTransform;
                    Debug.Log("Gimbal End:"+GimbalTransform);
                    break;
            }
            UndoList.RemoveAt(UndoList.Count - 1);
            Debug.Log("List Size" + UndoAction.UndoList.Count);
        }
        public void AddToList()
        {
           
            if (!isBuilt)
            {
                UndoAction.UndoList = new List<UndoAction>(); Debug.Log("new list");
                isBuilt = true;
            }
            
            UndoAction.UndoList.Add(this);
            Debug.Log("List Size" + UndoAction.UndoList.Count);
        }
    }
}