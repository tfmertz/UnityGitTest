using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width; // z will match x
    public int height;

    public float size; // in unity meters

    public Material mat;
    public GameObject hoverPreview;

    public bool preview; // Draws gizmo preview

    GameObject ground;
    Vector3 currentPos;

    CreateVoxel createVoxel;
    // Start is called before the first frame update
    void Start()
    {
        //CreateGrid();
        CreateGroundPlane();

        GameObject createVoxelObj = GameObject.Find("VoxelCreator");
        if (createVoxelObj != null)
        {
            createVoxel = createVoxelObj.GetComponent<CreateVoxel>();
        } else
        {
            Debug.LogError("VoxelCreator not found!");
        }
    }

    private void Update()
    {
        HandleInput();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleHover();
    }

    void CreateGrid()
    {
        

        // Go through x
        for (int i = 0; i < width; i++)
        {
            // Go through z
            for (int j = 0; j < width; j++)
            {
                // Start with the position of the grid gameobject
                Vector3 pos = new Vector3(transform.position.x + (i * size), transform.position.y, transform.position.z + (j * size));

                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }

    void CreateGroundPlane()
    {
        // Create the component in the heirarchy
        ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.transform.SetParent(transform);

        // Remove the meshcollider and add box collider
        Destroy(ground.GetComponent <MeshCollider>());
        ground.AddComponent<BoxCollider>();

        float quadWidth = width * size;
        float center = (width * size / 2);
        ground.transform.position = transform.position + new Vector3(center, 0, center);
        ground.transform.localScale = new Vector3(quadWidth, quadWidth, 0);
        ground.transform.Rotate(new Vector3(90, 0, 0));
        MeshRenderer meshRenderer = ground.GetComponent<MeshRenderer>();
        mat.mainTextureScale = new Vector2(width, width);
        meshRenderer.material = mat;
    }

    void CreateGridLines()
    {
        // Create lines
        for(int i = 0; i < width; i++)
        {

        }
    }

    /// <summary>
    /// Take a position vector and find the nearest grid item
    /// </summary>
    /// <param name="pos"></param>
    void FindNearestPoint(Vector3 pos)
    {

    }

    private void OnDrawGizmos()
    {
        if (!preview) return;

        CreateGrid();
    }

    void HandleHover()
    {
        // Raycast from mouse to detect hover
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 300.0f))
        {
            Debug.Log("hit");
            // Round the positioning down to snap
            int x = Mathf.FloorToInt(hit.point.x);
            int z = Mathf.FloorToInt(hit.point.z);
            currentPos = new Vector3(x, transform.position.y, z);
            hoverPreview.transform.position = currentPos;
        }
    }

    void HandleInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            createVoxel.Create(currentPos);
        }
    }
}
