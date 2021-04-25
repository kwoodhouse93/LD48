using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour
{
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopSource;

    private bool startedLoop;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Load volume
        if (PlayerPrefs.HasKey("sound"))
        {
            string sound = PlayerPrefs.GetString("sound");
            if (sound == "false")
            {
                introSource.volume = 0f;
                loopSource.volume = 0f;
            }
            else
            {
                introSource.volume = 1f;
                loopSource.volume = 1f;
            }
        }
    }

    public void ToggleMute()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            string sound = PlayerPrefs.GetString("sound");
            if (sound == "false")
            {
                PlayerPrefs.SetString("sound", "true");
                introSource.volume = 1f;
                loopSource.volume = 1f;
            }
            else
            {
                PlayerPrefs.SetString("sound", "false");
                introSource.volume = 0f;
                loopSource.volume = 0f;
            }
        }
        else
        {
            PlayerPrefs.SetString("sound", "false");
            introSource.volume = 0f;
            loopSource.volume = 0f;
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (!introSource.isPlaying && !startedLoop)
        {
            loopSource.Play();
            startedLoop = true;
        }
    }
}
