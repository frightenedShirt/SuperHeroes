using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CanvasHUD : MonoBehaviour
{
    public GameObject panelStart,panelWaiting,panelTutorial,panelGameOver;
    public Button buttonHost, buttonClient;
    public InputField inputFieldAddress;

    public TextMeshProUGUI timerText;

    private void Start()
    {
        //Update the canvas text if you have manually changed network managers address from the game object before starting the game scene
        if (NetworkManager.singleton.networkAddress != "localhost")
        {
            inputFieldAddress.text = NetworkManager.singleton.networkAddress;
        }

        //Adds a listener to the main input field and invokes a method when the value changes.
        inputFieldAddress.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        //Make sure to attach these Buttons in the Inspector
        buttonHost.onClick.AddListener(ButtonHost);
        buttonClient.onClick.AddListener(ButtonClient);

        //This updates the Unity canvas, we have to manually call it every change, unlike legacy OnGUI.
        SetupCanvas();
    }

    // Invoked when the value of the text field changes.
    public void ValueChangeCheck()
    {
        NetworkManager.singleton.networkAddress = inputFieldAddress.text;
    }

    public void ButtonHost()
    {
        NetworkManager.singleton.StartHost();
        SetupCanvas();
    }

    public void ButtonServer()
    {
        NetworkManager.singleton.StartServer();
        SetupCanvas();
    }

    public void ButtonClient()
    {
        NetworkManager.singleton.StartClient();
        SetupCanvas();
    }

    public void SetupCanvas()
    {
        // Here we will dump majority of the canvas UI that may be changed.

        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (NetworkClient.active)
            {
                panelStart.SetActive(false);
                panelWaiting.SetActive(true);
                Debug.Log($"<color=cyan>Connecting to {NetworkManager.singleton.networkAddress}..</color>");
            }
            else
            {
                panelStart.SetActive(true);
                panelWaiting.SetActive(false);
            }
        }
        else
        {
            panelStart.SetActive(false);
            panelWaiting.SetActive(true);

            // server / client status message
            if (NetworkServer.active)
            {
                Debug.Log($"<color=cyan>Server: active. Transport: {Transport.active}</color>");
                // Note, older mirror versions use: Transport.activeTransport
            }
            if (NetworkClient.isConnected)
            {
                Debug.Log($"<color=cyan>Client: address = {NetworkManager.singleton.networkAddress}</color>");
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}