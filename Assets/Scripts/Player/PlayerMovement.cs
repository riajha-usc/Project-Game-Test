using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float rotationSpeed = 3f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("Primary Fields")]
    public Rigidbody rb;
    public float hp = 200f;
    public Slider healthbar;
    public TMP_Text healthtxt;
    public float maxHp = 0f;
    public float hpBoost = 20f;

    [Header("PerkFields")]
    public float pulsecost = 0.1f; // In % of hp
    public GameObject pulsePrefab;
    public bool usePulse = false;
    public Slider pulseBar;
    public TMP_Text pulseText;
    public float maxPulse = 50f;
    public float defaultPulse = 20f;
    private float playerPulse = 0f;
    public float pulseIncrement = 5f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }

        maxHp = hp;
        playerPulse = defaultPulse;
        if (pulseBar != null)
            pulseBar.gameObject.SetActive(usePulse);

        if (pulseText != null)
            pulseText.gameObject.SetActive(usePulse);
    }

    void Update()
    {
        Die();
        PulseCalculator();
        if (healthtxt != null) healthtxt.text = hp + " /" + maxHp;
        if (healthbar != null) healthbar.value = hp / maxHp;
        if (usePulse)
        {
            if (pulseBar != null) pulseBar.gameObject.SetActive(true);
            if (pulseText != null) pulseText.gameObject.SetActive(true);

            if (pulseText != null)
                pulseText.text = playerPulse + " / " + maxPulse;

            if (pulseBar != null)
                pulseBar.value = playerPulse / maxPulse;

            if (Input.GetKeyDown(KeyCode.Space) && playerPulse >= 30f)
            {
                FirePulse();
            }
        }
        else
        {
            if (pulseBar != null) pulseBar.gameObject.SetActive(false);
            if (pulseText != null) pulseText.gameObject.SetActive(false);
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Mathf.Max(0, Input.GetAxis("Vertical"));
        
        if (Mathf.Abs(x) < 0.15f) x = 0;
        if (Mathf.Abs(z) < 0.15f) z = 0;

        // Player-relative: W = forward (where player faces), S = back, A/D = strafe
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 move = (forward * z + right * x).normalized;

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move((move * speed + velocity) * Time.deltaTime);

        // Rotate player toward movement direction
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(rotationSpeed * Time.deltaTime));
        }
        if(GameManager.Instance.currentState == GameManager.GameState.Start)
        {
            hp = maxHp;
            // I want to reset the player gameobject in case of a restart here
        }
    }
    private void Die()
    {
        if (hp <= 0f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordDeathToEnemy();
                GameManager.Instance.GameOver();
            }
            UIManager.Instance.gameOverScreen.SetActive(true);
            Debug.Log("Game Over!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Projectile")
        {
            ProjectileController pc = new();
            hp = hp - pc.damage;
        }
        if (other.CompareTag("HpBooster") && hp != maxHp)
        {
            hp = Mathf.Clamp(hp + hpBoost, 0f, maxHp);
        }
        if (other.CompareTag("Projectile") && !SafeZoneController.InSafeZone)
        {
            ProjectileController pc = other.GetComponent<ProjectileController>();
            if (pc != null)
            {
                hp -= pc.damage;
            }
        }
        if(other.CompareTag("Pulser"))
        {
            usePulse = true;
        }
    }

    public void PulseCalculator()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerPulse = Mathf.Clamp(playerPulse + pulseIncrement, 0f, maxPulse);
        }
    }

    private void FirePulse()
    {
        GameObject pulse = Instantiate(pulsePrefab, transform.position, Quaternion.identity);

        PulseController pc = pulse.GetComponent<PulseController>();

        if (pc != null)
        {
            float chargePercent = playerPulse / maxPulse;
            pc.Initialize(chargePercent);
            hp -= (pulsecost * hp);
        }

        playerPulse = 0f;
    }
}