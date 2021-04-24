using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [SerializeField] private int segmentCount;
    [SerializeField] private float segmentWidth;
    [SerializeField] private float segmentLength;
    [SerializeField] private float spawnSeparation;
    [SerializeField] private float maxThrowForce;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private GameObject ropeSegment;

    private Vector2 ropeSource;
    private List<GameObject> ropeSegments = new List<GameObject>();

    public void CreateRope(Vector3 startPos, Vector3 endForce)
    {
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
                Vector3 f = Vector3.ClampMagnitude(endForce, maxThrowForce);
                r.GetComponent<Rigidbody2D>().AddForce(f, ForceMode2D.Impulse);
            }

            startPoint += endForce.normalized * spawnSeparation;

            ropeSegments.Add(r);
        }
    }

    public void DestroyRope()
    {
        Debug.Log("Destroying rope");
        foreach (GameObject r in ropeSegments)
        {
            Object.Destroy(r);
        }
        ropeSegments.Clear();
    }
}
