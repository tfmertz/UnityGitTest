using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorEngine
{
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
        public Camera creatorCamera;
        public GameObject voxelParent;

        public CreateVoxel CreateVoxel { set { createVoxel = value; } }

        private Touch theTouch;
        private bool isDragging;

        public void Init()
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
            Destroy(ground.GetComponent<MeshCollider>());
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
            GetGridPosition(Input.mousePosition);
        }

        private Vector3 GetGridPosition(Vector3 point)
        {
            RaycastHit hit;
            if (Physics.Raycast(creatorCamera.ScreenPointToRay(Input.mousePosition), out hit, 300.0f))
            {
                Vector3 p = voxelParent.transform.InverseTransformPoint(hit.point);
                //Debug.Log("hit");

                int x = Mathf.FloorToInt(p.x);
                int z = Mathf.FloorToInt(p.z);
                int hx = Mathf.FloorToInt(hit.point.x);
                int hz = Mathf.FloorToInt(hit.point.z);
                currentPos = new Vector3(x + voxelParent.transform.position.x, voxelParent.transform.position.y, z + voxelParent.transform.position.z);
                validHover = true;
                hoverPreview.transform.position = new Vector3(hx, transform.position.y, hz);
                return currentPos;
            }
            else
            {
                validHover = false;
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
                    //DrawVoxel(GetGridPosition(point));
                }
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
}