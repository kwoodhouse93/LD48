using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour
{
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopSource;

    private bool startedLoop;

    void FixedUpdate()
    {
        if (!introSource.isPlaying && !startedLoop)
        {
            loopSource.Play();
            startedLoop = true;
        }
    }
}
