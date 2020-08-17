using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    public class TouchController : MonoBehaviour
    {
        public GameObject SpinableObjectY;
        public GameObject SpinableObjectX;
        private Touch screenTouchOne;
        private Vector2 startPosition;
        public float SpinVelocity = 1;
        public float CameraZoomSpeedOrtho = .2f;
        public float CameraZoomSpeedPersp = .2f;
        public float CameraZoomSpeedWheel = .8f;
        public float rotSpeed = 30f;
        private bool isDragging;
        public Grid theGrid;
        public Camera camera;
        private bool mouseDown = false;

        private Vector2 lastPosition;

        // Use this for initialization
        void Awake()
        {
            //if (!SpinableObject) SpinableObject = this.gameObject;
        }

        private void Update()
        {
            Vector2 currentPosition = lastPosition;
            Vector2 deltaPositon = Vector2.zero;

            Vector3 point = Vector3.zero;

            if (Input.touchCount > 0)
            {
                point = HandleTouch();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                Zoom(Input.mouseScrollDelta.y);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Down");
                if (mouseDown == false)
                {
                    currentPosition = lastPosition = Input.mousePosition;
                    mouseDown = true;
                    isDragging = true;
                    theGrid.DrawVoxelOnMouseDown(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Up");
                isDragging = false;
                mouseDown = false;
            }

            if (isDragging)
            {
                currentPosition = Input.mousePosition;
                deltaPositon = currentPosition - lastPosition;
                lastPosition = Input.mousePosition;

                if (!theGrid.validHover) RotateObject();
                else if (deltaPositon.magnitude > 7 || deltaPositon.magnitude < -7)
                {
                    Debug.Log("Dragging:" + deltaPositon.magnitude);
                    //theGrid.DrawVoxelOnMouseDown(point);
                }
            }
        }

        private Vector3 HandleTouch()
        {
            Vector3 point;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            point = touchZero.position;
            // Get the position in the previous frame of each touch.
            Vector2 touchZeroPrev = touchZero.position - touchZero.deltaPosition;

            if (Input.touchCount == 1)
            {
                point = HandleOneFingerTouch(touchZero);
            }
            else if (Input.touchCount == 2)
            {
                HandleTwoFingerTouch(touchZero, touchZeroPrev);
            }
            else if (Input.touchCount == 1)
            {
                RotateObject();
            }

            return point;
        }

        private Vector3 HandleOneFingerTouch(Touch touchZero)
        {
            Vector3 point = touchZero.position;
            if (touchZero.phase == TouchPhase.Began)
            {
                theGrid.DrawVoxelOnMouseDown(touchZero.position);
            }

            else if (touchZero.phase == TouchPhase.Moved)
            {
                theGrid.DrawVoxelOnMouseDown(touchZero.position);
            }
            else if (touchZero.phase == TouchPhase.Ended)
            {
                isDragging = false;
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

        private void RotateObject()
        {
            float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

            SpinableObjectY.transform.Rotate(Vector3.up, rotX);
            SpinableObjectX.transform.Rotate(Vector3.right, -rotY);
        }
        private void Zoom(float deltaMagnitudeDiff)
        {

            if (!camera) camera = GetComponent<Camera>();
            float cameraZ = camera.transform.gameObject.transform.localPosition.z;
            cameraZ += deltaMagnitudeDiff * CameraZoomSpeedWheel;
            Debug.Log(deltaMagnitudeDiff * CameraZoomSpeedOrtho);
            camera.transform.localPosition = new Vector3(0, 0, cameraZ);
        }
    }

}