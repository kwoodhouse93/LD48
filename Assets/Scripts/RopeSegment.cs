using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class RopeSegment : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private HingeJoint2D startHinge;
    private HingeJoint2D endHinge;

    private float width;
    private float length;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void SetWidth(float w)
    {
        width = w;
        Vector2 s = spriteRenderer.size;
        s.x = width;
        spriteRenderer.size = s;
        boxCollider2D.size = s;
    }

    public void SetLength(float l)
    {
        length = l;
        Vector2 s = spriteRenderer.size;
        s.y = l;
        spriteRenderer.size = s;
        boxCollider2D.size = s;
    }

    public void FixStart(Vector2 pos)
    {
        if (width == 0 || length == 0) throw new System.Exception("Trying to fix rope segment start while width or length is 0");

        if (startHinge == null)
        {
            startHinge = gameObject.AddComponent<HingeJoint2D>();
        }
        startHinge.connectedBody = null;
        startHinge.anchor = new Vector2(0, length / 2);
    }

    public void ConnectStart(Rigidbody2D connectTo, bool connectingInRope)
    {
        if (width == 0 || length == 0) throw new System.Exception("Trying to connect rope segment start while width or length is 0");

        if (startHinge == null)
        {
            startHinge = gameObject.AddComponent<HingeJoint2D>();
        }
        startHinge.connectedBody = connectTo;
        startHinge.anchor = new Vector2(0, length / 2);
        if (connectingInRope)
        {
            startHinge.autoConfigureConnectedAnchor = false;
            startHinge.connectedAnchor = new Vector2(0, -(length / 2));
            // startHinge.useLimits = true;
            // JointAngleLimits2D limits = new JointAngleLimits2D();
            // limits.max = 105;
            // limits.min = 435;
            // startHinge.limits = limits;
        }
    }
}
