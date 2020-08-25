using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    public class UndoAction
    {
        public static List<UndoAction> UndoList = new List<UndoAction>();
        public static Tool theTool;
        public static LayerManager theLayerManager;
        //
        private Type action;
        public Type Action { get { return action; } }
        public Camera Camera;
        public static bool ignoreDuringUndo = false;
        public Color ColorUndoValue;
        public Vector3 TouchedUndoPosition;
        public Vector3 CameraUndoPosition;
        public GameObject Gimbal;
        public Quaternion GimbalUndoTransform;
        public delegate void CallDelegate();
        public delegate void CallDelegateTranslate(Transform t, Transform r);
        public CallDelegate MethodCall;
        public CreationLayer LayerHolder;
        //
        // Information on Current State 
        //
        public static Quaternion CurrentGimbalRotation;
        public static Vector3 CurrentCameraPosition;
        //
        public CallDelegateTranslate MethodCallTranslate;

        public enum Type { ROTATE, ADD, DELETE, PAINT, ZOOM, ADDLAYER, RENAMELAYER, REMOVELAYER, SELECTLAYER, DUPLICATELAYER};

        public UndoAction(Type CameraAction, GameObject Gimbal)
        {
            // Undo Gimbal Rotation
            if (CurrentGimbalRotation != Gimbal.transform.rotation)
            {
                action = CameraAction;
                this.Gimbal = Gimbal;
                GimbalUndoTransform = CurrentGimbalRotation;
                Debug.Log("Gimbal Create:" + GimbalUndoTransform);

                // set the current position
                CurrentGimbalRotation = Gimbal.transform.rotation;
                AddToList();
            }
        }
        public UndoAction(Type CameraAction, Camera Camera)
        {
            // Undo Camera Zoom
            if (CurrentCameraPosition != Camera.transform.localPosition)
            {
                action = CameraAction;
                this.Camera = Camera;
                CameraUndoPosition = CurrentCameraPosition;
                Debug.Log("Camera Create:" + CameraUndoPosition);

                // set the current position
                CurrentCameraPosition = Camera.transform.localPosition;
                AddToList();
            }
        }
        public UndoAction(Type CreationAction, CallDelegate Method)
        {
            // Add a Method Undo Action
            action = CreationAction;
            this.MethodCall = Method;
            Debug.Log("Method Create:" + GimbalUndoTransform);
            AddToList();
        }
        public UndoAction(Type CreationAction, Vector3 TouchPosition)
        {
            // Add a Delete or Add Undo Action
            if (!ignoreDuringUndo)
            {
                action = CreationAction;
                TouchedUndoPosition = TouchPosition;
                Debug.Log("create:" + CreationAction);
                AddToList();
            }else
            {
                Debug.Log("Skip");
            }
        }
        public UndoAction(Type CreationAction, Vector3 TouchPosition, Color c)
        {
            // Add a color Undo Action
            if (!ignoreDuringUndo)
            {
                action = CreationAction;
                TouchedUndoPosition = TouchPosition;
                ColorUndoValue = c;
                Debug.Log("create:" + CreationAction);
                AddToList();
            }
            else
            {
                Debug.Log("Skip");
            }
        }

        public UndoAction(Type CreationAction, CreationLayer layer)
        {
            // Add a Method Undo Action
            action = CreationAction;
            LayerHolder = new CreationLayer(layer.Name, layer.Voxel, layer.Position, layer.ID);
            Debug.Log("Method Layer Action:" + CreationAction + " Name:"+layer.Name);
            AddToList();
        }

        public void DoUndo()
        {
            Debug.Log(action);

            // set up temp tool holders
            Tool.Tools tempTool = theTool.activeTool;
            Vector3 tempPosition = theTool.previewPosition;
            Color tempColor = theTool.color;
            ignoreDuringUndo = true;
            switch (action)
            {
                case Type.ADD:
                    //MethodCall();
                    //
                    // assign temp tool values
                    theTool.Undo(this);
                    break;
                case Type.DELETE:
                    //MethodCall();
                    //
                    // assign temp tool values
                    theTool.Undo(this);
                    break;
                case Type.PAINT:
                    //MethodCall();
                    //
                    // assign temp tool values
                    theTool.Undo(this);
                    break;
                case Type.ROTATE:
                    CurrentGimbalRotation = Gimbal.transform.rotation = GimbalUndoTransform;
                    Debug.Log("Gimbal End:" + GimbalUndoTransform);
                    break;
                case Type.ZOOM:
                    CurrentCameraPosition = Camera.transform.localPosition = CameraUndoPosition;
                    Debug.Log("Camera End:" + CameraUndoPosition);
                    break;
                case Type.REMOVELAYER:
                    Debug.Log("remove."+ LayerHolder);
                    theLayerManager.DuplicateLayer(LayerHolder, true);
                    break;
                case Type.RENAMELAYER:
                    theLayerManager.RenameLayer(LayerHolder.Name, true);
                    break;
                case Type.ADDLAYER:
                    theLayerManager.RemoveLayer(LayerHolder.ID, true);
                    break;
                case Type.SELECTLAYER:
                    theLayerManager.SelectLayer(LayerHolder.ID, true);
                    break;
                case Type.DUPLICATELAYER:
                    theLayerManager.RemoveLayer(LayerHolder.ID, true);
                    break;
            }
            ignoreDuringUndo = false;
            UndoList.RemoveAt(UndoList.Count - 1);
            //Debug.Log("List Size" + UndoAction.UndoList.Count);
        }
        public void AddToList()
        {
            UndoAction.UndoList.Add(this);
        }
        public static void Undo()
        {
            if (UndoAction.UndoList.Count > 0)
            {
                UndoAction.UndoList[UndoList.Count - 1].DoUndo();
            }
        }

        public static void ClearHistory()
        {
            UndoList.Clear();
        }
    }
}
