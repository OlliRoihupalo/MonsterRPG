using Mono.Cecil;
using NUnit.Framework.Internal;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;
//using static UnityEngine.Rendering.DebugUI;

public class Unit : MonoBehaviour
{
    public GameObject sprite;
    public string unitName;
    public PlayerController playerController;
    public GameObject timeline;
    public GameObject cam;
    public Canvas unitUI;
    public GameObject playerUI;
    public float healthBarLength = 280f;
    public bool downed = false;
    public string faction;
    public int level;
    public float maxHealth;
    public float health;
    public float attackPower;
    public float defense;
    public int speed;
    public float xpYield;
    public float healthGrowthPerLevel;
    public float attackGrowthPerLevel;
    public float defenseGrowthPerLevel;
    public float xpYieldPerLevel;
    public CombatAction[] skillList; // A list of all actions that the unit can learn (through levelusp etc)
    public GameObject highlight;
    public GameObject target;
    public GameObject skills;
    public GameObject buttonPrefab;
    public float healthDisplayDelay = 1.2f;
    public TMPro.TextMeshProUGUI nameDisplay;
    public CombatAction currentAction;
    public GameObject actionDescription;
    public Animator animator;
    private RectTransform healthBar;
    private RectTransform healthDisplay;
    private RectTransform healthLoss;
    private float healthLossAmount;
    private float healthDisplayTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeline = GameObject.Find("Timeline");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        sprite = transform.Find("sprite root").gameObject;
        animator = sprite.transform.Find("Sprite").GetComponent<Animator>();
        unitUI = transform.Find("UnitUI").GetComponent<Canvas>();
        playerUI = transform.Find("PlayerUI").gameObject;
        skills = playerUI.transform.Find("Skills List").Find("Viewport").Find("Content").gameObject;
        healthBar = unitUI.transform.Find("HealthBar").GetComponent<RectTransform>();
        healthDisplay = healthBar.transform.Find("HealthDisplay").GetComponent<RectTransform>();
        healthLoss = healthBar.transform.Find("HealthLossDisplay").GetComponent<RectTransform>();
        highlight = unitUI.transform.Find("Highlight").gameObject;
        target = unitUI.transform.Find("Target").gameObject;
        nameDisplay = playerUI.transform.Find("NameDisplay").GetComponent<TMPro.TextMeshProUGUI>();
        nameDisplay.text = unitName;
        actionDescription = playerUI.transform.Find("Skill Description").gameObject;
        cam = Camera.main.gameObject;
        health = maxHealth;     // The HP of player units should carry over between battles (So they start the battle already damaged if they took damage in aprevious battle)
        healthLossAmount = healthBarLength;
        highlight.SetActive(false);
        target.SetActive(false);
        actionDescription.SetActive(false);
        playerUI.SetActive(false);
        //Assets/Scripts/Unit.cs:61
        CreateSkillList();
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rectTransform = highlight.GetComponent<RectTransform>();
        if (faction == "Player")
        {
            sprite.transform.localScale = new Vector3(-1, 1, 1);
            rectTransform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            sprite.transform.localScale = new Vector3(1, 1, 1);
            rectTransform.localScale = new Vector3(1, 1, 1);
        }

        healthDisplayTimer -= Time.deltaTime;
        if (healthDisplayTimer <= 0 && healthLoss.sizeDelta.x > healthDisplay.sizeDelta.x)
        {
            healthLossAmount -= Time.deltaTime * healthBarLength;
            healthLoss.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthLossAmount);
        }
    }

    public void IncreaseStatsByLevel(int lv)
    {
        if (lv > 0)
        {
            level = lv;
            maxHealth += healthGrowthPerLevel * lv;
            attackPower += attackGrowthPerLevel * lv;
            defense += defenseGrowthPerLevel * lv;
            xpYield += xpYieldPerLevel;
            health = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage < 0)
        {
            downed = false;
            health += Mathf.Abs(damage);
            if (health > maxHealth)
            {
                health = maxHealth;
            }
        }
        else
        {
            health -= damage;
        }

        healthDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Ceil((health / maxHealth) * healthBarLength));
        healthDisplayTimer = healthDisplayDelay;

        if (health <= 0)
        {
            //animator.SetTrigger("Death");
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

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

                int alive = 0;

                foreach (GameObject unit in units)
                {
                    Unit un = unit.GetComponent<Unit>();
                    if (un.faction == "Player" && un.downed == false)
                    {
                        alive++;
                    }
                }

                if (alive == 0)
                {
                    // End combat
                    playerController.EndCombat();
                }
            }
            else
            {
                playerController.score++;
                playerController.currentXP += xpYield;
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
                    playerController.EndCombat();
                }

                Destroy(gameObject);
            }
        }
    }

    public void CreateTimelineEvent()
    {
        if (timeline == null)
        {
            timeline = GameObject.Find("Timeline");
        }
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

    public void CreateSkillList()
    {
        int h = 0;
        foreach (CombatAction ca in skillList)
        {
            if (ca.levelObtained <= level)
            {
                GameObject o = Instantiate(buttonPrefab, skills.transform);

                UnityEngine.UI.Button button = o.GetComponent<UnityEngine.UI.Button>();
                button.onClick.AddListener(delegate () { BeginAction(ca); });

                TMPro.TextMeshProUGUI buttonText = o.transform.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
                buttonText.text = ca.actionName;

                RectTransform rec = o.GetComponent<RectTransform>();
                rec.anchoredPosition = new Vector2(0, 0 - ((rec.rect.height / 2) + (rec.rect.height * h)));
                h++;
            }
        }
        RectTransform s = skills.GetComponent<RectTransform>();
        s.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h * 30);
    }

    public void BeginAction(CombatAction combatAction)
    {
        combatAction.user = this;
        currentAction = combatAction;
        playerController.cancelButton.SetActive(true);
        actionDescription.SetActive(true);
        actionDescription.GetComponent<TMPro.TextMeshProUGUI>().text = combatAction.description;

        playerController.targeting = true;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

        if (combatAction.targetGroup == "Ally" || combatAction.targetGroup == "All")
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

        if (combatAction.targetGroup == "Enemy" || combatAction.targetGroup == "All")
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
}
