using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkh.CreatorEngine
{
    public class InteractionController : MonoBehaviour
    {
        public GameObject SpinableObject;

        public float SpinVelocity = 1;
        public float CameraZoomSpeedOrtho = .2f;
        public float CameraZoomSpeedPersp = .2f;
        public float CameraZoomSpeedWheel = .8f;
        public float rotSpeed = 50.0f;
        private bool isDragging;
        private bool isDrawing;
        public Grid theGrid;
        public Camera TheCamera;
        private bool mouseDown = false;
        private bool isWheelScrolling = false;
        private float timer = 0.0f;
        private float waitTime = 0.7f;
        private Vector3 oldPosition;
        private Vector3 oldNormal;
        private Vector3 heightLock;
        // 
        //Starting Camera Location
        //
        public Quaternion StartSpinPosition
        {
            get
            {
                return lastSpinPosition;
            }
        }
        private Quaternion lastSpinPosition = Quaternion.Euler(30, 45, 0);
        public Vector3 StartZoomPosition
        {
            get
            {
                return lastZoomPosition;
            }
        }
        private Vector3 lastZoomPosition = new Vector3(0, 0, -79);
        private Vector2 lastPosition;

        // Use this for initialization
        void Awake()
        {
            //if (!SpinableObject) SpinableObject = this.gameObject;
        }

        private void Update()
        {
            if (theGrid == null) return;

            Vector2 currentPosition = lastPosition;
            Vector2 deltaPositon = Vector2.zero;

            isScrolling();
            Vector3 point = Vector3.zero;
            if (!IsPointerOverUIObject())
            {
                if (Input.touchCount > 0)
                {
                    // Handle all Touch Events in Handle Touch
                    point = HandleTouch();
                }
                else if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                {
                    Zoom(Input.mouseScrollDelta.y);
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    if (mouseDown == false)
                    {
                        currentPosition = lastPosition = Input.mousePosition;
                        mouseDown = true;
                        TouchDown();

                        //theGrid.DrawVoxelOnMouseDown(Input.mousePosition);

                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    TouchUp();
                }

                if (isDragging)
                {
                    currentPosition = Input.mousePosition;
                    deltaPositon = currentPosition - lastPosition;
                    lastPosition = Input.mousePosition;

                    RotateObject();
                }
                if (isDrawing && !isDragging)
                {
                    theGrid.isDrawing = isDrawing;
                    Vector3? normal = theGrid.GetNormalDirection();
                    Vector3? position = theGrid.GetTruePosition();

                    if (normal.HasValue && position.HasValue)
                    {
                        Vector3 norm = (Vector3)normal;

                        Vector3 pos = theGrid.RoundByNormal((Vector3)position, norm);

                        if (pos != oldPosition && norm == oldNormal)
                        {
                            Debug.Log("normal:" + norm.y + " pos:" + pos.y + " lock:" + heightLock.y);

                            AddVoxelByNormal(norm, pos);

                            oldPosition = pos;
                            oldNormal = norm;
                        }
                    }
                }
            }
        }


        private Vector3 AddVoxelByNormal(Vector3 norm, Vector3 pos)
        {
            if (norm.y == 1 || norm.y == -1)
            {
                if (theGrid.Tool.AddDragPosition(new Vector3(pos.x, 0, pos.z)))
                    theGrid.DrawVoxelOnMouseDown(pos); Debug.Log("pos:" + pos);
            }
            else if (norm.z == -1 || norm.z == 1)
            {
                if (theGrid.Tool.AddDragPosition(new Vector3(pos.x, pos.y, 0)))
                    theGrid.DrawVoxelOnMouseDown(pos);
            }
            else if (norm.x == -1 || norm.x == 1)
            {
                if (theGrid.Tool.AddDragPosition(new Vector3(0, pos.x, pos.z)))
                    theGrid.DrawVoxelOnMouseDown(pos);
            }
            return pos;
        }

        private Vector3 ConvertNormal(Vector3? normal)
        {
            return (Vector3)normal;
        }
        private void isScrolling()
        {
            timer += Time.deltaTime;
            if (Input.mouseScrollDelta.y != 0)
            {
                isWheelScrolling = true;
                timer = 0.0f;
            }
            else
            {
                if (timer > waitTime)
                {
                    if (isWheelScrolling == true)
                    {
                        Debug.Log("Wheel Stopped, tracking undo state");
                        isWheelScrolling = false;
                        CheckZoomHasChanged();
                    }
                }
            }
        }

        private void CheckRotationHasChanged()
        {
            new UndoAction(UndoAction.Type.ROTATE, SpinableObject);

        }
        private void CheckZoomHasChanged()
        {
            new UndoAction(UndoAction.Type.ZOOM, TheCamera);
        }

        private Vector3 HandleTouch()
        {
            // Handle All touch Events
            Vector3 point;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            point = touchZero.position;
            // Get the position in the previous frame of each touch.
            Vector2 touchZeroPrev = touchZero.position - touchZero.deltaPosition;

            if (Input.touchCount == 1)
            {
                // Single finger Touches
                point = HandleOneFingerTouch(touchZero);
            }
            else if (Input.touchCount == 2)
            {
                // Double Finger Touch
                HandleTwoFingerTouch(touchZero, touchZeroPrev);
            }

            return point;
        }

        private Vector3 HandleOneFingerTouch(Touch touchZero)
        {
            Vector3 point = touchZero.position;
            if (touchZero.phase == TouchPhase.Began)
            {

                //isDrawing = true;

                TouchDown();
            }

            else if (touchZero.phase == TouchPhase.Moved)
            {
                //theGrid.DrawVoxelOnMouseDown(touchZero.position);
                // Allow Object to rotate
                //isDrawing = isDragging = true;
            }
            else if (touchZero.phase == TouchPhase.Ended)
            {
                TouchUp();

            }

            return point;
        }

        private void HandleTwoFingerTouch(Touch touchZero, Vector2 touchZeroPrev)
        {
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Get the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrev - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

            if (deltaMagnitudeDiff > 1 || deltaMagnitudeDiff < -1)
            {
                Zoom(deltaMagnitudeDiff);
            }
        }

        private void TouchDown()
        {
            if (!theGrid.validHover)
            {
                isDragging = true;
            }
            else
            {
                isDrawing = true;

                oldNormal = ConvertNormal(theGrid.GetNormalDirection());

                theGrid.Tool.ClearDragPositions();

                //heightLock = theGrid.RoundByNormal((Vector3) theGrid.GetTruePosition(), oldNormal);

                Debug.Log("x:" + oldNormal.x + "y:" + oldNormal.y + "z:" + oldNormal.z);
            }
            AddVoxelByNormal(oldNormal, Input.mousePosition);
        }

        private void TouchUp()
        {
            //Debug.Log("Up");
            theGrid.isDrawing = mouseDown = isDrawing = isDragging = false;
            CheckRotationHasChanged();
        }

        private void RotateObject()
        {

            float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

            // Work around to avoid Gimbal Lock
            // 
            SpinableObject.transform.Rotate(Vector3.up, rotX, Space.World);
            SpinableObject.transform.Rotate(Vector3.right, -rotY, Space.Self);


        }
        private void Zoom(float deltaMagnitudeDiff)
        {

            if (!TheCamera) TheCamera = GetComponent<Camera>();
            float cameraZ = TheCamera.transform.gameObject.transform.localPosition.z;
            cameraZ += deltaMagnitudeDiff * CameraZoomSpeedWheel;
            //Debug.Log(deltaMagnitudeDiff * CameraZoomSpeedOrtho);
            TheCamera.transform.localPosition = new Vector3(0, 0, cameraZ);
        }

        /// <summary> Checks if the the current input is over canvas UI </summary>
        public bool IsPointerOverUIObject()
        {

            if (EventSystem.current == null) return false;
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }

}
