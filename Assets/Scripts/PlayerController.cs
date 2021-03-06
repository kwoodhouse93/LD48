using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private enum Tools
    {
        Jump,
        Rope,
        Flag,
    }

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LaunchArcRenderer launchArc;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Rope rope;
    [SerializeField] private GameObject spawnOnDeath;
    [SerializeField] private GameObject spawnOnCollision;
    [SerializeField] private GameObject winFlag;
    [SerializeField] private FollowPlayer cam;

    [Header("Movement parameters")]
    [SerializeField] private float walkForceScale;
    [SerializeField] private float launchForceScale;
    [SerializeField] private float maxLaunchForce;
    [SerializeField] private float ropeMoveSpeed;
    [SerializeField] private float ropeSwingForce;
    [SerializeField] private float ropeJumpVelocity;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Health parameters")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float fallDamageThreshold;

    [Header("UI references")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI jumpText;
    [SerializeField] private TextMeshProUGUI ropeText;
    [SerializeField] private GameObject jumpIcon;
    [SerializeField] private GameObject ropeIcon;

    [Header("Audio")]
    [SerializeField] private MuteButton mute;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField] private AudioClip jumpAudioClip;
    [SerializeField] private AudioClip ropeThrowAudioClip;
    [SerializeField] private AudioSource ropeSlideAudioSource;
    [SerializeField] private AudioClip ropeSlideAudioClip;
    [SerializeField] private float ropeSlideRepeatTime;
    [SerializeField] private float ropeSlidePitchJitter;
    [SerializeField] private AudioSource victoryAudioSource;

    // Component references
    private Rigidbody2D rb;
    private LineRenderer lr;

    // Input
    float horizontal;
    float vertical;

    // State variables
    private bool dead;
    private bool aiming;
    private bool roped;
    private bool deroping;
    private float ropePos;
    private float curHealth;
    private Tools selected;
    private bool readyToWin;
    private bool winTriggered;

    // Physics jank
    private Vector2 lastPos;

    // Audio timing
    private float nextSlideAudio;

    // Public accessors
    public bool IsDead => dead;
    public bool HasWon => winTriggered;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lr = GetComponent<LineRenderer>();

        curHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(maxHealth);

        selected = Tools.Rope;

        mute.onChange.AddListener(OnToggleMute);
    }

    private void OnToggleMute()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            string sound = PlayerPrefs.GetString("sound");
            if (sound == "false")
            {
                audioSource.volume = 0f;
                ropeSlideAudioSource.volume = 0f;
                victoryAudioSource.volume = 0f;
                return;
            }
        }
        audioSource.volume = 1f;
        ropeSlideAudioSource.volume = 1f;
        victoryAudioSource.volume = 1f;

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Quick hack to avoid annoying glitches with rope physics killing the player.
        if (roped) return;

        float impact = 0;

        // Find largest relative velocity
        foreach (ContactPoint2D contact in collision.contacts)
        {
            impact = Mathf.Max(impact, contact.relativeVelocity.magnitude);
        }

        if (impact > fallDamageThreshold)
        {
            curHealth -= impact;
            healthBar.SetHealth(curHealth);
        }
    }

    void Update()
    {
        if (winTriggered) return;

        UpdateUI();

        CheckHealth();
        if (dead) return;

        if (!roped && !aiming && readyToWin)
        {
            selected = Tools.Flag;
            UpdateUI();
            HandleFlagPlacement();
            return;
        }

        HandleInventory();
        GetAxisInput();


        if (roped)
        {
            HandleRopedMovement();
            return;
        }

        if (aiming)
        {
            HandleAiming();
            return;
        }

        // if (IsGrounded())
        // {
        //     float horizontal = Input.GetAxisRaw("Horizontal");
        //     Walk(horizontal);
        // }

        if ((Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) || Input.GetKeyDown(KeyCode.Space))
        {
            aiming = true;
        }
    }

    void FixedUpdate()
    {
        if (roped || deroping)
            HandleRopedMovementFixed();
    }

    private void CheckHealth()
    {
        if (!dead && curHealth <= 0)
        {
            ClearArc();

            if (spawnOnDeath != null)
                Object.Instantiate(spawnOnDeath, transform.position, transform.rotation);

            audioSource.PlayOneShot(deathAudioClip);

            dead = true;
            rb.constraints = RigidbodyConstraints2D.None;

            // Indicate death by encouraging player to fall over.
            float deathTorque = Random.Range(-1f, 1f);
            rb.AddTorque(deathTorque);
        }
    }

    private void GetAxisInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    private void HandleInventory()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selected = Tools.Jump;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selected = Tools.Rope;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selected == Tools.Jump) selected = Tools.Rope;
            else if (selected == Tools.Rope) selected = Tools.Jump;
        }
    }

    private void UpdateUI()
    {
        if (selected == Tools.Flag)
        {
            instructionText.SetText("Click or [SPACE] to PLANT FLAG");
            jumpText.color = Color.red;
            jumpIcon.SetActive(true);
            jumpIcon.GetComponent<Image>().color = Color.red;
            jumpText.SetText("FLAG");
            ropeIcon.SetActive(false);
            ropeText.SetText("");
            return;
        }

        string tool = "JUMP";
        string toolAction = "JUMP";
        if (selected == Tools.Rope)
        {
            tool = "ROPE";
            toolAction = "SET UP ROPE";
        }

        if (dead)
            instructionText.SetText("");
        else if (aiming)
        {
            if (Input.GetMouseButton(0))
                instructionText.SetText("Aim with mouse, release click to " + toolAction);
            else
                instructionText.SetText("Aim with mouse, [SPACE] again to " + toolAction);
        }
        else if (roped)
        {
            instructionText.SetText("[WASD] to move, Click or [SPACE] to LET GO");
        }
        else
        {
            instructionText.SetText("Click and hold or [SPACE] to AIM " + tool);
        }


        if (selected == Tools.Jump)
        {
            if (aiming)
            {
                jumpIcon.GetComponent<Image>().color = Color.cyan;
            }
            else
            {
                jumpIcon.GetComponent<Image>().color = Color.red;
            }
            jumpText.color = Color.red;
            jumpIcon.SetActive(true);
            ropeText.color = Color.white;
            ropeIcon.SetActive(false);
        }
        if (selected == Tools.Rope)
        {
            if (aiming)
            {
                ropeIcon.GetComponent<Image>().color = Color.cyan;
            }
            else if (roped)
            {
                ropeIcon.GetComponent<Image>().color = Color.green;
            }
            else
            {
                ropeIcon.GetComponent<Image>().color = Color.red;
            }
            ropeText.color = Color.red;
            ropeIcon.SetActive(true);
            jumpText.color = Color.white;
            jumpIcon.SetActive(false);
        }

    }

    private void HandleFlagPlacement()
    {
        if (winTriggered) return;
        if ((Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) || Input.GetKeyDown(KeyCode.Space))
        {
            winTriggered = true;
            winFlag.SetActive(true);
            Music m = FindObjectOfType<Music>();
            if (m != null) m.Destroy();
            victoryAudioSource.Play();
            cam.Drift();
        }
    }

    private void HandleRopedMovement()
    {
        ropePos -= vertical * ropeMoveSpeed * Time.deltaTime;
        ropePos = Mathf.Clamp01(ropePos);

        if (Time.time > nextSlideAudio && Mathf.Abs(vertical) > 0.1)
        {
            ropeSlideAudioSource.pitch += Random.Range(-ropeSlidePitchJitter, ropeSlidePitchJitter);
            ropeSlideAudioSource.PlayOneShot(ropeSlideAudioClip, 0.5f);
            nextSlideAudio = Time.time + ropeSlideRepeatTime;
        }

        if (ropePos >= 1 || Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()))
        {
            audioSource.PlayOneShot(jumpAudioClip, 0.5f);

            deroping = true;
            roped = false;
            return;
        }
    }

    private void HandleRopedMovementFixed()
    {
        if (deroping)
        {
            Vector2 vel = (rb.position - lastPos) * (1 / Time.fixedDeltaTime);

            // Add a little go-juice if you're trying to move sideways when you let go.
            vel.x += horizontal * ropeJumpVelocity;

            rb.velocity = vel;
            deroping = false;
        }

        Vector2[] points = rope.GetPoints();
        int divisions = points.Length - 1; // Gaps between each point that we can lerp between.
        float scaledPos = ropePos * divisions; // ropePos (0 to 1) * no. of gaps scales it to (0 to points.Length -1)
        int lowerIndex = Mathf.FloorToInt(scaledPos); // Floor to make sure we pick a valid index from the points array
        float lerpT = scaledPos - lowerIndex; // Keep the scale but subtract the lower index to get our lerp t
        Vector2 playerPos;
        if (lowerIndex >= points.Length - 1) playerPos = points[lowerIndex];
        else playerPos = Vector2.Lerp(points[lowerIndex], points[lowerIndex + 1], lerpT);

        lastPos = rb.position;
        rb.MovePosition(playerPos);

        rope.AddForceAtSegment(lowerIndex, new Vector2(horizontal, 0) * ropeSwingForce);
    }

    private void HandleAiming()
    {
        // Allow cancel with right click or escape
        if (Input.GetMouseButton(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            aiming = false;
            ClearArc();
            return;
        }

        Vector3 mousePos = getMousePosition();
        Vector3 force = GetLaunchForce(mousePos);

        // Fire on release or space
        if (Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.Space))
        {
            aiming = false;
            ClearArc();

            if (IsGrounded())
            {
                switch (selected)
                {
                    case Tools.Jump:
                        Launch(force);
                        break;
                    case Tools.Rope:
                        DeployRope(force);
                        break;
                }
            }
            return;
        }

        if (IsGrounded())
        {
            DrawArc(force);
        }
        else
        {
            ClearArc();
        }
    }

    private bool IsGrounded()
    {
        Collider2D overlap = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        return overlap != null;
    }

    private void Walk(float horizontalInput)
    {
        Vector3 raycastDir = Quaternion.AngleAxis(20, Vector3.forward) * -Vector3.up;
        if (horizontalInput < 0) raycastDir = Quaternion.AngleAxis(-20, Vector3.forward) * -Vector3.up;

        Debug.DrawLine(groundCheck.position, (groundCheck.position + (raycastDir * groundCheckRadius)), Color.blue, .01f);

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, -transform.up, groundCheckRadius, groundLayers);
        if (hit.collider != null)
        {
            // Calculate ground angle to allow walking up slopes more smoothly.
            Vector3 walkDir = Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal;
            rb.AddForce(walkDir * horizontalInput * walkForceScale);
        }
    }

    private Vector3 GetLaunchForce(Vector3 mousePos)
    {
        Vector3 launchForce = (mousePos - transform.position) * launchForceScale;
        launchForce = Vector3.ClampMagnitude(launchForce, maxLaunchForce);
        return launchForce;
    }

    private void Launch(Vector3 force)
    {
        audioSource.PlayOneShot(jumpAudioClip);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void DeployRope(Vector3 force)
    {
        audioSource.PlayOneShot(ropeThrowAudioClip);
        Vector3 ropeSource = new Vector3(
            transform.position.x + (force.normalized.x * 0.5f),
            transform.position.y - 0.2f,
            0
        );
        rope.CreateRope(ropeSource, force);
        ropePos = 0;
        roped = true;
    }

    private void DrawArc(Vector3 force)
    {
        launchArc.DrawArc(force, rb);
    }

    private void ClearArc()
    {
        launchArc.ClearArc();
    }

    private Vector2 getMousePosition()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    public void KillPlayer()
    {
        curHealth = 0;
    }

    public void ReadyToWin()
    {
        readyToWin = true;
    }
}
