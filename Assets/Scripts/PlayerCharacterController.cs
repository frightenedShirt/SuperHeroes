using UnityEngine;
using Mirror;
using StarterAssets;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCharacterController : NetworkBehaviour
{
    public Animator animator;
    public float walkSpeed;
    public float runSpeed;

    private StarterAssetsInputs _input;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        //Move the player 
        Vector3 movement = new Vector3(_input.move.x, 0f, _input.move.y) * walkSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }
}
