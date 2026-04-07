using UnityEngine;

/// <summary>
/// A presentation layer utility that forces a World Space Canvas (e.g., enemy health bars)
/// to continually face the active camera, bypassing the rotation of its parent transform.
/// </summary>
public class UIBillboard : MonoBehaviour
{
    // --- COMPONENT CACHING ---
    private Camera _mainCamera;

    // ==============================================================================================================

    private void Start()
    {
        // Cache the main camera reference to avoid expensive lookups during runtime
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// Billboarding logic is executed in LateUpdate to ensure all movement and
    /// camera tracking computations have finished for the current frame.
    /// This strictly prevents visual stuttering or single-frame lag.
    /// </summary>
    private void LateUpdate()
    {
        if (_mainCamera != null )
        {
            // Synchronise rotation with the camera lens so the UI always renders flat to the screen
            transform.rotation = _mainCamera.transform.rotation;
        }
    }
}
