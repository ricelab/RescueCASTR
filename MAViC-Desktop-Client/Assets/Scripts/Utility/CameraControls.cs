using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private float _rotateX;
    private float _rotateY;

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

    private float _mouseRatioX;
    private float _mouseRatioY;

    private bool _rightMouseButtonStartedInScene = false;
    private bool _leftMouseButtonStartedInScene = false;

    // Old rotation values in 3D mode (for when switching to 2D mode)
    private float _oldRotationX;
    private float _oldRotationY;
    private float _oldPositionZ;

    // Camera viewing mode toggle button
    public GameObject toggleButtonTextObj;

    // For maintaining consistent control between varying screen sizes
    public MainController mainController;
    private GameObject _wholeScreenUiObj;
    private RectTransform _wholeScreenUiCanvasRect;
    private Vector2 _referencePosition;

    public void Start()
    {
        _camera = GetComponent<Camera>();

        if (cameraViewingMode == CameraViewingMode._2D)
            _oldCameraViewingMode = CameraViewingMode._3D;
        else
            _oldCameraViewingMode = CameraViewingMode._2D;

        _wholeScreenUiObj = mainController.wholeScreenUiObj;
        _wholeScreenUiCanvasRect = _wholeScreenUiObj.GetComponent<RectTransform>();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            rotateSpeed /= 2.0f;
            dragSpeed /= 2.0f;
            zoomSpeed /= 2.0f;
        }
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

            toggleButtonTextObj.GetComponent<Text>().text = "3D";
        }
        // If switching from 2D to 3D mode
        else if (_oldCameraViewingMode == CameraViewingMode._2D && cameraViewingMode == CameraViewingMode._3D)
        {
            _oldCameraViewingMode = cameraViewingMode;

            transform.rotation = Quaternion.Euler(_oldRotationX, _oldRotationY, transform.rotation.z);
            transform.position = new Vector3(transform.position.x, transform.position.y, _oldPositionZ);

            _camera.clearFlags = CameraClearFlags.Skybox;
            _camera.orthographic = false;

            toggleButtonTextObj.GetComponent<Text>().text = "2D";
        }

        _referencePosition = new Vector2(
            Input.mousePosition.x / _wholeScreenUiObj.GetComponent<Canvas>().pixelRect.size.x * _wholeScreenUiObj.GetComponent<CanvasScaler>().referenceResolution.x,
            Input.mousePosition.y / _wholeScreenUiObj.GetComponent<Canvas>().pixelRect.size.y * _wholeScreenUiObj.GetComponent<CanvasScaler>().referenceResolution.y
        );

        _mouseRatioX = _referencePosition.x / _wholeScreenUiCanvasRect.sizeDelta.x;
        _mouseRatioY = _referencePosition.y / _wholeScreenUiCanvasRect.sizeDelta.y;
        
        // Zoom
        if (_mouseRatioX < 0.75 && _mouseRatioY > 0.2)
        {
            if (Input.GetMouseButtonDown(1))
            {
                _rightMouseButtonStartedInScene = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                _leftMouseButtonStartedInScene = true;
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


        // Rotate
        if (cameraViewingMode == CameraViewingMode._3D && Input.GetMouseButton(1) && _rightMouseButtonStartedInScene)
        {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotateSpeed, -Input.GetAxis("Mouse X") * rotateSpeed, 0));
            _rotateX = transform.rotation.eulerAngles.x;
            _rotateY = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(_rotateX, _rotateY, 0);
        }
        
        // Drag
        if (Input.GetMouseButton(0) && _leftMouseButtonStartedInScene)
        {
            if (cameraViewingMode == CameraViewingMode._3D)
            {
                transform.Translate(new Vector3(
                    -Input.GetAxis("Mouse X") * dragSpeed * 1.0f / 50.0f * (transform.position.y - minimumY + 10.0f),
                    -Input.GetAxis("Mouse Y") * dragSpeed * 1.0f / 50.0f * (transform.position.y - minimumY + 10.0f),
                    0));
            }
            else // if (cameraViewingMode == CameraViewingMode._2D)
            {
                transform.Translate(new Vector3(
                    -Input.GetAxis("Mouse X") * dragSpeed * 1.0f / 50.0f * _camera.orthographicSize,
                    -Input.GetAxis("Mouse Y") * dragSpeed * 1.0f / 50.0f * _camera.orthographicSize,
                    0));
            }
        }


        if (Input.GetMouseButtonUp(1))
        {
            _rightMouseButtonStartedInScene = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _leftMouseButtonStartedInScene = false;
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

    public void ToggleCameraViewingMode()
    {
        if (cameraViewingMode == CameraViewingMode._2D)
        {
            cameraViewingMode = CameraViewingMode._3D;
        }
        else
        {
            cameraViewingMode = CameraViewingMode._2D;
        }
    }
}
