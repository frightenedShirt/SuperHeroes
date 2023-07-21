using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class EnemyManager : NetworkBehaviour
{
    [SerializeField] int maxNumEnemies;
    [SerializeField] float spawnDelay;
    [SerializeField] GameObject[] enemyToSpawn;
    [SerializeField] List<Transform> spawnPositions;
    [SerializeField] Transform targetPosition;

    private float curSpawnTime;
    private bool canSpawnEnemy = false;

    public override void OnStartServer()
    {
        curSpawnTime = spawnDelay;
        StartCoroutine(DelaySpawnEnemies(10f));
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        if(!canSpawnEnemy)
        {
            return;
        }

        if (transform.childCount < maxNumEnemies && curSpawnTime <= 0.0f)
        {
            var index = UnityEngine.Random.Range(0, spawnPositions.Count);
            curSpawnTime = spawnDelay;
            SpawnEnemies(spawnPositions[index]);
        }
        else
        {
            curSpawnTime -= Time.deltaTime;
        }
    }

    [Server]
    private IEnumerator DelaySpawnEnemies(float delay)
    {
        yield return new WaitForSeconds(delay);
        canSpawnEnemy = true;
    }

    [Server]
    private void SpawnEnemies(Transform spawnPoint)
    {
        var index = UnityEngine.Random.Range(0, enemyToSpawn.Length);
        GameObject gobj = Instantiate(enemyToSpawn[index], spawnPoint.position,spawnPoint.rotation);
        gobj.transform.SetParent(this.transform);
        gobj.GetComponent<EnemyController>().target = targetPosition;
        NetworkServer.Spawn(gobj);
    }
}
