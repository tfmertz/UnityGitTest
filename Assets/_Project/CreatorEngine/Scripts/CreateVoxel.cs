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

    int vertIndex = 0;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    List<Voxel> currentVoxels = new List<Voxel>();

    public List<Voxel> CurrentVoxels { get { return currentVoxels; } }

    // Start is called before the first frame update
    void Awake()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
    }

    public void Create(Vector3 position)
    {

        AddVoxel(position);
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
    }

    public void Undo()
    {
        // Bail if there's nothing to undo
        if (currentVoxels.Count == 0) return;

        // Remove any existing vertices, tris, or uvs and reset our vertIndex
        ResetGeometry();

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
        for (int i = 0; i < voxels.Length; i++)
        {
            Create(voxels[i].position);
        }
    }

    void ResetGeometry()
    {
        vertIndex = 0;
        verts = new List<Vector3>();
        tris = new List<int>();
        uvs = new List<Vector2>();
    }

    void AddVoxel(Vector3 pos)
    {
        // Add the data in to our array so that we can save
        currentVoxels.Add(new Voxel
        {
            position = pos
        });
        // When creating a voxel face we need to go through the VoxelData.faceVertices
        // array to find the indices for each vertex in the VoxelData.vertices array that correspond
        // to the correct position for our mesh's vertices.
        for (int j = 0; j < 6; j++)
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
                    // because we are done adding vertices
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
