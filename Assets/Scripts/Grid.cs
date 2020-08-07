using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width = 16; // z will match x
    public int height = 1;

    public float size = 1; // in unity meters

    public Material mat;
    public GameObject hoverPreview;

    public bool preview = false; // Draws gizmo preview

    GameObject ground;
    Vector3 currentPos;
    bool validHover;

    CreateVoxel createVoxel;

    public CreateVoxel CreateVoxel { set { createVoxel = value;  } }

    // Start is called before the first frame update
    void Start()
    {
        CreateGroundPlane();
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
            validHover = true;
            hoverPreview.transform.position = currentPos;
        } else
        {
            validHover = false;
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
        Destroy(hoverPreview);
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
}
