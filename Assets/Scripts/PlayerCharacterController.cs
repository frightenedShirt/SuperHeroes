using UnityEngine;
using Mirror;
using StarterAssets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCharacterController : NetworkBehaviour
{
    public Animator animator;
    public float walkSpeed;
    public float JumpHeight = 1.2f;
    public Slider rageMeterUi;

    public bool isGrounded;
    public Transform groundCheck;
    public LayerMask enemyLayer;
    public LayerMask groundLayers;
    public SuperPowers m_SuperPower;

    private Rigidbody rb;
    private StarterAssetsInputs input;

    [SerializeField] private LayerMask layer;
    [SerializeField] private GameObject ramp;
    [SerializeField] private GameObject crystalPrefabs;
    [SerializeField] private GameObject rampPrefab;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Vector3 maxScale;

    private RaycastHit hitInfo;
    private List<Vector3> enemyPosList;
    private Coroutine dashPlayerRage;
    private float RaycastDistance = 10.0f;
    private float DashSpeed = 25.0f;
    private float RayRange = 50;
    private float timeLongPress = 0.0f;
    private float SuperDashSpeed = 1000f;
    private float rageMeter = 0;
    private bool canMove = false;
    private bool isDashing = false;
    private bool inRage = false;
    private int speed = 0;
    private float sizeIncrease = 0;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<CollectableCrystal>(out CollectableCrystal collectable))
        {
            collectable.DoCollect();
            NetworkManager.Destroy(collision.gameObject);
        }
    }

    private void StartAttack(SuperPowers _superPowers)
    {
        
        if (Input.GetMouseButtonUp(0))
        {
            if (rageMeter >= 10)
            {
                RageAttack(_superPowers);
                return;
            }
            Debug.Log("[Debug]Short Press Input");
            timeLongPress = 0;
            speed = 0;
            ShortPressAttack(_superPowers);
        }

        if (Input.GetMouseButton(0))
        {
            if (rageMeter >= 10)
            {
                RageAttack(_superPowers);
                return;
            }
            timeLongPress += Time.deltaTime;
            if (timeLongPress >= 2.0)
            {
                Debug.Log("[Debug]Long Press Input");
                LongPressAttack(_superPowers);
            }
            if(inRage)
            {
                inRage = false;
                ramp = Instantiate(rampPrefab, hitInfo.collider.transform.position, Quaternion.identity, null);
                rampPrefab.transform.localScale += maxScale;
                //if (rampPrefab.transform.localScale.x <= maxScale.x && rampPrefab.transform.localScale.y <= maxScale.y && rampPrefab.transform.localScale.z <= maxScale.z)
                //{
                //    rampPrefab.transform.localScale += new Vector3(0, 0, 1);
                //}
            }
        }
    }

    private void ShortPressAttack(SuperPowers _superPower)
    {
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                Debug.DrawRay(rayOrigin.position, this.transform.GetChild(1).gameObject.transform.forward * RayRange, Color.cyan);
                if (Physics.Raycast(rayOrigin.position, this.transform.GetChild(1).gameObject.transform.forward, out hitInfo, RayRange, enemyLayer, QueryTriggerInteraction.Collide))
                {
                    Debug.Log("[Debug]Short Press Freeze power");
                    if(hitInfo.rigidbody.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
                    {   
                        enemyController.canMove = false;
                        rageMeter++;
                        rageMeterUi.value = rageMeter;
                    }
                }
                break;
            case SuperPowers.Dash:
                if(!isDashing)
                {
                    Debug.Log("[Debug]Short Press Dash power");
                    rb.AddForce(transform.forward * DashSpeed, ForceMode.Impulse);
                    isDashing = true;
                    StartCoroutine(StopPlayer());
                }
                break;
        }
    }

    private void LongPressAttack(SuperPowers _superPower)
    {
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                Debug.DrawRay(rayOrigin.position, this.transform.GetChild(1).gameObject.transform.forward * RayRange, Color.cyan);
                if (Physics.Raycast(rayOrigin.position, this.transform.GetChild(1).gameObject.transform.forward, out hitInfo, RayRange, layer, QueryTriggerInteraction.Collide))
                {
                    if(maxScale.y < 2 && maxScale.z < 2)
                    maxScale.y += 0.25f; 
                    maxScale.z += 0.25f;
                    inRage = true;
                    Debug.Log($"[Debug]Long Press Freeze power1::{maxScale}");
                }
                break;
            case SuperPowers.Dash:
                if(!isDashing)
                {
                    Debug.Log("[Debug]Short Press Dash power");
                    if (speed <= SuperDashSpeed)
                    {
                        speed += 2;
                    }
                    rb.AddForce(transform.forward * (speed + DashSpeed), ForceMode.Force);
                    isDashing = true;
                    StartCoroutine(StopPlayer());
                }
                break;
        }
    }

    private IEnumerator StopPlayer()
    {
        yield return new WaitForSecondsRealtime(3);
        isDashing = false;
        rb.velocity = new Vector3(0,0,0);
    }

    private void RageAttack(SuperPowers _superPower)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10.0f, enemyLayer, QueryTriggerInteraction.Ignore);
        rageMeter = 0;
        rageMeterUi.value = rageMeter;
        switch (_superPower)
        {
            case SuperPowers.Freeze:
                foreach (var hitCollider in hitColliders)
                {
                    if(hitCollider.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
                    {
                        if(enemyController.canMove == true)
                        {
                            Debug.Log("[Debug] Collider Detected");
                            enemyController.canMove = false;
                            rageMeterUi.value = rageMeter;
                        }
                    }
                }
                break;
            case SuperPowers.Dash:
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
                    {
                        isDashing = true;
                        Debug.Log("[Debug] Collider Detected");
                        if (enemyController.canMove == false)
                        {
                            enemyPosList.Add(hitCollider.gameObject.transform.position);
                        }
                    }
                }
                rb.velocity = new Vector3(0,0,0);
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
