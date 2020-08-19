using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arkh.CreatorEngine
{
    public class Tool
    {
        public enum Tools { Add, Delete, Paint }
        public Tools activeTool;

        CreateVoxel currentVoxelCreator;
        GameObject AddPreview;
        public Vector3 currentPosition;
        Vector3 lastPosition;

        public Color color = Color.red;

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
            else
            {
                if (activeTool == Tools.Delete || activeTool == Tools.Paint)
                {
                    StopPreview();
                }
            }
            currentPosition = pos;

            switch (activeTool)
            {
                case Tools.Add:
                    AddPreview.SetActive(true);
                    AddPreview.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case Tools.Delete:
                    // Remove the voxel at the current position for the preview

                    int voxelIndex = currentVoxelCreator.GetVoxelFromPosition(pos);
                    if (voxelIndex == -1)
                    {
                        // bail if there's no voxel there
                        return;
                    }
                    Debug.Log($"Delete at {pos}");
                    List<Voxel> previewVoxels = new List<Voxel>(currentVoxelCreator.CurrentVoxels);
                    previewVoxels.RemoveAt(voxelIndex);
                    currentVoxelCreator.Preview(previewVoxels.ToArray());
                    break;
                case Tools.Paint:
                    int index = currentVoxelCreator.GetVoxelFromPosition(pos);
                    if (index == -1)
                    {
                        return;
                    }
                    // Deep copy the current array so we can change colors
                    List<Voxel> voxels = new List<Voxel>();
                    for (int i = 0; i < currentVoxelCreator.CurrentVoxels.Count; i++)
                    {
                        voxels.Add(currentVoxelCreator.CurrentVoxels[i].Clone());
                    }
                    voxels[index].color = color;
                    currentVoxelCreator.Preview(voxels.ToArray());
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
            switch (activeTool)
            {
                case Tools.Add:
                    AddPreview.SetActive(false);
                    break;
                case Tools.Delete:
                    currentVoxelCreator.StopPreview();
                    break;
                case Tools.Paint:
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
            switch (activeTool)
            {
                case Tools.Add:
                    // TODO take the logic out of the createvoxel and add it to the tool to manage
                    // CreateVoxel should be an API the tools use to manipulate the mesh
                    currentVoxelCreator.Create(currentPosition);
                    new UndoAction(UndoAction.Type.ADD, currentPosition);
                    break;
                case Tools.Delete:
                    int voxelIndex = currentVoxelCreator.GetVoxelFromPosition(currentPosition);
                    if (voxelIndex == -1)
                    {
                        Debug.Log($"Bailing for {currentPosition}");
                        return;
                    }

                    List<Voxel> voxels = new List<Voxel>(currentVoxelCreator.CurrentVoxels);
                    voxels.RemoveAt(voxelIndex);
                    currentVoxelCreator.Load(voxels.ToArray());
                    new UndoAction(UndoAction.Type.DELETE, currentPosition);
                    break;
                case Tools.Paint:
                    int index = currentVoxelCreator.GetVoxelFromPosition(currentPosition);
                    if (index == -1)
                    {
                        Debug.Log($"Bailing for {currentPosition}");
                        return;
                    }
                    new UndoAction(UndoAction.Type.PAINT, currentPosition, currentVoxelCreator.CurrentVoxels[index].color);
                    currentVoxelCreator.CurrentVoxels[index].color = color;
                    currentVoxelCreator.Load(currentVoxelCreator.CurrentVoxels.ToArray());
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
}