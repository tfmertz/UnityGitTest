using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Appvolks.UXFoundation;

namespace Arkh.CreatorEngine
{
    public class CreationController : MonoBehaviour
    {
        private CreationEditor editor;
        public Camera CreatorCamera;
        private int num; // get Voxel Layer count;
        // Start is called before the first frame update
        private void Start()
        {
            editor = gameObject.AddComponent<CreationEditor>(); 
            editor.CreatorCamera = CreatorCamera;
            editor.CreateNew("layer1");
            UXAppManager.UXEventManager.Register(SF.GenericEventManager.AppEventType.CreationEngine, Controller);
            gameObject.AddComponent<CreationEditor>();
        }

        private void Controller(object o)
        {

            string s = (string)o;
            Debug.Log("command:" + s);
            switch (s)
            {
                case "remove":
                    editor.Exit();
                    break;
                case "undo":
                    editor.Undo();
                    break;
                case "create":
                    editor.CreateNew("layer_" + num);
                    break;
                case "save":
                    editor.Save();
                    break;
                case "load":
                    var exists = editor.TryLoad();
                    if (!exists) editor.CreateNew("layer_" + num);
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
