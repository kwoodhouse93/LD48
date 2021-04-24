using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RopeSegment : MonoBehaviour
{
    private BoxCollider2D boxCollider2D;
    private HingeJoint2D startHinge;
    private HingeJoint2D endHinge;

    private float width;
    private float length;

    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public Vector3 LocalStart => new Vector3(0, length / 2, 0);
    public Vector3 LocalEnd => new Vector3(0, -length / 2, 0);

    public void SetWidth(float w)
    {
        width = w;
        Vector2 s = boxCollider2D.size;
        s.x = width;
        boxCollider2D.size = s * 1.1f;
    }

    public void SetLength(float l)
    {
        length = l;
        Vector2 s = boxCollider2D.size;
        s.y = l;
        boxCollider2D.size = s;
    }

    public void FixStart(Vector2 pos)
    {
        if (width == 0 || length == 0) throw new System.Exception("Trying to fix rope segment start while width or length is 0");

        if (startHinge == null)
        {
            startHinge = gameObject.AddComponent<HingeJoint2D>();
        }
        startHinge.autoConfigureConnectedAnchor = false;
        startHinge.connectedBody = null;
        startHinge.anchor = LocalStart;
        startHinge.connectedAnchor = transform.TransformPoint(LocalStart);
    }

    public void ConnectStart(Rigidbody2D connectTo, bool connectingInRope)
    {
        if (width == 0 || length == 0) throw new System.Exception("Trying to connect rope segment start while width or length is 0");

        if (startHinge == null)
        {
            startHinge = gameObject.AddComponent<HingeJoint2D>();
        }
        startHinge.autoConfigureConnectedAnchor = false;
        startHinge.connectedBody = connectTo;
        startHinge.anchor = LocalStart;
        if (connectingInRope)
        {
            // This should really be the LocalEnd of the connected body. Making an assumption here that it
            // is an identical RopeSegment.
            startHinge.connectedAnchor = LocalEnd;
        }
    }
}
