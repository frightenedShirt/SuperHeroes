using UnityEngine;
using Mirror;
using StarterAssets;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEditor.PackageManager;

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

    private float powerMeter = 0;
    private PlayerModel playerModel;

    // power up
    private SuperPowers m_SuperPower;
    [SerializeField] private LayerMask layer;
    private RaycastHit hitInfo;
    private float RaycastDistance = 10.0f;
    private float DashSpeed = 10.0f;
    private LineRenderer trail;
    private float RayRange = 50;
    [SerializeField] private Transform rayOrigin;
    private float timeLongPress = 0.0f;

    private bool canMove = false;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        rb = GetComponent<Rigidbody>();

        if(!isLocalPlayer)
        {
            return;
        }
        m_SuperPower = SuperPowers.Freeze;

        this.transform.GetChild(1).gameObject.SetActive(true);
    }

    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        if(!canMove)
        {
            animator.SetBool("isGrounded", true);
            return;
        }

        StartAttack(m_SuperPower);

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundLayers.value, QueryTriggerInteraction.Ignore);

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
                rb.AddForce(Vector3.up * JumpHeight);
            }
        }
        else
        {
            input.jump = false;
            animator.SetBool("isGrounded", false);
            animator.SetBool("isJumping", false);
        }
    }

    private void StartAttack(SuperPowers _superPowers)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Debug]Short Press Input");
            ShortPressAttack(_superPowers);
        }

        if (Input.GetMouseButton(0))
        {
            Debug.Log("[Debug]Long Press Input");
            LongPressAttack(_superPowers);
        }

        if (Input.GetMouseButtonUp(0))
        {
            timeLongPress = 0;
        }

        if (powerMeter >= 100)
        {
            RageAttack(_superPowers);
        }
    }

    private void ShortPressAttack(SuperPowers _superPower)
    {
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hitInfo, RayRange, layer, QueryTriggerInteraction.Collide))
                {
                    Debug.Log("[Debug]Short Press Freeze power");
                    Debug.DrawRay(rayOrigin.position, rayOrigin.forward * RayRange, Color.red);
                }
                break;
        }
    }

    private void LongPressAttack(SuperPowers _superPower)
    {
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                timeLongPress += Time.deltaTime;
                if(timeLongPress >= 2.0)
                {
                    Debug.Log("[Debug]Long Press Freeze power1");
                    //Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hitInfo, RayRange, layer, QueryTriggerInteraction.Collide);
                    if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hitInfo, RayRange, layer, QueryTriggerInteraction.Collide))
                    {
                        Debug.Log("[Debug]Long Press Freeze power2");
                        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * RayRange, Color.red);
                    }
                }
                break;
                
        }
    }

    private void RageAttack(SuperPowers _superPower)
    {

    }

    [TargetRpc]
    public void DisableInput(NetworkConnectionToClient target)
    {
        canMove = false;
    }

    [TargetRpc]
    public void EnableInput(NetworkConnectionToClient target)
    {
        canMove = true;
    }
}
