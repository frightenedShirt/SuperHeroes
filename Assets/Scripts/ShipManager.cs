using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ShipManager : NetworkBehaviour
{
    public CanvasHUD canvasHUD;
    public GameManager gameManager;
    public GameObject healthHUD;
    public Slider slider;
    public float maxHealth;

    private float currentHealth;
    private GameObject localPlayerGO;
    private bool isDead=false;

    private void Start()
    {
        currentHealth = maxHealth / 2f;
        slider.maxValue = maxHealth;
        slider.value = maxHealth;

        if (isServerOnly)
        {
            return;
        }

        StartCoroutine(delayGetReference());
    }

    private void Update()
    {
        if(isDead)
        {
            return;
        }

        if(!gameManager.hasGameStarted)
        {
            return;
        }

        if(isServerOnly)
        {
            return;
        }

        if (localPlayerGO != null)
        {
            Vector3 targetDirection = localPlayerGO.transform.position - healthHUD.transform.position;
            Vector3 newDirection = Vector3.RotateTowards(healthHUD.transform.forward, targetDirection, 1000f, 0.0f);
            newDirection.y = 0f;

            healthHUD.transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);
        }
    }

    private IEnumerator delayGetReference()
    {
        yield return new WaitUntil(() => gameManager.hasGameStarted);
        localPlayerGO = NetworkClient.localPlayer.gameObject;
        healthHUD.GetComponent<Canvas>().worldCamera = localPlayerGO.transform.GetChild(1).gameObject.GetComponent<Camera>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(isDead)
        {
            return;
        }

        if(!isServer)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            currentHealth -= collision.gameObject.GetComponent<Bullet>().damage;
            UpdateHealthUI(currentHealth);
            Destroy(collision.gameObject);

            if(currentHealth <= 0f)
            {
                isDead = true;
                ShowGameOver();
            }
        }
    }

    [ClientRpc]
    private void UpdateHealthUI(float value)
    {
        slider.value = value;
    }

    [ClientRpc]
    private void ShowGameOver()
    {
        canvasHUD.panelGameOver.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
