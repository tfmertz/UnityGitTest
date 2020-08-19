using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    public class UndoAction
    {
        public static List<UndoAction> UndoList;
        public static bool isBuilt = false;
        public static Tool theTool;
        //
        private Type action;
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
        //
        // Information on Current State 
        //
        public static Quaternion CurrentGimbalRotation;
        public static Vector3 CurrentCameraPosition;
        //
        public CallDelegateTranslate MethodCallTranslate;

        public enum Type { ROTATE, ADD, DELETE, PAINT, ZOOM };

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
        public void DoUndo()
        {
            Debug.Log(action);

            // set up temp tool holders
            Tool.Tools tempTool = theTool.activeTool;
            Vector3 tempPosition = theTool.currentPosition;
            Color tempColor = theTool.color;
            ignoreDuringUndo = true;
            switch (action)
            {
                case Type.ADD:
                    //MethodCall();
                    //
                    // assign temp tool values
                    theTool.currentPosition = TouchedUndoPosition;
                    theTool.activeTool = Tool.Tools.Delete;
                    //
                    // Apply the changes
                    theTool.Apply();
                    //
                    // reset the tool
                    theTool.currentPosition = tempPosition;
                    theTool.activeTool = tempTool;
                    break;
                case Type.DELETE:
                    //MethodCall();
                    //
                    // assign temp tool values
                    theTool.currentPosition = TouchedUndoPosition;
                    theTool.activeTool = Tool.Tools.Add;
                    //
                    // Apply the changes
                    theTool.Apply();
                    //
                    // reset the tool
                    theTool.currentPosition = tempPosition;
                    theTool.activeTool = tempTool;
                    break;
                case Type.PAINT:
                    //MethodCall();
                    //
                    // assign temp tool values
                    theTool.currentPosition = TouchedUndoPosition;
                    theTool.activeTool = Tool.Tools.Paint;
                    theTool.color = ColorUndoValue;
                    //
                    // Apply the changes
                    theTool.Apply();
                    //
                    // reset the tool
                    theTool.currentPosition = tempPosition;
                    theTool.activeTool = tempTool;
                    theTool.color = tempColor;
                    break;
                case Type.ROTATE:
                    CurrentGimbalRotation = Gimbal.transform.rotation = GimbalUndoTransform;
                    Debug.Log("Gimbal End:" + GimbalUndoTransform);
                    break;
                case Type.ZOOM:
                    CurrentCameraPosition = Camera.transform.localPosition = CameraUndoPosition;
                    Debug.Log("Camera End:" + CameraUndoPosition);
                    break;
            }
            ignoreDuringUndo = false;
            UndoList.RemoveAt(UndoList.Count - 1);
            //Debug.Log("List Size" + UndoAction.UndoList.Count);
        }
        public void AddToList()
        {

            if (!isBuilt)
            {
                UndoAction.UndoList = new List<UndoAction>(); 
                isBuilt = true;
            }
            UndoAction.UndoList.Add(this);
        }
        public static void Undo()
        {
            if (UndoAction.UndoList.Count > 0)
            {
                UndoAction.UndoList[UndoList.Count - 1].DoUndo();
            }
        }
    }
}
