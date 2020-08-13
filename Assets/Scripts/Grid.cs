using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width = 16; // z will match x
    public int height = 3;

    public float size = 1; // in unity meters

    public Material mat;

    GameObject ground;
    Vector3 currentPos;
    bool validHover;

    CreateVoxel createVoxel;
    Tool tool;

    public CreateVoxel CreateVoxel { set { createVoxel = value;  } }

    // Start is called before the first frame update
    void Start()
    {
        CreateGroundPlane();
        tool = new Tool(createVoxel);
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

    void CreateGroundPlane()
    {
        // Create the component in the heirarchy
        ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.transform.SetParent(transform);

        // Remove the cooking options
        ground.GetComponent<MeshCollider>().cookingOptions -= MeshColliderCookingOptions.CookForFasterSimulation;
        //Destroy(ground.GetComponent <MeshCollider>());
        //ground.AddComponent<BoxCollider>();

        float quadWidth = width * size;
        float center = (width * size / 2);
        ground.transform.position = transform.position + new Vector3(center, 0, center);
        ground.transform.localScale = new Vector3(quadWidth, quadWidth, 0);
        ground.transform.Rotate(new Vector3(90, 0, 0));
        MeshRenderer meshRenderer = ground.GetComponent<MeshRenderer>();
        mat.mainTextureScale = new Vector2(width, width);
        meshRenderer.material = mat;
    }

    void HandleHover()
    {
        // Raycast from mouse to detect hover
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 300.0f))
        {
            // Round the positioning down to snap
            int x = Mathf.FloorToInt(hit.point.x);
            int z = Mathf.FloorToInt(hit.point.z);
            int y = Mathf.FloorToInt(hit.point.y);
            Debug.Log($"hit: {hit.point.y}, rounded: {y}, normal: {hit.normal}");
            if (y >= height)
            {
                // restrict y based off our predetermined height 
                y = height - 1;
            }
            // In case we accidentally get a tiny number smaller than 0
            if (y < 0) y = 0;

            // Check normals for adding on sides
            if (tool.activeTool == Tool.Tools.Add)
            {
                if (hit.normal.Equals(Vector3.right) || hit.normal.Equals(-Vector3.right))
                {
                    x += (int) hit.normal.x;
                } else if(hit.normal.Equals(Vector3.forward) || hit.normal.Equals(-Vector3.forward))
                {
                    z += (int) hit.normal.z;
                }
            }
            else if (tool.activeTool == Tool.Tools.Delete)
            {
                if (hit.normal.Equals(Vector3.up) && y > 0)
                {
                    y--;
                }
            }

            currentPos = new Vector3(x, y, z);
            validHover = true;
            tool.StartPreview(currentPos);
        } else
        {
            validHover = false;
            tool.StopPreview();
        }
    }

    void HandleInput()
    {
        if (Input.GetButtonDown("Fire1") && validHover)
        {
            createVoxel.Create(currentPos);
        }
    }

    // Clean up the voxelCreator script and hoverPreview
    void OnDestroy()
    {
        Destroy(createVoxel.gameObject);
        createVoxel = null;
        tool.Cleanup();
    }

    public void Save(Creation creation)
    {
        creation.voxels = createVoxel.CurrentVoxels.ToArray();
    }

    public void Load(Creation creation)
    {
        createVoxel.Load(creation.voxels);
    }

    public void Undo()
    {
        createVoxel.Undo();
    }

    public void SwitchTool(Tool.Tools toolEnum)
    {
        tool.SwitchTool(toolEnum);
    }
}
