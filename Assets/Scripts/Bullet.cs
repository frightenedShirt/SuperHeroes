using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float moveSpeed;
    public float damage;

    private void Update()
    {
        if(!isServer)
        {
            return;
        }

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}
