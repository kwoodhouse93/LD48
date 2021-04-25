using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private List<GameObject> letters;
    [SerializeField] private List<MenuRope> ropes;

    [Header("Level loading")]
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private int nextBuildIndex;

    void Start()
    {
        if (letters.Count != ropes.Count) throw new System.Exception("Must have same number of ropes and letters");

        for (int i = 0; i < letters.Count; i++)
        {
            GameObject l = letters[i];
            MenuRope r = ropes[i];

            r.CreateRope(
                new Vector2(l.GetComponent<RectTransform>().position.x, Camera.main.orthographicSize + .5f),
                l.GetComponent<Rigidbody2D>()
            );
        }
    }

    public void Play()
    {
        sceneTransition.LoadWithTransition(nextBuildIndex);
    }
}
