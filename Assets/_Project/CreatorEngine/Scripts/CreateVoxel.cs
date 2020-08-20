using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    /// <summary>
    /// The CreateVoxel class is the API for adding voxels at a position into a mesh
    /// and correctly generating everything needed to view them. 
    /// </summary>
    [System.Serializable]
    public class CreateVoxel : MonoBehaviour
    {
        public Material mat;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        public Grid Grid { get; set; }

        Mesh currentMesh;
        int[,,] currentVoxelMap;
        bool isPreview;

        // Stores information about where our voxels are in the current grid
        // Each CreateVoxel (or Layer) will have one to store info to quickly
        // look up whether a space is occupied. The int corresponds to the Voxel's
        // index inside the currentVoxel List for easy reverse lookup.
        int[,,] voxelMap;


        List<Voxel> currentVoxels = new List<Voxel>();
        public List<Voxel> CurrentVoxels { get { return currentVoxels; } }

        // Start is called before the first frame update
        void Awake()
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        private void Start()
        {
            CreateVoxelMap();
        }

        // Resets information about where voxels are in our grid
        void CreateVoxelMap()
        {
            voxelMap = new int[Grid.width, Grid.height, Grid.width];
            // Fill it with -1
            for (int i = 0; i < Grid.width; i++)
            {
                for (int j = 0; j < Grid.height; j++)
                {
                    for (int k = 0; k < Grid.width; k++)
                    {
                        voxelMap[i, j, k] = -1;
                    }
                }
            }
        }

        // Checks if the position is taken
        bool CheckVoxelMap(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);

            // First, make sure we aren't out of bounds
            if (x < 0 || x > Grid.width - 1 || y < 0 || y > Grid.height - 1 || z < 0 || z > Grid.width - 1)
            {
                return false;
            }

            // if we have a valid voxel index inside currentVoxels, then a voxel exists at this position
            return voxelMap[x, y, z] > -1;
        }

        public void Create(Vector3 position)
        {
            // First, check if we have a voxel at this position
            if (CheckVoxelMap(position))
            {
                // bail if so
                Debug.Log($"Existing voxel at position: {position}");
                return;
            }

            // Otherwise, add this position to the currentVoxels and save its index into the voxelMap
            currentVoxels.Add(new Voxel
            {
                position = position,
            });
            voxelMap[(int)position.x, (int)position.y, (int)position.z] = currentVoxels.Count - 1;

            CreateVoxelMesh(currentVoxels.ToArray());
        }

        public void Undo()
        {
            // Bail if there's nothing to undo
            if (currentVoxels.Count == 0) return;

            // If there is only 1 voxel left, make a blank mesh
            if (currentVoxels.Count == 1)
            {
                meshFilter.mesh = new Mesh();
                currentVoxels.Clear();
                return;
            }

            // Otherwise, take the last voxel created out and save the result into our array to rebuild
            currentVoxels.RemoveAt(currentVoxels.Count - 1);
            Voxel[] newVoxels = currentVoxels.ToArray();
            // Clear out our existing array, this will be rebuilt on load
            currentVoxels.Clear();

            Load(newVoxels);
            Debug.Log("Undo");
        }

        public void Load(Voxel[] voxels)
        {
            // Reset our position map before load
            CreateVoxelMap();
            currentVoxels.Clear();

            for (int i = 0; i < voxels.Length; i++)
            {
                Vector3 position = voxels[i].position;
                currentVoxels.Add(new Voxel
                {
                    position = position,
                    color = voxels[i].color,
                });
                voxelMap[(int)position.x, (int)position.y, (int)position.z] = i;
            }

            // Once our currentVoxels and voxelMap is populated, we can create the mesh
            CreateVoxelMesh(currentVoxels.ToArray());
        }

        // Allows tools to manipulate the current voxels for the purpose
        // of displaying previews, but doesn't actually affect the current mesh
        // or collider
        public void Preview(Voxel[] voxels)
        {
            isPreview = true;
            // Reset the voxelMap
            CreateVoxelMap();
            // Add in our preview voxels
            for (int i = 0; i < voxels.Length; i++)
            {
                Vector3 position = voxels[i].position;
                voxelMap[(int)position.x, (int)position.y, (int)position.z] = i;
            }
            CreateVoxelMesh(voxels);
        }

        public void StopPreview()
        {
            meshFilter.mesh = currentMesh;
            voxelMap = currentVoxelMap;
            isPreview = false;
        }

        public int GetVoxelFromPosition(Vector3 pos)
        {
            return voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        }

        /// <summary>
        /// Goes through all the currentVoxels and checks them against the voxelMap
        /// and adds each of their faces that aren't shared by another voxel. Then
        /// it saves all the vertices, triangles, and uvs into a unity mesh
        /// </summary>
        void CreateVoxelMesh(Voxel[] voxels)
        {
            int vertIndex = 0;
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();

            // Go through each voxel in our current voxels
            for (int k = 0; k < voxels.Length; k++)
            {
                // Go through each of the faces of that voxel
                // When creating a voxel face we need to go through the VoxelData.faceVertices
                // array to find the indices for each vertex in the VoxelData.vertices array that correspond
                // to the correct position for our mesh's vertices.
                for (int j = 0; j < 6; j++)
                {
                    Vector3 pos = voxels[k].position;
                    Color color = voxels[k].color;
                    // Check if the face we are creating has a voxel at that position already
                    if (!CheckVoxelMap(pos + VoxelData.faceChecks[j]))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int triIndex = VoxelData.faceVertices[j, i];
                            verts.Add(VoxelData.vertices[triIndex] + pos);
                            colors.Add(color);

                            if (i == 3)
                            {
                                // There are 4 total vertices for a face, but we use 6 indices to make 2 triangles
                                // for the unity mesh
                                // When we add the last vertex, we need to setup the last tri in one go
                                // because we are done adding vertices in the i for loop
                                tris.Add(vertIndex - 1);
                                tris.Add(vertIndex - 2);
                            }
                            tris.Add(vertIndex);
                            uvs.Add(VoxelData.uvs[i]);
                            vertIndex++;
                        }
                    }
                }
            }

            // Once everything is added to the vertices, trianges, and uvs array, we can create our unity mesh
            Mesh mesh = new Mesh
            {
                name = "Voxel",
                vertices = verts.ToArray(),
                triangles = tris.ToArray(),
                uv = uvs.ToArray(),
                colors = colors.ToArray(),
            };
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            meshRenderer.material = mat;

            // Set the collider to be the mesh
            if (!isPreview)
            {
                currentMesh = mesh;
                currentVoxelMap = (int[,,])voxelMap.Clone();
                meshCollider.sharedMesh = mesh;
            }
        }
    }
}