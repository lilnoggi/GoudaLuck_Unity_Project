using UnityEngine;

/// <summary>
/// Forces a World Space UI element to always face the main camera,
/// ignoring the rotation of its parent object.
/// </summary>

public class UIBillboard : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCamera != null )
        {
            // Match the camera's exact rotation so it always lays flat on the screen
            transform.rotation = _mainCamera.transform.rotation;
        }
    }
}
