using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine {
    public class CreationLayerManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<CreationLayer> CreationLayers;
        void Start()
            {

            }
        public void AddLayer(string name, CreateVoxel voxelLayer)
        {
            CreationLayers.Add(new CreationLayer(name, voxelLayer));
        }
        public void RemoveLayer(int id)
        {
            
        }
    }

    [System.Serializable]
    public class CreationLayer
    {
        public string Name;
        private CreateVoxel voxel;
        private string ID;

        public CreationLayer (string name, CreateVoxel voxelLayer)
        {
            Name = name;
            voxel = voxelLayer;
            ID = System.Guid.NewGuid().ToString();
        }
    }
}