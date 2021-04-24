using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI distanceText;
    [SerializeField] TextMeshProUGUI timeText;

    private float distance;
    private float time;
    private string distanceFormatted;
    private string timeFormatted;

    public void SetDistance(float d)
    {
        distance = d;
        if (d > 1000)
        {
            distanceFormatted = Mathf.FloorToInt(d / 1000) + " km";
        }
        distanceFormatted = Mathf.FloorToInt(d) + " m";
        distanceText.SetText(distanceFormatted);
    }

    public void SetTime(float t)
    {
        time = t;
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
        timeFormatted = string.Format("{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds);
        timeText.SetText(timeFormatted);
    }
}
