using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds static data on the configuration of the voxel, so that we can use other tools
/// to create voxels using this data as a starting point.
/// </summary>
public static class VoxelData
{
    public static readonly Vector3[] vertices = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    public static readonly Vector3[] faceChecks =
    {
        new Vector3(0.0f, 0.0f, 1.0f), // check front
        new Vector3(0.0f, 0.0f, -1.0f), // check back
        new Vector3(0.0f, 1.0f, 0.0f), // top
        new Vector3(0.0f, -1.0f, 0.0f), // bottom
        new Vector3(1.0f, 0.0f, 0.0f), // right
        new Vector3(-1.0f, 0.0f, 0.0f), // left
    };

    // Each outer array holds the index of the vertices inside vertices array that make
    // up the face. There are two tris made for each outer array, but some of the vertices
    // are reused. For example, faceVertices[0] holds two tris to make up the top face:
    //
    // vertices[3], vertices[7], and vertices[2] then vertices[7], vertices[6], vertices[2]
    // Ordering of these are important for each face needs to be constructed the same way for
    // the texturing to be mapped correctly.
    public static readonly int[,] faceVertices =
    {
        { 0, 3, 1, 2 }, // front
        { 5, 6, 4, 7 }, // back
        { 3, 7, 2, 6 }, // top
        { 4, 0, 5, 1 }, // bottom
        { 1, 2, 5, 6 }, // right
        { 4, 7, 0, 3 }, // left
    };

    public static readonly Vector2[] uvs =
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };
}
