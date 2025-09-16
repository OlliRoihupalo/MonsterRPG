using Mono.Cecil;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public string unitName;
    public PlayerController playerController;
    public GameObject timeline;
    public GameObject cam;
    public Canvas canvas;
    //public GameObject sprite;
    public string faction;
    public float maxHealth;
    public float health;
    public int speed;
    public GameObject highlight;
    private RectTransform healthBar;
    private RectTransform healthDisplay;
    public RectTransform healthLoss;
    private float healthLossAmount;
    public float healthDisplayDelay = 1.2f;
    public float healthDisplayTimer;

    // health, attack, defense, healthGrowthPerLevel, attackGrowthPerLevel, defenseGrowthPerLevel
    // A list of actions that the unit can obtain / use

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeline = GameObject.Find("Timeline");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        canvas = GetComponentInChildren<Canvas>();
        healthBar = canvas.transform.Find("HealthBar").GetComponent<RectTransform>();
        healthDisplay = healthBar.transform.Find("HealthDisplay").GetComponent<RectTransform>();
        healthLoss = healthBar.transform.Find("HealthLossDisplay").GetComponent<RectTransform>();
        highlight = canvas.transform.Find("Highlight").gameObject;
        cam = Camera.main.gameObject;
        health = maxHealth;
        healthLossAmount = maxHealth;
        highlight.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //sprite.transform.LookAt(cam.transform.position);
        healthDisplayTimer -= Time.deltaTime;
        if (healthDisplayTimer <= 0 && healthLoss.sizeDelta.x > healthDisplay.sizeDelta.x)
        {
            healthLossAmount -= Time.deltaTime * 30f;
            healthLoss.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthLossAmount);
        }
    }

    public void TakeDamage(float damage)
    {
        /*if (health == maxHealth)
        {
            canvas.gameObject.SetActive(true);
        }*/
        if (healthDisplayTimer <= 0)
        {
            healthLoss.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Ceil((health / maxHealth) * 300f));
        }
        health -= damage;
        healthDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Ceil((health / maxHealth) * 300f));
        /*if (healthLossAmount > (500f - Mathf.Ceil((health / maxHealth) * 500f)))
        {
            healthLossAmount = 500f - Mathf.Ceil((health / maxHealth) * 500f);
        }*/
        //healthLoss.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthLossAmount);
        healthDisplayTimer = healthDisplayDelay;
        if (health <= 0)
        {
            //animator.SetTrigger("Death");
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

            int enemies = 0;

            foreach (GameObject unit in units)
            {
                Unit un = unit.GetComponent<Unit>();
                if (un.faction == "Enemy")
                {
                    enemies++;
                }
            }

            if (enemies == 1)
            {
                // End combat
            }
            Destroy(gameObject);
        }
    }

    public void CreateTimelineEvent()
    {
        TimelineEvent te = timeline.AddComponent<TimelineEvent>();
        te.timelinePosition = RollInitiative();
        te.owner = this;
    }

    public int RollInitiative()
    {
        int initiative = UnityEngine.Random.Range(100, 120) - speed;
        if (initiative < 1) initiative = 1;
        return initiative;
    }
}
