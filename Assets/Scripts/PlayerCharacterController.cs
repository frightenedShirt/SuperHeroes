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
    public float JumpHeight = 1.2f;

    public Transform groundCheck;
    public LayerMask groundLayers;

    private Rigidbody rb;
    private StarterAssetsInputs input;
    public bool isGrounded;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<StarterAssetsInputs>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayers.value, QueryTriggerInteraction.Ignore);

        if (input.move.magnitude > 0.1f)
        {
            animator.SetBool("isMoving", true);
            transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        if(isGrounded)
        {
            animator.SetBool("isGrounded", true);
            if(input.jump)
            {
                animator.SetBool("isJumping", true);
                rb.AddForce(Mathf.Sqrt(JumpHeight * -2f * -15f)*Vector3.up);
            }
        }
        else
        {
            animator.SetBool("isGrounded", false);
            animator.SetBool("isJumping", false);
            input.jump = false;
        }
    }
}
