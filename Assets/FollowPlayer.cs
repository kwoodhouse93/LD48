using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] private float smoothTime;
    [SerializeField] private float yOffset;

    private float vel;

    // Update is called once per frame
    void Update()
    {
        if (player == null) throw new System.Exception("FollowPlayer script has null player reference");
        float targetY = player.position.y + yOffset;
        Vector3 pos = transform.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref vel, smoothTime);
        transform.position = pos;
    }
}
