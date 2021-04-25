using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MuteButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;

    public UnityEvent onChange;

    void Start()
    {
        UpdateText();
    }

    public bool IsMuted()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            string sound = PlayerPrefs.GetString("sound");
            if (sound == "false") return false;
            return true;
        }
        return true;
    }

    public void TryToggle()
    {
        Music music = FindObjectOfType<Music>();
        if (music != null)
        {
            music.ToggleMute();
            onChange.Invoke();
        }
        UpdateText();
    }

    public void UpdateText()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            string sound = PlayerPrefs.GetString("sound");
            if (sound == "true") buttonText.SetText("MUTE");
            else buttonText.SetText("UNMUTE");
        }
        else
        {
            buttonText.SetText("MUTE");
        }
    }
}
