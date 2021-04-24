using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Transform innerBar;

    private float maxHealth;
    private float curHealth;

    private float maxInnerScale;

    void Start()
    {
        maxInnerScale = innerBar.localScale.x;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
    }

    public void SetHealth(float health)
    {
        curHealth = health;
    }

    void Update()
    {
        if (maxHealth == 0) throw new System.Exception("HealthBar: maxHealth is 0 - cannot calculate scale");

        float scaleFactor = (curHealth / maxHealth);
        if (scaleFactor < 0) scaleFactor = 0;

        Vector3 scale = innerBar.localScale;
        scale.x = scaleFactor * maxInnerScale;
        innerBar.localScale = scale;

        transform.rotation = Quaternion.identity;
    }
}
