using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float moveSpeed;
    [SerializeField] float rotateSpeed;
    [SerializeField] float stoppingDistance;

    [HideInInspector]
    public Transform target;

    private bool canMove = true;

    [Server]
    private void Start()
    {
        if (!isServer)
        {
            return;
        }

        animator.SetBool("isMoving", true);
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
                animator.SetBool("isMoving", false);
                animator.SetTrigger("canAttack");
            }
        }
    }
}
