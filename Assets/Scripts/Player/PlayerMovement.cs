using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float rotationSpeed = 1f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("Player Operation Fields")]
    //public string mode = "default";
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    [Header("Health")]
    public Rigidbody rb;
    public float hp = 100f;
    public float maxHp = 0f;
    private bool isDead = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null){
            rb.freezeRotation = true;
            rb.isKinematic = true;
            rb.useGravity = true;
        }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isDead = false;
        velocity = Vector3.zero;
        if (controller != null) controller.enabled = true;
        ResetToSpawnPoint();
    }

    private void ResetToSpawnPoint()
    {
        GameObject spawn = GameObject.Find("PlayerSpawnPoint");
        if (spawn == null) spawn = GameObject.FindWithTag("Respawn");
        if (spawn != null)
        {
            if (controller != null) controller.enabled = false;
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
            if (controller != null) controller.enabled = true;
        }
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        CameraController cam = Camera.main.GetComponent<CameraController>();
        if (cam != null)
        {
            cam.SetPlayer(transform);
        }
    }

    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
        }

        if (maxHp <= 0f)
            maxHp = 100f;

        hp = maxHp;
        isDead = false;

        ResetToSpawnPoint();
    }

    void Update()
    {
        Die();

        //float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxis("Vertical");

        //if (Mathf.Abs(x) < 0.15f) x = 0;
        if (Mathf.Abs(z) < 0.15f) z = 0;

        // Player-relative: W = forward (where player faces), S = back, A/D = strafe
        //Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        //Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        //Vector3 move = (forward * z + right * x).normalized;

        float rotationInput = Input.GetAxisRaw("Horizontal"); // A/D
        float forwardInput = Input.GetAxis("Vertical"); // W/S

        transform.Rotate(0f, rotationInput * 120f * Time.deltaTime, 0f);

        // Move forward in facing direction
        Vector3 move = transform.forward * forwardInput;
        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        if (controller != null && controller.enabled)
        {
            controller.Move((move * speed + velocity) * Time.deltaTime);
        }

        // Rotate player toward movement direction
        //if (move.sqrMagnitude > 0.01f)
        //{
        //    Quaternion targetRotation = Quaternion.LookRotation(move);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(rotationSpeed * Time.deltaTime));
        //}

        if (GameManager.Instance != null &&
            GameManager.Instance.currentState == GameManager.GameState.Start &&
            GameManager.Instance.GetCurrentLaneNumber() != 3)
        {
            hp = maxHp;
            isDead = false;
            if (controller != null) controller.enabled = true;
        }
    }
    private void Die()
    {
        if (hp <= 0.001f && !isDead)
        {
            isDead = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            if (UIManager.Instance != null && UIManager.Instance.gameOverScreen != null)
                UIManager.Instance.gameOverScreen.SetActive(true);

            // Stop player movement but keep script active so restart logic can run
            controller.enabled = false;
        }
    }

    public void TakeDamage(float amount)
    {
        hp = Mathf.Max(0f, hp - amount);
    }

}