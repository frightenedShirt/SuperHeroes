using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class EnemyManager : NetworkBehaviour
{
    [SerializeField] int maxNumEnemies;
    [SerializeField] float spawnDelay;
    [SerializeField] GameObject enemyToSpawn;
    [SerializeField] List<Transform> spawnPositions;

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
            Vector3 spawnPoint = new Vector3(spawnPositions[index].position.x, spawnPositions[index].position.y, spawnPositions[index].position.z);
            curSpawnTime = spawnDelay;
            SpawnEnemies(spawnPoint);
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
    private void SpawnEnemies(Vector3 spawnPoint)
    {
        GameObject gobj = Instantiate(enemyToSpawn, spawnPoint, Quaternion.identity);
        gobj.transform.SetParent(this.transform);
        NetworkServer.Spawn(gobj);
    }
}
