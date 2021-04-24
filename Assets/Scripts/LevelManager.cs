using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private EndScreen endScreen;

    [SerializeField] private Animator fadeToBlack;
    [SerializeField] private float transitionTriggerDelay;
    [SerializeField] private float transitionRunTime;

    private float playerStartY;
    private float finalDistance;
    private float startTime;
    private float totalTime;
    private bool deadHandled;

    void Start()
    {
        if (player == null)
            throw new System.Exception("LevelManager player cannot be null");
        playerStartY = player.transform.position.y;
        startTime = Time.time;
    }

    void Update()
    {
        if (player == null)
            throw new System.Exception("LevelManager player cannot be null");

        if (!deadHandled)
        {
            float curDepth = playerStartY - player.transform.position.y;
            float curTime = Time.time - startTime;
            UpdateUI(curDepth, curTime);
        }

        if (!deadHandled && player.GetComponent<PlayerController>().IsDead)
        {
            deadHandled = true;
            TriggerEndScreen();

            finalDistance = playerStartY - player.transform.position.y;
            totalTime = Time.time - startTime;
        }
    }

    private void UpdateUI(float depth, float time)
    {
        string distanceFormatted;
        if (depth > 1000)
        {
            distanceFormatted = Mathf.FloorToInt(depth / 1000) + " km";
        }
        else
        {
            distanceFormatted = Mathf.FloorToInt(depth) + " m";
        }
        depthText.SetText(distanceFormatted);

        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
        string timeFormatted = string.Format("{0:D2}m {1:D2}s", timeSpan.Minutes, timeSpan.Seconds);
        timeText.SetText(timeFormatted);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void TriggerEndScreen()
    {
        StartCoroutine(ShowEndScreen());
    }

    IEnumerator ShowEndScreen()
    {
        // Wait a hot sec
        yield return new WaitForSeconds(transitionTriggerDelay);

        // Start fading to black
        fadeToBlack.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(transitionRunTime);

        // After that, show results
        endScreen.SetDistance(finalDistance);
        endScreen.SetTime(totalTime);
        endScreen.gameObject.SetActive(true);
    }
}
