using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The CreateVoxel class is the API for adding voxels at a position into a mesh
/// and correctly generating everything needed to view them. 
/// </summary>
public class CreateVoxel : MonoBehaviour
{
    public Material mat;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Grid grid;

    public Grid Grid { set { grid = value; } }

    List<Voxel> currentVoxels = new List<Voxel>();

    // Stores information about where our voxels are in the current grid
    // Each CreateVoxel (or Layer) will have one to store info to quickly
    // look up whether a space is occupied. The int corresponds to the Voxel's
    // index inside the currentVoxel List for easy reverse lookup.
    int[,,] voxelMap;

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
        voxelMap = new int[grid.width, grid.height, grid.width];
        // Fill it with -1
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                for (int k = 0; k < grid.width; k++)
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
        if (x < 0 || x > grid.width - 1 || y < 0 || y > grid.height - 1 || z < 0 || z > grid.width - 1)
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

        CreateVoxelMesh();
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

        for (int i = 0; i < voxels.Length; i++)
        {
            // TODO Create is less efficient because it needs to recreate the voxel mesh
            // for each new voxel as it's meant to add to an existing voxel mesh, but with load we know all
            // the voxel positions to start, so we can add them all to the voxelMap and then create the mesh once
            Create(voxels[i].position);
        }
    }

    /// <summary>
    /// Goes through all the currentVoxels and checks them against the voxelMap
    /// and adds each of their faces that aren't shared by another voxel. Then
    /// it saves all the vertices, triangles, and uvs into a unity mesh
    /// </summary>
    void CreateVoxelMesh()
    {
        int vertIndex = 0;
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Go through each voxel in our current voxels
        for (int k = 0; k < currentVoxels.Count; k++)
        {
            // Go through each of the faces of that voxel
            // When creating a voxel face we need to go through the VoxelData.faceVertices
            // array to find the indices for each vertex in the VoxelData.vertices array that correspond
            // to the correct position for our mesh's vertices.
            for (int j = 0; j < 6; j++)
            {
                Vector3 pos = currentVoxels[k].position;
                // Check if the face we are creating has a voxel at that position already
                if (!CheckVoxelMap(pos + VoxelData.faceChecks[j]))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int triIndex = VoxelData.faceVertices[j, i];
                        verts.Add(VoxelData.vertices[triIndex] + pos);

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
        };
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material = mat;

        // Set the collider to be the mesh
        meshCollider.sharedMesh = mesh;
    }
}
