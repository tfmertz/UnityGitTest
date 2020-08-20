using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    public class Grid : MonoBehaviour
    {
        public int width = 16; // z will match x
        public int height = 3;
        public float size = 1; // in unity meters

        public Material gridMat;
        public Material gridSidesMat;
        public CreateVoxel createVoxel;
        //
        //
        public Camera creatorCamera;
        public GameObject voxelParent;

        List<GameObject> planes;
        Vector3 currentPos;
        public bool validHover;
        Tool tool;

        private Touch theTouch;
        private bool isDragging;

        public CreateVoxel CreateVoxel { set { createVoxel = value; } }

        // Start is called before the first frame update
        public void Start()
        {
            gridMat = Resources.Load<Material>("Grid");
            gridSidesMat = Resources.Load<Material>("GridSides");
            planes = new List<GameObject>();
            CreateGrid();
            tool = new Tool(createVoxel);
            UndoAction.theTool = tool;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            HandleHover();
        }
        public void ChangeGridSize(int w = 16, int h = 16)
        {
            width = w;
            height = h;
            CreateGrid();
            createVoxel.Load(createVoxel.CurrentVoxels.ToArray());
        }

        void CreateGrid()
        {
            // Destroy our existing planes if any
            foreach(GameObject plane in planes)
            {
                Destroy(plane);
            }
            planes = new List<GameObject>(); // for 6 sides of a cube

            // Set up new grid values
            float quadWidth = width * size;
            float heightCenter = height * size / 2;
            float widthCenter = width * size / 2;

            // Set up the material tiling
            gridMat.mainTextureScale = new Vector2(width, width);
            gridSidesMat.mainTextureScale = new Vector2(width, height);

            // Create ground
            Vector3 pos = transform.position + new Vector3(widthCenter, 0, widthCenter);
            Vector3 scale = new Vector3(quadWidth, quadWidth, 0);
            Vector3 rot = new Vector3(90.0f, 0, 0);
            CreatePlane("ground", pos, scale, rot, gridMat);

            // Top plane
            pos = transform.position + new Vector3(widthCenter, height, widthCenter);
            scale = new Vector3(quadWidth, quadWidth, 0);
            rot = new Vector3(-90.0f, 0, 0);
            CreatePlane("top", pos, scale, rot, gridMat);

            // Back plane
            pos = transform.position + new Vector3(widthCenter, heightCenter, width);
            scale = new Vector3(quadWidth, height, 0);
            rot = Vector3.zero;
            CreatePlane("back", pos, scale, rot, gridSidesMat);

            // Front plane
            pos = transform.position + new Vector3(widthCenter, heightCenter, 0);
            scale = new Vector3(quadWidth, height, 0);
            rot = new Vector3(0, 180.0f, 0);
            CreatePlane("front", pos, scale, rot, gridSidesMat);

            // Right plane
            pos = transform.position + new Vector3(width, heightCenter, widthCenter);
            scale = new Vector3(quadWidth, height, 0);
            rot = new Vector3(0, 90.0f, 0);
            CreatePlane("right", pos, scale, rot, gridSidesMat);

            // Left plane
            pos = transform.position + new Vector3(0, heightCenter, widthCenter);
            scale = new Vector3(quadWidth, height, 0);
            rot = new Vector3(0, -90.0f, 0);
            CreatePlane("left", pos, scale, rot, gridSidesMat);

            // Reset the grid's position to be the center
            transform.position = new Vector3(-widthCenter, 0, -widthCenter);
        }

        void CreatePlane(string name, Vector3 pos, Vector3 scale, Vector3 rot, Material mat)
        {
            // Create the component in the heirarchy
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.name = name;
            plane.transform.SetParent(transform);

            // Remove the cooking options
            plane.GetComponent<MeshCollider>().cookingOptions -= MeshColliderCookingOptions.CookForFasterSimulation;
            
            plane.transform.position = pos;
            plane.transform.localScale = scale;
            plane.transform.Rotate(rot);
            MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
            meshRenderer.material = mat;

            planes.Add(plane);
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
                // translate our hit into localposition
                Vector3 hPoint = transform.InverseTransformPoint(hit.point);

                // Snap the ray hit to the grid
                int x = Mathf.FloorToInt(hPoint.x);
                int y = Mathf.FloorToInt(hPoint.y);
                int z = Mathf.FloorToInt(hPoint.z);

                // For some reason the hit point sometimes is just under the integer increment
                // so it incorrectly rounds down. So, we check the face normal and
                // round depending on the face we're touching
                if (hit.normal.y == 1 || hit.normal.y == -1)
                {
                    Debug.Log($"Y face");
                    y = Mathf.RoundToInt(hPoint.y);
                    if (hit.normal.y > 0 && y > 0) y--;
                    if (hit.normal.y < 0 && y == height) y--;
                } else if (hit.normal.x == 1 || hit.normal.x == -1)
                {
                    Debug.Log($"X face");
                    x = Mathf.RoundToInt(hPoint.x);
                    if (hit.normal.x > 0 && x > 0) x--;
                    if (hit.normal.x < 0 && x == width) x--;
                } else if (hit.normal.z == 1 || hit.normal.z == -1)
                {
                    Debug.Log($"Z face");
                    z = Mathf.RoundToInt(hPoint.z);
                    if (hit.normal.z > 0 && z > 0) z--;
                    if (hit.normal.z < 0 && z == width) z--;
                }

                // Adjust our position based on the tool type and then the local position of grid
                //CheckNormalsForSides(hit, ref x, ref z, ref y);
                currentPos = new Vector3(x, y, z);

                //Debug.Log($"hit: {hit.point.y}, rounded: {y}, normal: {hit.normal}");
                if (CheckValidGridPosition(currentPos))
                {
                    validHover = true;
                    tool.StartPreview(currentPos);
                    Debug.Log($"Valid position: {x}, {y}, {z}");
                } else
                {
                    Debug.Log($"Invalid position: {x}, {y}, {z}, {currentPos}");
                    validHover = false;
                    // Set the vector to a position that should be impossible to hover
                    tool.StopPreview();
                }
                return currentPos;
            }
            else
            {
                validHover = false;
                tool.StopPreview();
            }
            return Vector3.zero;
        }

        private void CheckNormalsForSides(RaycastHit hit, ref int x, ref int z, ref int y)
        {
            // Check normals for adding on sides
            if (tool.activeTool == Tool.Tools.Add)
            {
                if (hit.normal.x == 1 || hit.normal.x == -1)
                {
                    x += (int)hit.normal.x;
                }
                else if (hit.normal.z == 1 || hit.normal.z == -1)
                {
                    z += (int)hit.normal.z;
                }
                else if (y > 0 && (hit.normal.y == 1 || hit.normal.y == -1))
                {
                    y += (int)hit.normal.y;
                }
            }
            else if (tool.activeTool == Tool.Tools.Delete)
            {
                //if (hit.normal.y == 1 || hit.normal.y == -1)
                //{
                //    y += (int) hit.normal.y;
                //}
            }
        }

        public void DrawVoxel(Vector3 p)
        {
            if (validHover)
            {
                tool.Apply(currentPos);
            }
        }

        public void DrawVoxelOnMouseDown(Vector3 point)
        {

            DrawVoxel(GetGridPosition(point));
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

        public bool CheckValidGridPosition(Vector3 point)
        {
            float y = point.y;
            float x = point.x;
            float z = point.z;
            if (y < 0 || y > height || x < 0 || x > width || z < 0 || z > width)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}