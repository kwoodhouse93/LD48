using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MenuRope : MonoBehaviour
{
    [Header("Dimensions")]
    [SerializeField] private int segmentCount;
    [SerializeField] private float segmentWidth;
    [SerializeField] private float segmentLength;

    [Header("Spawning")]
    [SerializeField] private float spawnSeparation;
    [SerializeField] private float throwForce;

    [Header("References")]
    [SerializeField] private GameObject ropeSegment;

    private Vector2 ropeSource;
    private List<GameObject> ropeSegments = new List<GameObject>();
    private LineRenderer lineRenderer;
    private bool active;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        DrawRope();
    }

    public void DrawRope()
    {
        if (!active) return;

        Vector3[] positions = new Vector3[ropeSegments.Count + 1];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            positions[i] = ropeSegments[i].transform.TransformPoint(ropeSegments[i].GetComponent<RopeSegment>().LocalStart);
        }
        positions[ropeSegments.Count] = ropeSegments[ropeSegments.Count - 1].transform.TransformPoint(ropeSegments[ropeSegments.Count - 1].GetComponent<RopeSegment>().LocalEnd);
        lineRenderer.positionCount = ropeSegments.Count + 1;
        lineRenderer.SetPositions(positions);
    }

    public void CreateRope(Vector3 startPos, Rigidbody2D attachEnd)
    {
        DestroyRope();

        ropeSource = new Vector3(startPos.x, startPos.y, 0);
        Vector3 startPoint = ropeSource;
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject r = Object.Instantiate(ropeSegment, startPoint, Quaternion.identity);
            RopeSegment rs = r.GetComponent<RopeSegment>();
            rs.SetLength(segmentLength);
            rs.SetWidth(segmentWidth);

            if (i == 0)
            {
                rs.FixStart(ropeSource);
            }
            else
            {
                rs.ConnectStart(ropeSegments[i - 1].GetComponent<Rigidbody2D>(), true);
            }

            // Throw end of rope.
            if (i == segmentCount - 1)
            {
                rs.ConnectEnd(attachEnd);
                Vector3 f = Vector3.ClampMagnitude(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0), throwForce);
                attachEnd.GetComponent<Rigidbody2D>().AddForce(f, ForceMode2D.Impulse);
            }


            startPoint += -Vector3.up * spawnSeparation;

            ropeSegments.Add(r);
        }
        active = true;
        lineRenderer.enabled = true;
    }

    private void DestroyRope()
    {
        active = false;
        lineRenderer.enabled = false;
        foreach (GameObject r in ropeSegments)
        {
            Object.Destroy(r);
        }
        ropeSegments.Clear();
    }

    public Vector2[] GetPoints()
    {
        Vector2[] points = new Vector2[ropeSegments.Count];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            points[i] = ropeSegments[i].transform.position;
        }
        return points;
    }

    public void AddForceAtSegment(int index, Vector2 force)
    {
        ropeSegments[index].GetComponent<Rigidbody2D>().AddForce(force);
    }
}
