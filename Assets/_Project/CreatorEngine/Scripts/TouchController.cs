using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public GameObject SpinableObject;
    private Touch screenTouchOne;
    private Vector2 startPosition;
    public float SpinVelocity = 1;
    public float CameraZoomSpeedOrtho = .2f;
    public float CameraZoomSpeedPersp = .2f;
    public float CameraZoomSpeedWheel = .8f;
    public float rotSpeed = 30f;
    private bool isDragging;
    public Camera camera;

    // Use this for initialization
    void Awake()
    {
        if (!SpinableObject) SpinableObject = this.gameObject;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {

            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            // Get the position in the previous frame of each touch.
            Vector2 touchZeroPrev = touchZero.position - touchZero.deltaPosition;

            if (Input.touchCount == 2)
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
            else if (Input.touchCount == 1)
            {
                RotateObject();
            }

        } else if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
            Zoom(Input.mouseScrollDelta.y);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            RotateObject();
        }
    }
    
    private void RotateObject()
    {
        float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
        float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad ;

        SpinableObject.transform.Rotate(Vector3.up, rotX);
        SpinableObject.transform.Rotate(Vector3.right, -rotY);
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



