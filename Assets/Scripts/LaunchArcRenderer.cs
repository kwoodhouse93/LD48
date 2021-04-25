using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour
{
    [SerializeField] int resolution;

    private LineRenderer lineRenderer;
    private float gravity;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        gravity = Mathf.Abs(Physics2D.gravity.y);
    }

    public void DrawArc(Vector3 force, Rigidbody2D rb)
    {
        lineRenderer.enabled = true;

        Vector2 fromAngle = Vector2.right;

        float g = gravity * rb.gravityScale;
        float vel = (force.magnitude / rb.mass);
        float angle = Vector2.Angle(fromAngle, force);
        if (force.y < 0) angle = 360 - angle;
        float radAngle = Mathf.Deg2Rad * angle;

        float maxDist = (2 * vel * Mathf.Sin(radAngle)) / g;
        Vector3[] linePositions = new Vector3[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            float x = vel * t * Mathf.Cos(radAngle);
            float y = (vel * t * Mathf.Sin(radAngle)) - ((g * t * t) / 2);

            Vector3 newPos = new Vector3(x, y);
            linePositions[i] = newPos;
        }

        lineRenderer.positionCount = resolution + 1;
        lineRenderer.SetPositions(linePositions);
    }

    public void ClearArc()
    {
        lineRenderer.enabled = false;
    }
}
