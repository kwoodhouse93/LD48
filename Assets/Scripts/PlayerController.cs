using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private enum Tools
    {
        Jump,
        Rope,
    }

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LaunchArcRenderer launchArc;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Rope rope;

    [Header("Movement parameters")]
    [SerializeField] private float walkForceScale;
    [SerializeField] private float launchForceScale;
    [SerializeField] private float maxLaunchForce;
    [SerializeField] private float ropeMoveSpeed;
    [SerializeField] private float ropeSwingForce;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Health parameters")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float fallDamageThreshold;


    // Component references
    private Rigidbody2D rb;
    private LineRenderer lr;

    // State variables
    private bool dead;
    private bool aiming;
    private bool roped;
    private float ropePos;
    private float curHealth;
    private Tools selected;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lr = GetComponent<LineRenderer>();

        curHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(maxHealth);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Quick hack to avoid annyoing glitches with rope physics.
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
        CheckHealth();
        if (dead) return;

        HandleInventory();

        if (roped)
        {
            HandleRopedMovement();
            return;
        }

        if (aiming)
        {
            HandleAiming();
        }

        // if (IsGrounded())
        // {
        //     float horizontal = Input.GetAxisRaw("Horizontal");
        //     Walk(horizontal);
        // }

        if (Input.GetMouseButtonDown(0))
        {
            aiming = true;
        }
    }

    private void CheckHealth()
    {
        if (curHealth < 0)
        {
            dead = true;
            rb.constraints = RigidbodyConstraints2D.None;

            // Indicate death by encouraging player to fall over.
            float deathTorque = Random.Range(-1f, 1f);
            rb.AddTorque(deathTorque);
        }
    }

    private void HandleInventory()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selected = Tools.Jump;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selected = Tools.Rope;
        }
    }

    private void HandleRopedMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            roped = false;
            return;
        }

        float vertical = Input.GetAxisRaw("Vertical");
        ropePos -= vertical * ropeMoveSpeed * Time.deltaTime;
        ropePos = Mathf.Clamp01(ropePos);

        if (ropePos >= 1)
        {
            roped = false;
            return;
        }

        Vector2[] points = rope.GetPoints();
        int divisions = points.Length - 1; // Gaps between each point that we can lerp between.
        float scaledPos = ropePos * divisions; // ropePos (0 to 1) * no. of gaps scales it to (0 to points.Length -1)
        int lowerIndex = Mathf.FloorToInt(scaledPos); // Floor to make sure we pick a valid index from the points array
        float lerpT = scaledPos - lowerIndex; // Keep the scale but subtract the lower index to get our lerp t
        Vector2 playerPos = Vector2.Lerp(points[lowerIndex], points[lowerIndex + 1], lerpT);

        rb.MovePosition(playerPos);

        float horizontal = Input.GetAxisRaw("Horizontal");
        Debug.Log("swing input: " + horizontal);
        rope.AddForceAtSegment(lowerIndex, new Vector2(horizontal, 0) * ropeSwingForce);
    }

    private void HandleAiming()
    {
        // Allow cancel with right click or escape
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.Space))
        {
            aiming = false;
            ClearArc();
            return;
        }

        Vector3 mousePos = getMousePosition();
        Vector3 force = GetLaunchForce(mousePos);

        // Fire on release
        if (Input.GetMouseButtonUp(0))
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
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void DeployRope(Vector3 force)
    {
        Vector3 ropeSource = new Vector3(
            transform.position.x + (force.normalized.x * 0.5f),
            transform.position.y - 0.3f,
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
}
