using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    // Target object for the camera to look at
    public Transform target;

    // Rotation speed of the camera
    public float rotationSpeed = 1.0f;

    // Zoom speed of the camera
    public float zoomSpeed = 2.0f;

    // Minimum zoom distance allowed
    public float minZoomDistance = 1.0f;

    // Maximum zoom distance allowed
    public float maxZoomDistance = 10.0f;

    // Last recorded touch positions for calculating touch delta
    private Vector2[] lastTouchPositions = new Vector2[2];

    // Current distance of the camera from the target
    private float distance = 5.0f;

    void Start()
    {
        // Ensure the target is not null, and make the camera initially look at it
        if (target != null)
        {
            transform.LookAt(target);
        }
    }

    void Update()
    {
        try
        {
            // Check if there is a target assigned
            if (target == null)
            {
                return;
            }

            // Handle touch input
            if (Input.touchCount == 1)
            {
                // Single touch for rotation
                HandleOneFingerRotation(Input.GetTouch(0));
            }
            else if (Input.touchCount == 2)
            {
                // Two touches for pinch-to-zoom
                HandlePinchZoom(Input.GetTouch(0), Input.GetTouch(1));
            }

            // Update the camera position along the Z-axis based on the new distance
            Vector3 offset = transform.rotation * Vector3.forward * -distance;
            transform.position = target.position + offset;
        }
        catch(Exception ex)
        {

        }
    }

    void HandleOneFingerRotation(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            lastTouchPositions[0] = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            Vector2 deltaTouch = touch.position - lastTouchPositions[0];

            float horizontalInput = deltaTouch.x * rotationSpeed;
            float verticalInput = deltaTouch.y * rotationSpeed;

            // Rotate the camera around the target based on touch movement
            transform.RotateAround(target.position, Vector3.up, horizontalInput);
            transform.RotateAround(target.position, transform.right, -verticalInput);

            lastTouchPositions[0] = touch.position;
        }
    }

    void HandlePinchZoom(Touch touch1, Touch touch2)
    {
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        float touchDeltaMag = (touch1.position - touch2.position).magnitude;

        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        // Adjust the zoom distance based on pinch-to-zoom input, clamped within specified bounds
        distance += deltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
    }
}
