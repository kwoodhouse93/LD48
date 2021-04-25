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
    [SerializeField] private float deadTransitionTriggerDelay;
    [SerializeField] private float winTransitionTriggerDelay;
    [SerializeField] private float transitionRunTime;
    [SerializeField] private EndScreen deadScreen;
    [SerializeField] private EndScreen winScreen;

    [Header("Milestones")]
    [SerializeField] private float milestoneInterval;
    [SerializeField] private float milestoneFlashTime;
    [SerializeField] private AudioClip milestoneAudioClip;
    [SerializeField] private MuteButton mute;
    [SerializeField] private float winDepth;

    private AudioSource audioSource;
    private float playerStartY;
    private float bestDepth;
    private float finalDistance;
    private float startTime;
    private float totalTime;
    private bool deadHandled;
    private bool winHandled;
    private float lastMilestone;
    private float unflashDepthMilestone;

    void Start()
    {
        if (player == null)
            throw new System.Exception("LevelManager player cannot be null");
        playerStartY = player.transform.position.y;
        startTime = Time.time;
        audioSource = GetComponent<AudioSource>();

        mute.onChange.AddListener(OnToggleMute);
    }

    private void OnToggleMute()
    {
        if (PlayerPrefs.HasKey("sound"))
        {
            string sound = PlayerPrefs.GetString("sound");
            if (sound == "false")
            {
                audioSource.volume = 0f;
                return;
            }
        }
        audioSource.volume = 1f;
    }

    void Update()
    {
        if (player == null)
            throw new System.Exception("LevelManager player cannot be null");

        if (Time.time > unflashDepthMilestone)
        {
            depthText.color = Color.white;
        }

        if (!deadHandled && !winHandled)
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

            if (curDepth > winDepth)
            {
                player.GetComponent<PlayerController>().ReadyToWin();
            }
        }

        if (!deadHandled && !winHandled && player.GetComponent<PlayerController>().IsDead)
        {
            deadHandled = true;
            TriggerDeadScreen();

            totalTime = Time.time - startTime;
        }
        if (!deadHandled && !winHandled && player.GetComponent<PlayerController>().HasWon)
        {
            winHandled = true;
            TriggerWinScreen();

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

    void TriggerDeadScreen()
    {
        StartCoroutine(ShowDeadScreen());
    }

    IEnumerator ShowDeadScreen()
    {
        // Wait a hot sec
        yield return new WaitForSeconds(deadTransitionTriggerDelay);

        gameUI.SetActive(false);

        // Start fading to black
        fadeToBlack.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(transitionRunTime);

        // After that, show results
        deadScreen.SetDistance(bestDepth);
        deadScreen.SetTime(totalTime);
        deadScreen.gameObject.SetActive(true);
    }

    void TriggerWinScreen()
    {
        StartCoroutine(ShowWinScreen());
    }

    IEnumerator ShowWinScreen()
    {
        // Wait a hot sec
        yield return new WaitForSeconds(winTransitionTriggerDelay);

        gameUI.SetActive(false);

        // Start fading to black
        fadeToBlack.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(transitionRunTime);

        // After that, show results
        winScreen.SetDistance(bestDepth);
        winScreen.SetTime(totalTime);
        winScreen.gameObject.SetActive(true);
    }
}
