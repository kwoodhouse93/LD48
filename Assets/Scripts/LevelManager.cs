using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform player;

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

        if (!deadHandled && player.GetComponent<PlayerController>().IsDead)
        {
            deadHandled = true;
            TriggerEndScreen();

            finalDistance = playerStartY - player.transform.position.y;
            totalTime = Time.time - startTime;
            Debug.Log("Distance reached: " + finalDistance + " Time taken: " + totalTime);
        }
    }

    public void TriggerEndScreen()
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
