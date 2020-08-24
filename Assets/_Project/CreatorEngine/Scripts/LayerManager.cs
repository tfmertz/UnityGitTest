using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{

    public class LayerManager : MonoBehaviour
    {

        public delegate void CreateALayer();
        public event CreateALayer LayersUpdated;

        // Start is called before the first frame update
        public List<CreationLayer> CreationLayers;
        public static CreationLayer SelectedLayer { get; set; }
        void Start()
        {

        }

        public void AddLayer(string name, CreateVoxel voxelLayer)
        {
            /*GameObject layer = new GameObject();
            CreationLayer cLayer = layer.AddComponent<CreationLayer>();
            cLayer.Create(name, voxelLayer);
            CreationLayers.Add(cLayer);
            */
            SelectedLayer = new CreationLayer(name, voxelLayer);
            CreationLayers.Add(SelectedLayer);
            LayersUpdated.Invoke();

        }
        public void RenameLayer(string name)
        {
            SelectedLayer.Name = name;
        }

        public void RemoveLayer(string guid)
        {
            CreationLayers.RemoveAll(x => x.ID == guid);
            SelectedLayer = CreationLayers[0];
            LayersUpdated.Invoke();
        }

        public CreationLayer SelectLayer(string guid)
        {
            SelectedLayer = CreationLayers.Find(x => x.ID == guid);
            Debug.Log("SelectedLayer:" + SelectedLayer);
            return SelectedLayer;
        }

        public void RenameLayer(string name, string guid)
        {
            CreationLayer layer = CreationLayers.Find(x => x.ID == guid);
            layer.Name = name;
            LayersUpdated.Invoke();
        }

        public void MoveLayer(int position, string guid)
        {
            CreationLayer layer = CreationLayers.Find(x => x.ID == guid);
            CreationLayers.RemoveAll(x => x.ID == guid);
            CreationLayers.Insert(position, layer);
            LayersUpdated.Invoke();
        }

        public void GetLayer(string guid)
        {
            CreationLayer layer = CreationLayers.Find(x => x.ID == guid);
        }

        public void HideShowLayer(bool show, string guid)
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
        }
    }

    [System.Serializable]
    public class CreationLayer
    {
        public string Name;
        public CreateVoxel Voxel;
        public int Position;
        public string ID { get; private set; }

        public CreationLayer(string name, CreateVoxel voxelLayer)
        {
            Name = name;
            Voxel = voxelLayer;
            ID = System.Guid.NewGuid().ToString();
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