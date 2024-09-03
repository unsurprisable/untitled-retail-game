using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraAnchor;

    [SerializeField] private float maxLookAngle;

    private float xRotation;
    private float yRotation;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void LateUpdate()
    {
        Vector2 mouseInput = GameInput.Instance.GetMouseDelta();
        float mouseX = mouseInput.x * sensX / 10; // divide by 10 just so sensitivity is a nicer number
        float mouseY = mouseInput.y * sensY / 10; // divide by 10 just so sensitivity is a nicer number

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        transform.position = cameraAnchor.position;
    }
}
