using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityFx.Outline;

public class CameraViewModeScript : MonoBehaviour
{
    private static CameraViewState _cameraViewState = CameraViewState.OnTop;

    protected Camera cameraPlayer;

    private enum CameraViewState
    {
        None,
        OnTop,
        OnDiagonal
    }


    private void Start()
    {
        cameraPlayer = GetComponent<Camera>();
    }

    public static bool OnDiagonalView() { if (_cameraViewState == CameraViewState.OnDiagonal) return true;
        else return false;
    }

    public void ChangeViewMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_cameraViewState == CameraViewState.OnTop) { _cameraViewState = CameraViewState.OnDiagonal; ChangeCameraMode(); }
            else { _cameraViewState = CameraViewState.OnTop; ChangeCameraMode(); }
        }
    }

    private void ChangeCameraMode()
    {
        if( _cameraViewState == CameraViewState.OnDiagonal)
        {
            transform.position = new Vector3(transform.position.x,350f,transform.position.z - 100f);
            transform.rotation = Quaternion.Euler(75, 0, 0);

            return;
        }
        if(_cameraViewState == CameraViewState.OnTop)
        {
            transform.position = new Vector3(transform.position.x, 400f, transform.position.z + 100f);
            transform.rotation = Quaternion.Euler(90, 0, 0);

            return;
        }
    }
}
