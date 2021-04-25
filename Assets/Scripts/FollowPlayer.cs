using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] private float smoothTime;
    [SerializeField] private float yOffset;
    [SerializeField] private float driftSpeed;

    private float vel;
    private bool drifting;

    // Update is called once per frame
    void Update()
    {
        if (drifting)
        {
            Vector3 cPos = transform.position;
            cPos.y += driftSpeed * Time.deltaTime;
            transform.position = cPos;
            return;
        }

        if (player == null) throw new System.Exception("FollowPlayer script has null player reference");
        float targetY = player.position.y + yOffset;
        Vector3 pos = transform.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref vel, smoothTime);
        transform.position = pos;
    }

    public void Drift()
    {
        drifting = true;
    }
}
