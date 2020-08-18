using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    public class UndoAction
    {
        public static List<UndoAction> UndoList;
        public static bool isBuilt = false;
        //
        private Type action;
        public Camera Camera;

        public Vector3 CameraUndoPosition;
        public GameObject Gimbal;
        public Quaternion GimbalUndoTransform;
        public delegate void CallDelegate();
        public delegate void CallDelegateTranslate(Transform t, Transform r);
        public CallDelegate MethodCall;
        //
        // Information on Last Actions 
        //
        public static Vector3 LastTouchedPosition;
        public static Quaternion CurrentGimbalRotation;
        public static Vector3 CurrentCameraPosition;
        //
        public CallDelegateTranslate MethodCallTranslate;

        public enum Type { ROTATE, ADD, DELETE, COLOR, ZOOM };

        public UndoAction(Type CameraAction, GameObject Gimbal)
        {
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
            action = CreationAction;
            this.MethodCall = Method;
            Debug.Log("Method Create:" + GimbalUndoTransform);
            AddToList();
        }
        public void DoUndo()
        {
            Debug.Log(action);
            switch (action)
            {
                case Type.ADD:
                    MethodCall();

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
