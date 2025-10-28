using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDistanceScript : MonoBehaviour
{
    [Header("Максимальное приближение")]
    [SerializeField] private int minZoom;

    [Header("Максимальное отдаление")]
    [SerializeField] private int maxZoom;

    [Header("Сила изменения")]
    [SerializeField] private int scaleZoom;

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 inputVector2 = context.ReadValue<Vector2>();

            if(inputVector2.y < 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - scaleZoom, transform.position.z);

                CheckDistance(transform.position.y);
            }
            if(inputVector2.y > 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + scaleZoom, transform.position.z);

                CheckDistance(transform.position.y);
            }
        }
    }
    private void CheckDistance(float currentY)
    {
        if(currentY < minZoom) { transform.position = new Vector3(transform.position.x, minZoom, transform.position.z); }
        if(currentY > maxZoom) { transform.position = new Vector3(transform.position.x, maxZoom, transform.position.z); };
    }
}
