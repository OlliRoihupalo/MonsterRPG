using Mono.Cecil;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public class Unit : MonoBehaviour
{
    //public GameObject sprite;
    public string unitName;
    public PlayerController playerController;
    public GameObject timeline;
    public GameObject cam;
    public Canvas unitUI;
    public GameObject playerUI;
    public float healthBarLength = 280f;
    public bool downed = false;
    public string faction;
    public float maxHealth;
    public float health;
    public int speed;
    public GameObject highlight;
    public GameObject target;
    public float healthDisplayDelay = 1.2f;
    public TMPro.TextMeshProUGUI nameDisplay;
    private RectTransform healthBar;
    private RectTransform healthDisplay;
    private RectTransform healthLoss;
    private float healthLossAmount;
    private float healthDisplayTimer;

    // health, attack, defense, healthGrowthPerLevel, attackGrowthPerLevel, defenseGrowthPerLevel, level, currentXP, nextLevelXPRequirement
    // A list of actions that the unit can obtain / use

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeline = GameObject.Find("Timeline");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        unitUI = transform.Find("UnitUI").GetComponent<Canvas>();
        playerUI = transform.Find("PlayerUI").gameObject;
        healthBar = unitUI.transform.Find("HealthBar").GetComponent<RectTransform>();
        healthDisplay = healthBar.transform.Find("HealthDisplay").GetComponent<RectTransform>();
        healthLoss = healthBar.transform.Find("HealthLossDisplay").GetComponent<RectTransform>();
        highlight = unitUI.transform.Find("Highlight").gameObject;
        target = unitUI.transform.Find("Target").gameObject;
        nameDisplay = playerUI.transform.Find("NameDisplay").GetComponent<TMPro.TextMeshProUGUI>();
        nameDisplay.text = gameObject.name;
        cam = Camera.main.gameObject;
        health = maxHealth;     // The HP of player units should carry over between battles (So they start the battle already damaged if they took damage in aprevious battle)
        healthLossAmount = healthBarLength;
        highlight.SetActive(false);
        target.SetActive(false);
        playerUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //sprite.transform.LookAt(cam.transform.position);
        healthDisplayTimer -= Time.deltaTime;
        if (healthDisplayTimer <= 0 && healthLoss.sizeDelta.x > healthDisplay.sizeDelta.x)
        {
            healthLossAmount -= Time.deltaTime * healthBarLength;
            healthLoss.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthLossAmount);
        }
    }

    public void TakeDamage(float damage)
    {
        /*if (health == maxHealth)
        {
            canvas.gameObject.SetActive(true);
        }*/
        health -= damage;
        healthDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Ceil((health / maxHealth) * healthBarLength));
        healthDisplayTimer = healthDisplayDelay;
        if (health <= 0)
        {
            //animator.SetTrigger("Death");

            TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();

            foreach (TimelineEvent e in events)
            {
                if (e.owner == this)
                {
                    Destroy(timeline.GetComponentAtIndex(e.GetComponentIndex()));
                }
            }

            if (faction == "Player")
            {
                // Character is downed?
                downed = true;
                print("Man down!");
            }
            else
            {
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
                    print("Combat over");
                }

                Destroy(gameObject);
            }
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

    public void BeginAction(string targetGroup) // Should probably create a new script for actions and give that script as the parameter
    {
        playerController.targeting = true;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

        if (targetGroup == "Ally" || targetGroup == "All")
        {
            foreach (GameObject unit in units)
            {
                Unit un = unit.GetComponent<Unit>();
                if (un.faction == faction)
                {
                    un.target.SetActive(true);
                }
            }
        }

        if (targetGroup == "Enemy" || targetGroup == "All")
        {
            foreach (GameObject unit in units)
            {
                Unit un = unit.GetComponent<Unit>();
                if (un.faction != faction)
                {
                    un.target.SetActive(true);
                }
            }
        }
    }

    public void PerformAction(Unit[] targets, string effect, float value)
    {
        //GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
    }
}
