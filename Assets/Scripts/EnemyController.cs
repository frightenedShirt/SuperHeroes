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

    [HideInInspector]
    public Transform target;

    public bool canMove = true;

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

        if (canMove)
        {
            Vector3 targetDirection = target.position - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(this.transform.forward, targetDirection, rotateSpeed * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection,Vector3.up);
            transform.position += this.transform.forward * moveSpeed * Time.deltaTime;

            var distance = Vector3.Distance(target.position, this.transform.position);

            if (distance < stoppingDistance)
            {
                canMove = false;
                StartCoroutine(StartAttacking());
            }
        }
    }

    private IEnumerator StartAttacking()
    {
        while(true)
        {
            Vector3 targetDirection = target.position - shootPoint.position;
            Vector3 newDirection = Vector3.RotateTowards(shootPoint.forward, targetDirection, 1000f, 0.0f);

            Quaternion newRotation = Quaternion.LookRotation(newDirection);

            GameObject bulletObject = Instantiate(bulletPrefab, shootPoint.position, newRotation);
            NetworkServer.Spawn(bulletObject);
            yield return new WaitForSeconds(shootDelay);
        }
    }
}
