using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public Animator fadeToBlack;
    public float transitionTime;

    public void LoadWithTransition(int buildIndex)
    {
        StartCoroutine(LoadScene(buildIndex));
    }

    IEnumerator LoadScene(int buildIndex)
    {
        fadeToBlack.SetTrigger("FadeToBlack");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(buildIndex);
    }
}
