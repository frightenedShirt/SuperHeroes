using UnityEngine;
using Mirror;
using StarterAssets;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCameraController : NetworkBehaviour
{
    //turning speed of camera
    public float lookSensitivity = 1;
    public Transform target;
    public Transform player;

    //input values from mouse
    float mouseX;
    float mouseY;

    private StarterAssetsInputs _input;

    void Start()
    {
        _input = this.transform.parent.GetComponent<StarterAssetsInputs>();

        //hiding the cursor during runtime
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        ControlCamera();
    }

    void ControlCamera()
    {
        mouseX += _input.look.x;
        mouseY += _input.look.y;
        mouseY = Mathf.Clamp(mouseY, -35, 60);

        transform.LookAt(target);

        //rotate the player
        player.rotation = Quaternion.Euler(0, mouseX, 0);
        //rotate the camera
        target.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
    }
}
