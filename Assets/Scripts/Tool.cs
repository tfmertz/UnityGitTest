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

    // Handles the voxel creator preview for the active tool
    public void StartPreview(Vector3 pos)
    {
        if (currentPosition.Equals(pos))
        {
            // bail out if the position didn't change
            return;
        } else
        {
            if (activeTool == Tools.Delete)
            {
                StopPreview();
            }
        }
        currentPosition = pos;

        switch (activeTool)
        {
            case Tools.Add:
                AddPreview.SetActive(true);
                AddPreview.transform.position = pos + new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case Tools.Delete:
                // Remove the voxel at the current position for the preview
                Debug.Log($"Delete at {pos}");
                int voxelIndex = currentVoxelCreator.GetVoxelFromPosition(pos);
                if (voxelIndex == -1)
                {
                    // bail if there's no voxel there
                    return;
                }
                List<Voxel> previewVoxels = new List<Voxel>(currentVoxelCreator.CurrentVoxels);
                previewVoxels.RemoveAt(voxelIndex);
                currentVoxelCreator.Preview(previewVoxels.ToArray());
                break;
            default:
                Debug.Log("Default preview");
                AddPreview.SetActive(true);
                AddPreview.transform.position = pos + new Vector3(0.5f, 0.5f, 0.5f);
                break;
        }
    }

    public void StopPreview()
    {
        Debug.Log("FALSE");
        switch (activeTool)
        {
            case Tools.Add:
                AddPreview.SetActive(false);
                break;
            case Tools.Delete:
                currentVoxelCreator.StopPreview();
                break;
            default:
                Debug.Log("Default preview");
                break;
        }
    }

    // Handles updating the voxel creation when a tool is used
    public void Apply()
    {
        switch(activeTool)
        {
            case Tools.Add:
                // TODO take the logic out of the createvoxel and add it to the tool to manage
                // CreateVoxel should be an API the tools use to manipulate the mesh
                currentVoxelCreator.Create(currentPosition);
                break;
            case Tools.Delete:
                int voxelIndex = currentVoxelCreator.GetVoxelFromPosition(currentPosition);
                if (voxelIndex == -1)
                {
                    return;
                }

                List<Voxel> voxels = new List<Voxel>(currentVoxelCreator.CurrentVoxels);
                voxels.RemoveAt(voxelIndex);
                currentVoxelCreator.Load(voxels.ToArray());
                break;
            default:
                Debug.Log("No tools matches to apply");
                break;
        }
    }

    public void Cleanup()
    {
        GameObject.Destroy(AddPreview);
    }
}
