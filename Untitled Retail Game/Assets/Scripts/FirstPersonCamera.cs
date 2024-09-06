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

    private bool isEnabled;

    private void Start()
    {
        xRotation = orientation.rotation.eulerAngles.x;
        yRotation = orientation.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (isEnabled)
        {
            Vector2 mouseInput = GameInput.Instance.GetMouseDelta();
            float mouseX = mouseInput.x * sensX / 10; // divide by 10 just so sensitivity is a nicer number
            float mouseY = mouseInput.y * sensY / 10; // divide by 10 just so sensitivity is a nicer number

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        }

        orientation.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        transform.rotation = orientation.rotation;
        transform.position = cameraAnchor.position;
    }

    public void Enable()
    {
        isEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void Disable()
    {
        isEnabled = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

}
