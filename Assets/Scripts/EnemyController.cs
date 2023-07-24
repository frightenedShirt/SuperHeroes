using System.Collections;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float rotateSpeed;
    [SerializeField] float stoppingDistance;
    [SerializeField] Transform shootPoint;
    [SerializeField] float shootDelay;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject crystalPrefab;
    [SerializeField] GameObject shieldPrefab;

    [HideInInspector]
    public Transform target;

    public bool canMove = true;
    public bool reachedShip = false;

    [Server]
    private void Start()
    {
        if (!isServer)
        {
            return;
        }
    }

    [Server]
    private void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (!reachedShip)
        {
            Vector3 targetDirection = target.position - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(this.transform.forward, targetDirection, rotateSpeed * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection,Vector3.up);
            transform.position += this.transform.forward * moveSpeed * Time.deltaTime;

            var distance = Vector3.Distance(target.position, this.transform.position);

            if (distance < stoppingDistance)
            {
                reachedShip = true;
                StartCoroutine(StartAttacking());
            }
        }
    }

    private IEnumerator StartAttacking()
    {
        while(canMove)
        {
            Vector3 targetDirection = target.position - shootPoint.position;
            Vector3 newDirection = Vector3.RotateTowards(shootPoint.forward, targetDirection, 1000f, 0.0f);

            Quaternion newRotation = Quaternion.LookRotation(newDirection);

            GameObject bulletObject = Instantiate(bulletPrefab, shootPoint.position, newRotation);
            NetworkServer.Spawn(bulletObject);
            yield return new WaitForSeconds(shootDelay);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<PlayerCharacterController>(out PlayerCharacterController playerController))
        {
            Debug.LogError("GOt Player COntroller in collision");
            if (!canMove && playerController.m_SuperPower == SuperPowers.Dash && playerController.isDashing)
            {
                Debug.LogError("Player Hit");
                DropCollectables();
                NetworkManager.Destroy(this.gameObject);
            }
        }
    }

    private void DropCollectables()
    {
        GameObject crystals = Instantiate(crystalPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(crystals);
        if(crystals.TryGetComponent<CollectableCrystal>(out CollectableCrystal collectable))
        {
            collectable.shipManager = target.GetComponent<ShipManager>();
        }
    }

    [Command(requiresAuthority =false)]
    public void CMDStopMove()
    {
        canMove = false;
        reachedShip = true;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameObject shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
        shield.transform.SetParent(this.transform,false);
        shield.transform.localPosition = new Vector3(0f, 1f, 0f);
        NetworkServer.Spawn(shield);
        StopMove(shield.GetComponent<NetworkIdentity>().netId);
    }

    [ClientRpc]
    public void StopMove(uint id)
    {
        canMove = false;
        reachedShip = true;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameObject shield = NetworkClient.spawned[id].gameObject;
        shield.transform.SetParent(this.transform, false);
        shield.transform.localPosition = new Vector3(0f, 1f, 0f);
    }
}
