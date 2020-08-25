using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Arkh.CreatorEngine
{

    public class LayerManager : MonoBehaviour
    {

        public delegate void CreateALayer();
        public event CreateALayer LayersUpdated;

        // Start is called before the first frame update
        public static List<CreationLayer> CreationLayers = new List<CreationLayer>();
        public static CreationLayer SelectedLayer { get; set; }
        void Start()
        {
            UndoAction.theLayerManager = this;
        }
        public void DuplicateLayer(CreationLayer layer, bool undo = false)
        {
            Debug.Log("duplicate of:" + layer.Name);
            SelectedLayer = new CreationLayer(layer.Name, layer.Voxel, layer.Position, layer.ID);
            CreationLayers.Add(SelectedLayer);
            // TODO: move to correct position
            if (!undo)
                new UndoAction(UndoAction.Type.DUPLICATELAYER, SelectedLayer);
            LayersUpdated.Invoke();
        }
        public void AddLayer(string name, CreateVoxel voxelLayer, bool undo = false)
        {
            /*GameObject layer = new GameObject();
            CreationLayer cLayer = layer.AddComponent<CreationLayer>();
            cLayer.Create(name, voxelLayer);
            CreationLayers.Add(cLayer);
            */
            Debug.Log("adding Layer:" + name);
            SelectedLayer = new CreationLayer(name, voxelLayer, CreationLayers.Count+1);
            CreationLayers.Add(SelectedLayer);
            LayersUpdated.Invoke();
            if(!undo)
                new UndoAction(UndoAction.Type.ADDLAYER, SelectedLayer);
        }
        public void RenameLayer(string name, bool undo = false)
        {
             if (!undo)
                new UndoAction(UndoAction.Type.RENAMELAYER, SelectedLayer);
            SelectedLayer.Name = name;
            Debug.Log("Invoking renaming to:" + name);
            LayersUpdated.Invoke();
        }

        public void RemoveLayer(string guid, bool undo = false)
        {
            Debug.Log("priming:"+SelectedLayer);
            
            if (!undo)
                new UndoAction(UndoAction.Type.REMOVELAYER, SelectedLayer);

            CreationLayers.RemoveAll(x => x.ID == guid);
            bool isEmpty = !CreationLayers.Any();
            if (isEmpty)
            {
                SelectedLayer = null;
            }
            else
            {
                SelectedLayer = CreationLayers[0];
            }
            LayersUpdated.Invoke();

           
        }

        public CreationLayer SelectLayer(string guid, bool undo = false)
        {
            new UndoAction(UndoAction.Type.SELECTLAYER, SelectedLayer);
            SelectedLayer = CreationLayers.Find(x => x.ID == guid);
            Debug.Log("SelectedLayer:" + SelectedLayer);
            if (!undo)
                new UndoAction(UndoAction.Type.SELECTLAYER, SelectedLayer);
            return SelectedLayer;
        }

        public void MoveLayer(int position, string guid, bool undo = false)
        {
            CreationLayer layer = CreationLayers.Find(x => x.ID == guid);
            CreationLayers.RemoveAll(x => x.ID == guid);
            CreationLayers.Insert(position, layer);
            LayersUpdated.Invoke();
            if (!undo)
                new UndoAction(UndoAction.Type.RENAMELAYER, SelectedLayer);
        }

        public void GetLayer(string guid)
        {
            CreationLayer layer = CreationLayers.Find(x => x.ID == guid);
        }

        public void HideShowLayer(bool show, string guid, bool undo = false)
        {
            CreationLayer layer;
            if (guid == "")
            {
                layer = SelectedLayer;
            }
            else
            {
                layer = CreationLayers.Find(x => x.ID == guid);
            }

            layer.Voxel.enabled = show;
            if (!undo)
                new UndoAction(UndoAction.Type.RENAMELAYER, SelectedLayer);
        }

    }

    [System.Serializable]
    public class CreationLayer
    {
        public string Name;
        public CreateVoxel Voxel;
        public int Position;
        public string ID { get; private set; }

        public CreationLayer(string name, CreateVoxel voxelLayer, int position = -1, string id = "")
        {
            if (name == "") name = "Layer_" + (LayerManager.CreationLayers.Count + 1);
            Name = name;

            Voxel = voxelLayer;
            ID = id;
            if(ID == "")
                ID = System.Guid.NewGuid().ToString();
            Position = position;
            if (position == -1)
                if(LayerManager.CreationLayers != null)
                    Position = LayerManager.CreationLayers.Count+1;

        }
        /*
        public void Create(string name, CreateVoxel voxelLayer)
        {
            Name = name;
            Voxel = voxelLayer;
            ID = System.Guid.NewGuid().ToString();
        }
        */
    }
    public class LayerButton : MonoBehaviour
    {
        public string Name
        {
            get
            {
                return Layer.Name;
            }
        }
        public CreateVoxel Voxel
        {
            get
            {
                return Layer.Voxel;
            }
        }
        public int Position
        {
            get
            {
                return Layer.Position;
            }
        }
        public string ID
        {
            get
            {
                return Layer.ID;
            }
        }
        public CreationLayer Layer;
    }
}