using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool
{
    public enum Tools { Add, Delete, Paint }
    public Tools activeTool;

    CreateVoxel currentVoxelCreator;
    GameObject AddPreview;
    Vector3 currentPosition;
    Vector3 lastPosition;

    public Tool(CreateVoxel createVoxel)
    {
        currentVoxelCreator = createVoxel;
        activeTool = Tools.Add;
        // Create add previewer
        Material preview = Resources.Load<Material>("Preview");

        AddPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        AddPreview.name = "AddPreview";
        AddPreview.GetComponent<MeshRenderer>().material = preview;
        GameObject.Destroy(AddPreview.GetComponent<BoxCollider>());
        AddPreview.SetActive(false);
    }

    public void SwitchTool(Tools tool)
    {
        activeTool = tool;
    }

    public void SetParent(GameObject vParent)
    {
        AddPreview.transform.SetParent(vParent.transform);
    }

    // Handles the voxel creator preview for the active tool
    public void StartPreview(Vector3 pos)
    {
        if (currentPosition.Equals(pos))
        {
            // bail out if the position didn't change
            return;
        }
        currentPosition = pos;
        Debug.Log("position:" + pos);
        switch (activeTool)
        {
            case Tools.Add:
                AddPreview.SetActive(true);
                AddPreview.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0.5f);
                break;
            default:
                Debug.Log("Default preview");
                AddPreview.SetActive(true);
                AddPreview.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0.5f);
                break;
        }
    }

    public void StopPreview()
    {
        //Debug.Log("FALSE");
        switch (activeTool)
        {
            case Tools.Add:
                AddPreview.SetActive(false);
                break;
            default:
                Debug.Log("Default preview");
                break;
        }
    }

    // Handles updating the voxel creation when a tool is used
    public void Apply()
    {

    }

    public void Cleanup()
    {
        GameObject.Destroy(AddPreview);
    }
}
