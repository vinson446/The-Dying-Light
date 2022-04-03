using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 400;
    public float sensY = 400;

    float xRot;
    float yRot;

    public Transform orientation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseInput();
    }

    void GetMouseInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // calculate looking left and right
        yRot += mouseX;

        // calculate looking up and down
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);

        // looking left and right
        transform.rotation = Quaternion.Euler(xRot, yRot, 0);

        // set orientation obj's rotation for player obj to move towards camera's forward axis
        orientation.rotation = Quaternion.Euler(0, yRot, 0);
    }
}
