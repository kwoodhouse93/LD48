using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject gameUI;

    [Header("Light falloff")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private float lightFalloffStart;
    [SerializeField] private float lightFalloffRange;
    [SerializeField] private float maxLight;
    [SerializeField] private float minLight;

    [Header("Game over")]
    [SerializeField] private Animator fadeToBlack;
    [SerializeField] private float transitionTriggerDelay;
    [SerializeField] private float transitionRunTime;
    [SerializeField] private EndScreen endScreen;

    [Header("Milestone ping")]
    [SerializeField] private float milestoneInterval;
    [SerializeField] private float milestoneFlashTime;
    [SerializeField] private AudioClip milestoneAudioClip;

    private AudioSource audioSource;
    private float playerStartY;
    private float bestDepth;
    private float finalDistance;
    private float startTime;
    private float totalTime;
    private bool deadHandled;
    private float lastMilestone;
    private float unflashDepthMilestone;

    void Start()
    {
        if (player == null)
            throw new System.Exception("LevelManager player cannot be null");
        playerStartY = player.transform.position.y;
        startTime = Time.time;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null)
            throw new System.Exception("LevelManager player cannot be null");

        if (Time.time > unflashDepthMilestone)
        {
            depthText.color = Color.white;
        }

        if (!deadHandled)
        {
            float curDepth = playerStartY - player.transform.position.y;
            float curTime = Time.time - startTime;

            bestDepth = Mathf.Max(curDepth, bestDepth);

            UpdateUI(curDepth, curTime);

            float globalIntensity = Mathf.Lerp(maxLight, minLight, (curDepth - lightFalloffStart) / lightFalloffRange);
            globalLight.intensity = globalIntensity;
            playerLight.intensity = 1 - globalIntensity;

            if (curDepth >= lastMilestone + milestoneInterval)
            {
                audioSource.PlayOneShot(milestoneAudioClip);
                depthText.color = Color.red;
                unflashDepthMilestone = Time.time + milestoneFlashTime;
                lastMilestone += milestoneInterval;
            }
        }

        if (!deadHandled && player.GetComponent<PlayerController>().IsDead)
        {
            deadHandled = true;
            TriggerEndScreen();

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

        gameUI.SetActive(false);

        // Start fading to black
        fadeToBlack.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(transitionRunTime);

        // After that, show results
        endScreen.SetDistance(bestDepth);
        endScreen.SetTime(totalTime);
        endScreen.gameObject.SetActive(true);
    }
}
