using UnityEngine;
using Mirror;
using StarterAssets;
using System.Collections.Generic;
using System.Collections;

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
    public LayerMask enemyLayer;

    private Rigidbody rb;
    private StarterAssetsInputs input;
    public bool isGrounded;

    private float powerMeter = 0;
    private PlayerModel playerModel;

    // power up
    [SerializeField] private LayerMask layer;
    [SerializeField] private GameObject ramp;
    [SerializeField] private GameObject rampPrefab;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Vector3 maxScale;
    private SuperPowers m_SuperPower;
    private RaycastHit hitInfo;
    private float RaycastDistance = 10.0f;
    private float DashSpeed = 10.0f;
    private LineRenderer trail;
    private float RayRange = 50;
    private float timeLongPress = 0.0f;
    private List<Vector3> enemyPosList;
    private bool canMove = false;
    private bool isDashing = false;
    private Coroutine dashPlayerRage;
    private float rageMeter = 0;
    private float SuperDashSpeed = 1000f;

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
        if (rageMeter >= 100)
        {
            RageAttack(_superPowers);
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("[Debug]Short Press Input");
            ShortPressAttack(_superPowers);
        }

        if (Input.GetMouseButton(0))
        {
            timeLongPress += Time.deltaTime;
            if (timeLongPress >= 2.0)
            {
                Debug.Log("[Debug]Long Press Input");
                LongPressAttack(_superPowers);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            timeLongPress = 0;
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
            case SuperPowers.Dash:
                Debug.Log("[Debug]Short Press Dash power");
                rb.AddForce(transform.forward * DashSpeed, ForceMode.Force);
                isDashing = true;
                break;
        }
    }

    private void LongPressAttack(SuperPowers _superPower)
    {
        int speed = 0;
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                Debug.Log("[Debug]Long Press Freeze power1");
                ramp = Instantiate(rampPrefab, transform.forward * 2, Quaternion.identity, null);
                if(rampPrefab.transform.localScale.x <= maxScale.x && rampPrefab.transform.localScale.y <= maxScale.y && rampPrefab.transform.localScale.z <= maxScale.z)
                {
                    rampPrefab.transform.localScale += new Vector3(0, 0, 2);
                }
                //if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hitInfo, RayRange, layer, QueryTriggerInteraction.Collide))
                //{
                //    Debug.Log("[Debug]Long Press Freeze power2");
                //    Debug.DrawRay(rayOrigin.position, rayOrigin.forward * RayRange, Color.red);
                //}
                break;
            case SuperPowers.Dash:
                Debug.Log("[Debug]Short Press Dash power");
                if(speed <= SuperDashSpeed)
                {
                    speed += 2;
                }
                rb.AddForce(transform.forward * speed, ForceMode.Force);
                isDashing = true;
                break;
        }
    }

    private void RageAttack(SuperPowers _superPower)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10.0f, enemyLayer, QueryTriggerInteraction.Ignore);
        rageMeter = 0;
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                foreach (var hitCollider in hitColliders)
                {
                    if(hitCollider.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
                    {
                        Debug.Log("[Debug] Collider Detected");
                        enemyController.canMove = false;
                    }
                }
                break;
            case SuperPowers.Dash:
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
                    {
                        Debug.Log("[Debug] Collider Detected");
                        if(enemyController.canMove == false)
                        {
                            enemyPosList.Add(hitCollider.gameObject.transform.position);
                        }
                    }
                }
                StartCoroutine(MovePlayer());
                break;
        }
    }

    private IEnumerator MovePlayer()
    {
        for(int i = 0; i < enemyPosList.Count; i++)
        {
            dashPlayerRage = StartCoroutine(RagePlayerDashRoutine(i));
            yield return dashPlayerRage;
        }
    }

    private IEnumerator RagePlayerDashRoutine(int currentPos)
    {
        while (transform.position != enemyPosList[currentPos])
        {
            transform.position = Vector3.MoveTowards(transform.position, enemyPosList[currentPos], SuperDashSpeed * Time.deltaTime);
            yield return null;
        }
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
