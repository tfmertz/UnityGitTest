﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width = 16; // z will match x
    public int height = 3;
    public float size = 1; // in unity meters

    public Material mat;
    public CreateVoxel createVoxel;
    public Camera creatorCamera;
    public GameObject voxelParent;

    GameObject ground;
    Vector3 currentPos;
    bool validHover;
    Tool tool;

    private Touch theTouch;
    private bool isDragging;

    public CreateVoxel CreateVoxel { set { createVoxel = value;  } }

    // Start is called before the first frame update
    public void Init()
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
        GetGridPosition(Input.mousePosition);
    }

    private Vector3 GetGridPosition(Vector3 point)
    {
        // Raycast from mouse to detect hover
        RaycastHit hit;
        if (Physics.Raycast(creatorCamera.ScreenPointToRay(Input.mousePosition), out hit, 300.0f))
        {
            Vector3 hPoint = voxelParent.transform.InverseTransformPoint(hit.point);

            // Round the positioning down to snap
            int x = Mathf.FloorToInt(hPoint.x);
            int z = Mathf.FloorToInt(hPoint.z);
            int y = Mathf.FloorToInt(hPoint.y);

            
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
        
            validHover = true;
            tool.StartPreview(currentPos);
            currentPos = new Vector3(x + voxelParent.transform.position.x, voxelParent.transform.position.y, z + voxelParent.transform.position.z);

            return currentPos;
        } else
        {
            validHover = false;
            tool.StopPreview();
        }
        return Vector3.zero;
    }

    void DrawVoxel(Vector3 p)
    {
        if (validHover) createVoxel.Create(p);
    }

    void HandleInput()
    {
        Vector3 point = new Vector3();
        point = Input.mousePosition;
        if (Input.touchCount > 0)
        {
            theTouch = Input.GetTouch(0);

            if (theTouch.phase == TouchPhase.Began)
            {
                isDragging = true;
                //Debug.Log("Touch began:" + point);
                DrawVoxel(GetGridPosition(theTouch.position));

            }

            else if (theTouch.phase == TouchPhase.Moved)
            {
                isDragging = true;
                //Debug.Log("Touch Moved:" + point);
                DrawVoxel(GetGridPosition(theTouch.position));

            }
            else if (theTouch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
            point = Input.mousePosition;
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                DrawVoxel(GetGridPosition(point));
                //Debug.Log("MouseDown:" + point);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                //Debug.Log("point on drag:" + point);
                DrawVoxel(GetGridPosition(point));
            }
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
