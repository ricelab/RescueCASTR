using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    // Rotation
    public float rotateSpeed = -3.0f;
    private float X;
    private float Y;

    // Dragging
    public float dragSpeed = 12.0f;

    // Zooming
    public float zoomSpeed = 120.0f;

    // Camera position constraints
    public float mimimumX = -650.0f;
    public float maximumX = 700.0f;
    public float minimumY = -110.0f;
    public float maximumY = 500.0f;
    public float minimumZ = -650.0f;
    public float maximumZ = 550.0f;

    private float mouseRatioX;
    private float mouseRatioY;

    private bool rightMouseButtonStartedInScene = false;
    private bool leftMouseButtonStartedInScene = false;

    // Update is called once per frame
    void Update()
    {
        mouseRatioX = Input.mousePosition.x / Screen.width;
        mouseRatioY = Input.mousePosition.y / Screen.height;
        
        if (mouseRatioX < 0.75 && mouseRatioY > 0.2)
        {
            if (Input.GetMouseButtonDown(1))
            {
                rightMouseButtonStartedInScene = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                leftMouseButtonStartedInScene = true;
            }

            // Can adjust zoom based on scroll wheel
            transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);
        }


        if (Input.GetMouseButton(1) && rightMouseButtonStartedInScene)
        {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotateSpeed, -Input.GetAxis("Mouse X") * rotateSpeed, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);
        }
        
        if (Input.GetMouseButton(0) && leftMouseButtonStartedInScene)
        {
            transform.Translate(new Vector3(-Input.GetAxis("Mouse X") * dragSpeed, -Input.GetAxis("Mouse Y") * dragSpeed, 0));
        }


        if (Input.GetMouseButtonUp(1))
        {
            rightMouseButtonStartedInScene = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            leftMouseButtonStartedInScene = false;
        }

        // Keep camera position within constraints

        if (transform.position.x < mimimumX)
        {
            transform.position = new Vector3(mimimumX, transform.position.y, transform.position.z);
        }
        if (transform.position.x > maximumX)
        {
            transform.position = new Vector3(maximumX, transform.position.y, transform.position.z);
        }

        if (transform.position.y < minimumY)
        {
            transform.position = new Vector3(transform.position.x, minimumY, transform.position.z);
        }
        if (transform.position.y > maximumY)
        {
            transform.position = new Vector3(transform.position.x, maximumY, transform.position.z);
        }

        if (transform.position.z < minimumZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, minimumZ);
        }
        if (transform.position.z > maximumZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, maximumZ);
        }
    }
}
