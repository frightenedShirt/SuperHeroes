using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject menuCamera;
    public CanvasHUD canvasHUD;
    public GameObject[] characterPrefabs;
    public Transform[] spawnPoints;
    public float gameStartTime = 2f;
    public bool hasGameStarted { get; private set; }
    public List<SuperPowers> SuperPowers = new();
    private List<NetworkConnectionToClient> playerID = new();
    private List<GameObject> playerObjects = new();

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.LogError($"PlayerJoined!");
        AddPlayerID();
    }

    private void Start()
    {
        if (!isServer)
        {
            return;
        }
        SuperPowers.Add(global::SuperPowers.Dash);
        SuperPowers.Add(global::SuperPowers.Freeze);
        StartCoroutine(DelayStartGame());
    }

    private IEnumerator DelayStartGame()
    {
        yield return new WaitUntil(() => NetworkManager.singleton.isActiveAndEnabled && NetworkManager.singleton.numPlayers >= 2 && playerID.Count >=2);
        Debug.Log($"[Debug] 2 player joined");
        for (int i = 0; i < NetworkManager.singleton.numPlayers; i++)
        {
            Debug.Log($"[Debug]Player Replaced");
            GameObject playerObject = Instantiate(characterPrefabs[i], spawnPoints[i].position,Quaternion.identity);
            NetworkServer.ReplacePlayerForConnection(playerID[i], playerObject, true);
            playerObjects.Add(playerObject);
        }
        DisableWaitingHUD();
        DestroyMenuCamera();
        EnableTutorialHUD();
        StartCoroutine(DelayStartGameTimer());
    }

    [Command(requiresAuthority = false)]
    private void AddPlayerID(NetworkConnectionToClient sender = null)
    {
        playerID.Add(sender);
        Debug.Log($"[Debug]Added Player ID");
    }

    [ClientRpc]
    private void DisableWaitingHUD()
    {
        canvasHUD.panelWaiting.SetActive(false);
    }

    [ClientRpc]
    private void EnableTutorialHUD()
    {
        canvasHUD.panelTutorial.SetActive(true);
    }

    [ClientRpc]
    private void DisableTutorialHUD()
    {
        canvasHUD.panelTutorial.SetActive(false);
    }

    [ClientRpc]
    private void UpdateStartGameTimer(string time)
    {
        canvasHUD.timerText.text = "Game Starts In " + time + "..!!";
    }

    [ClientRpc]
    private void SetGameStarted()
    {
        hasGameStarted = true;
    }

    [ClientRpc]
    private void DestroyMenuCamera()
    {
        Destroy(menuCamera);
    }

    [ClientRpc]
    private void EnableMainHUD()
    {
        canvasHUD.panelHUD.SetActive(true);
    }

    private IEnumerator DelayStartGameTimer()
    {
        while(gameStartTime > 0)
        {
            gameStartTime--;
            UpdateStartGameTimer(gameStartTime.ToString());
            yield return new WaitForSeconds(1f);
        }

        hasGameStarted = true;
        SetGameStarted();
        DisableTutorialHUD();
        EnableMainHUD();

        for (int i = 0; i < NetworkManager.singleton.numPlayers; i++)
        {
            playerObjects[i].GetComponent<PlayerCharacterController>().EnableInput(playerID[i]);
        }
    }
}
