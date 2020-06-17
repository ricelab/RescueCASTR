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
    public float zoomSpeed = 40.0f;

    private float mouseRatioX;
    //private float mouseRatioY;

    private bool rightMouseButtonStartedInScene = false;
    private bool leftMouseButtonStartedInScene = false;

    // Update is called once per frame
    void Update()
    {
        mouseRatioX = Input.mousePosition.x / Screen.width;
        //mouseRatioY = Input.mousePosition.y / Screen.height;
        
        if (mouseRatioX < 0.75)
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
    }
}
