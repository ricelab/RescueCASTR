using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CameraDefaultsAndConstraints
{
    public float rotateSpeed;
    public float dragSpeed;
    public float zoomSpeed;
    public float mimimumX;
    public float maximumX;
    public float minimumY;
    public float maximumY;
    public float minimumZ;
    public float maximumZ;
    public float minimumOrthographicSize;
    public float maximumOrthographicSize;
    public float defaultOrthographicSize;
    public Vector3 defaultPosition;
}

public class CameraControls : MonoBehaviour
{
    public enum CameraViewingMode
    {
        _2D,
        _3D
    };

    private Camera _camera;

    public MainController mainController;

    // Viewing mode
    public CameraViewingMode cameraViewingMode = CameraViewingMode._3D;
    private CameraViewingMode _oldCameraViewingMode;

    // Rotation
    private float _rotateX;
    private float _rotateY;

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
    private GameObject _wholeScreenUiObj;
    private RectTransform _wholeScreenUiCanvasRect;
    private Vector2 _referencePosition;

    private CameraDefaultsAndConstraints _cameraDefaultsAndConstraints;

    public void Start()
    {
        _camera = GetComponent<Camera>();

        _cameraDefaultsAndConstraints = mainController.map.cameraDefaultsAndConstraints;

        _cameraDefaultsAndConstraints.rotateSpeed *= -1.0f;

        if (cameraViewingMode == CameraViewingMode._2D)
            _oldCameraViewingMode = CameraViewingMode._3D;
        else
            _oldCameraViewingMode = CameraViewingMode._2D;

        _wholeScreenUiObj = mainController.wholeScreenUiObj;
        _wholeScreenUiCanvasRect = _wholeScreenUiObj.GetComponent<RectTransform>();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            _cameraDefaultsAndConstraints.rotateSpeed /= 2.0f;
            _cameraDefaultsAndConstraints.dragSpeed /= 2.0f;
            _cameraDefaultsAndConstraints.zoomSpeed /= 2.0f;
        }

        //// Set starting orthographic size, based on min and max orthographic sizes
        //_camera.orthographicSize = _cameraDefaultsAndConstraints.maximumOrthographicSize -
        //    0.5f * (_cameraDefaultsAndConstraints.maximumOrthographicSize - _cameraDefaultsAndConstraints.minimumOrthographicSize);

        // Set camera position and zoom to the map's defaults
        _camera.orthographicSize = _cameraDefaultsAndConstraints.defaultOrthographicSize;
        transform.position = _cameraDefaultsAndConstraints.defaultPosition;
    }

    public void Update()
    {
        // If switching from 3D to 2D mode
        if (_oldCameraViewingMode == CameraViewingMode._3D && cameraViewingMode == CameraViewingMode._2D)
        {
            _oldCameraViewingMode = cameraViewingMode;

            _oldRotationX = 45.0f;
            _oldRotationY = transform.rotation.eulerAngles.y;
            _oldPositionZ = transform.position.z;

            transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
            transform.position = new Vector3(transform.position.x, -20.0f, transform.position.z);

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


        if (mainController.fullscreenView == null)
        {
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
                    transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * _cameraDefaultsAndConstraints.zoomSpeed);
                }
                else // if (cameraViewingMode == CameraViewingMode._2D)
                {
                    _camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * _cameraDefaultsAndConstraints.zoomSpeed;
                }
            }


            // Rotate
            if (cameraViewingMode == CameraViewingMode._3D && Input.GetMouseButton(1) && _rightMouseButtonStartedInScene)
            {
                transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * _cameraDefaultsAndConstraints.rotateSpeed, -Input.GetAxis("Mouse X") * _cameraDefaultsAndConstraints.rotateSpeed, 0));
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
                        -Input.GetAxis("Mouse X") * _cameraDefaultsAndConstraints.dragSpeed * 1.0f / 50.0f * (transform.position.y - _cameraDefaultsAndConstraints.minimumY + 10.0f),
                        -Input.GetAxis("Mouse Y") * _cameraDefaultsAndConstraints.dragSpeed * 1.0f / 50.0f * (transform.position.y - _cameraDefaultsAndConstraints.minimumY + 10.0f),
                        0));
                }
                else // if (cameraViewingMode == CameraViewingMode._2D)
                {
                    transform.Translate(new Vector3(
                        -Input.GetAxis("Mouse X") * _cameraDefaultsAndConstraints.dragSpeed * 1.0f / 50.0f * _camera.orthographicSize,
                        -Input.GetAxis("Mouse Y") * _cameraDefaultsAndConstraints.dragSpeed * 1.0f / 50.0f * _camera.orthographicSize,
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

            if (transform.position.x < _cameraDefaultsAndConstraints.mimimumX)
            {
                transform.position = new Vector3(_cameraDefaultsAndConstraints.mimimumX, transform.position.y, transform.position.z);
            }
            else if (transform.position.x > _cameraDefaultsAndConstraints.maximumX)
            {
                transform.position = new Vector3(_cameraDefaultsAndConstraints.maximumX, transform.position.y, transform.position.z);
            }

            if (transform.position.y < _cameraDefaultsAndConstraints.minimumY)
            {
                transform.position = new Vector3(transform.position.x, _cameraDefaultsAndConstraints.minimumY, transform.position.z);
            }
            else if (transform.position.y > _cameraDefaultsAndConstraints.maximumY)
            {
                transform.position = new Vector3(transform.position.x, _cameraDefaultsAndConstraints.maximumY, transform.position.z);
            }

            if (transform.position.z < _cameraDefaultsAndConstraints.minimumZ)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, _cameraDefaultsAndConstraints.minimumZ);
            }
            else if (transform.position.z > _cameraDefaultsAndConstraints.maximumZ)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, _cameraDefaultsAndConstraints.maximumZ);
            }

            if (_camera.orthographicSize > _cameraDefaultsAndConstraints.maximumOrthographicSize)
            {
                _camera.orthographicSize = _cameraDefaultsAndConstraints.maximumOrthographicSize;
            }
            else if (_camera.orthographicSize < _cameraDefaultsAndConstraints.minimumOrthographicSize)
            {
                _camera.orthographicSize = _cameraDefaultsAndConstraints.minimumOrthographicSize;
            }
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
