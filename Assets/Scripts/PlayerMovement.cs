using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    public float speed = 10f;
    public float gravity = -9.81f;
    public float rotationSpeed = 5f;
    public Rigidbody rb;
    [SerializeField] public float hp = 200f;
    public Slider healthbar;
    public TMP_Text healthtxt;
    public float maxHp = 0f;
    public float hpBoost = 20f;

    [Header("Heartbeat")]
    [SerializeField] public float bpm = 60f;
    [SerializeField] public float decayRate = 10f;
    [SerializeField] public float bpmIncrease = 15f;
    [SerializeField] public float bpmMin = 20f;
    [SerializeField] public float bpmMax = 100f;

    private CharacterController controller;
    private Vector3 velocity;
    [SerializeField] private GameOverUIFail guif;

    void Start()
    {
        controller = GetComponent<CharacterController>();
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
    }

    void Update()
    {
        Die();
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        // Disable backward movement (S / Down Arrow)
        if (z < 0f)
            z = 0f;

        Vector3 move = transform.right * x + transform.forward * z;
        move.y = 0; 

        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        healthtxt.text = hp + " /" + maxHp;
        healthbar.value = hp / maxHp;

    }

    void Awake()
    {
        if (FindObjectsOfType<PlayerMovement3D>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(GameManager.Instance.targetSpawnID))
            return;

        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();

        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.spawnID == GameManager.Instance.targetSpawnID)
            {
                CharacterController controller = GetComponent<CharacterController>();
                controller.enabled = false;

                transform.position = sp.transform.position;
                velocity = Vector3.zero;

                controller.enabled = true;

                break;
            }
        }
    }
    private void Die()
    {
        if (hp <= 0f)
        {
            Destroy(gameObject);
            Debug.Log("Game Over!");
            //if (guif != null)
            //{
            //    guif.RestartLevel();
            //}
            //else
            //{
            //    guif = FindFirstObjectByType<GameOverUIFail>();
            //    if (guif != null) guif.RestartLevel();
            //    else Debug.LogError("GameOverUIFail not found in scene!");
            //}
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("HpBooster") && hp != maxHp)
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
    }

}