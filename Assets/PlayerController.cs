using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LaunchArcRenderer launchArc;
    [SerializeField] private HealthBar healthBar;

    [Header("Movement parameters")]
    [SerializeField] private float walkForceScale;
    [SerializeField] private float launchForceScale;
    [SerializeField] private float maxLaunchForce;
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
    private float curHealth;

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
        if (dead) return;
        if (curHealth < 0)
        {
            dead = true;
            rb.constraints = RigidbodyConstraints2D.None;

            // Indicate death by encouraging player to fall over.
            float deathTorque = Random.Range(-1f, 1f);
            rb.AddTorque(deathTorque);
        }

        if (aiming)
        {
            // Allow cancel with right click or escape
            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.Escape))
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
                    Launch(force);
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

        if (IsGrounded())
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            Walk(horizontal);
        }

        if (Input.GetMouseButtonDown(0))
        {
            aiming = true;
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
