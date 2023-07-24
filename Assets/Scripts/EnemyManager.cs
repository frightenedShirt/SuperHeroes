using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

public class EnemyManager : NetworkBehaviour
{
    [SerializeField] int maxNumEnemies;
    [SerializeField] float spawnDelay;
    [SerializeField] GameObject[] enemyToSpawn;
    [SerializeField] List<Transform> spawnPositions;
    [SerializeField] Transform targetPosition;
    [SerializeField] GameManager gameManager;

    private float curSpawnTime;

    public override void OnStartServer()
    {
        curSpawnTime = spawnDelay;
        StartCoroutine(SpawnFirstEnemy());
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        if(!gameManager.hasGameStarted)
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
    private void SpawnEnemies(Transform spawnPoint)
    {
        var index = UnityEngine.Random.Range(0, enemyToSpawn.Length);
        GameObject gobj = Instantiate(enemyToSpawn[index], spawnPoint.position,spawnPoint.rotation);
        gobj.transform.SetParent(this.transform);
        gobj.GetComponent<EnemyController>().target = targetPosition;
        NetworkServer.Spawn(gobj);
    }

    private IEnumerator SpawnFirstEnemy()
    {
        yield return new WaitUntil(() => gameManager.hasGameStarted);
        var index = UnityEngine.Random.Range(0, spawnPositions.Count);
        SpawnEnemies(spawnPositions[index]);
    }
}
