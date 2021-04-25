using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private List<GameObject> slides;
    [SerializeField] private Rope rope;
    [SerializeField] private Vector2 ropeSource;
    [SerializeField] private Vector2 ropeForce;
    [SerializeField] private int deployRopeAtSlide;

    [Header("Level loading")]
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private int nextBuildIndex;


    private int currentSlide = 0;

    void Start()
    {
        for (int i = 0; i < slides.Count; i++)
        {
            if (i == 0) slides[i].SetActive(true);
            else slides[i].SetActive(false);
        }
    }
    public void Next()
    {
        currentSlide++;
        if (currentSlide == deployRopeAtSlide)
        {
            rope.CreateRope(ropeSource, ropeForce);
        }
        if (currentSlide >= slides.Count)
        {
            sceneTransition.LoadWithTransition(nextBuildIndex);
            return;
        }
        slides[currentSlide - 1].SetActive(false);
        slides[currentSlide].SetActive(true);
    }

    public void Skip()
    {
        sceneTransition.LoadWithTransition(nextBuildIndex);
    }
}
