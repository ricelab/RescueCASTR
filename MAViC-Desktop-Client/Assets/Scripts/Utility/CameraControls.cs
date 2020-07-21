using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public enum CameraViewingMode
    {
        _2D,
        _3D
    };

    private Camera _camera;

    // Viewing mode
    public CameraViewingMode cameraViewingMode = CameraViewingMode._3D;
    private CameraViewingMode _oldCameraViewingMode;

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
    public float minimumOrthographicSize = 10.0f;
    public float maximumOrthographicSize = 100.0f;

    private float mouseRatioX;
    private float mouseRatioY;

    private bool rightMouseButtonStartedInScene = false;
    private bool leftMouseButtonStartedInScene = false;

    // Old rotation values in 3D mode (for when switching to 2D mode)
    private float _oldRotationX;
    private float _oldRotationY;
    private float _oldPositionZ;

    public void Start()
    {
        _camera = GetComponent<Camera>();

        if (cameraViewingMode == CameraViewingMode._2D)
            _oldCameraViewingMode = CameraViewingMode._3D;
        else
            _oldCameraViewingMode = CameraViewingMode._2D;
    }

    public void Update()
    {
        // If switching from 3D to 2D mode
        if (_oldCameraViewingMode == CameraViewingMode._3D && cameraViewingMode == CameraViewingMode._2D)
        {
            _oldCameraViewingMode = cameraViewingMode;

            _oldRotationX = transform.rotation.eulerAngles.x;
            _oldRotationY = transform.rotation.eulerAngles.y;
            _oldPositionZ = transform.position.z;

            transform.rotation = Quaternion.Euler(90.0f, 180.0f, transform.rotation.z);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);

            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.orthographic = true;
        }
        // If switching from 2D to 3D mode
        else if (_oldCameraViewingMode == CameraViewingMode._2D && cameraViewingMode == CameraViewingMode._3D)
        {
            _oldCameraViewingMode = cameraViewingMode;

            transform.rotation = Quaternion.Euler(_oldRotationX, _oldRotationY, transform.rotation.z);
            transform.position = new Vector3(transform.position.x, transform.position.y, _oldPositionZ);

            _camera.clearFlags = CameraClearFlags.Skybox;
            _camera.orthographic = false;
        }

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
            if (cameraViewingMode == CameraViewingMode._3D)
            {
                transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);
            }
            else // if (cameraViewingMode == CameraViewingMode._2D)
            {
                _camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            }
        }


        if (cameraViewingMode == CameraViewingMode._3D && Input.GetMouseButton(1) && rightMouseButtonStartedInScene)
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
        else if (transform.position.x > maximumX)
        {
            transform.position = new Vector3(maximumX, transform.position.y, transform.position.z);
        }

        if (transform.position.y < minimumY)
        {
            transform.position = new Vector3(transform.position.x, minimumY, transform.position.z);
        }
        else if (transform.position.y > maximumY)
        {
            transform.position = new Vector3(transform.position.x, maximumY, transform.position.z);
        }

        if (transform.position.z < minimumZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, minimumZ);
        }
        else if (transform.position.z > maximumZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, maximumZ);
        }

        if (_camera.orthographicSize > maximumOrthographicSize)
        {
            _camera.orthographicSize = maximumOrthographicSize;
        }
        else if (_camera.orthographicSize < minimumOrthographicSize)
        {
            _camera.orthographicSize = minimumOrthographicSize;
        }
    }
}
