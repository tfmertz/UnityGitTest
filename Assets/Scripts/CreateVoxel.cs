using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateVoxel : MonoBehaviour
{
    public Material mat;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    int vertIndex = 0;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //List<bool, bool, bool> voxelMap = new List<bool, bool, bool>();

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        Create(transform.position);
        Create(transform.position + Vector3.forward);
        Create(transform.position + Vector3.right);
        Create(transform.position + Vector3.forward + Vector3.right);
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

    void AddVoxel(Vector3 pos)
    {
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
