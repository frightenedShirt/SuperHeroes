using UnityEngine;
using Mirror;
using StarterAssets;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCameraController : NetworkBehaviour
{
    public float lookSensitivity = 1;
    public Transform target;
    public Transform player;

    float mouseX;
    float mouseY;

    private StarterAssetsInputs _input;

    void Start()
    {
        _input = this.transform.parent.GetComponent<StarterAssetsInputs>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        ControlCamera();
    }

    void ControlCamera()
    {
        mouseX += _input.look.x * lookSensitivity;
        mouseY += _input.look.y * lookSensitivity;
        mouseY = Mathf.Clamp(mouseY, -20, 20);

        transform.LookAt(target);

        player.rotation = Quaternion.Euler(0, mouseX, 0);
        target.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
    }
}
