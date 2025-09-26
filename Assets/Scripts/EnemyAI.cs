using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public PlayerController playerController;
    public Unit unit;
    public float actionDelay = 1.2f;
    private float actionDelayTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        unit = GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.activeUnit == unit && unit.faction == "Enemy" && playerController.inCombat == true)
        {
            actionDelayTimer -= Time.deltaTime;
            if (actionDelayTimer <= 0)
            {
                actionDelayTimer = actionDelay;
                ChooseAndPerformAction();
                playerController.TimelineEventEnd();
            }
        }
        else
        {
            actionDelayTimer = actionDelay;
        }
    }

    public void ChooseAndPerformAction()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        bool targetFound = false;
        CombatAction chosen = FindAction(true);
        Unit target = null;

        bool isHealing = false;
        foreach (string t in chosen.actionTags)
        {
            if (t == "Healing")
            {
                isHealing = true;
            }
        }

        if (isHealing == true)
        {
            foreach (GameObject uni in units)
            {
                Unit u = uni.GetComponent<Unit>();
                if (u.faction == unit.faction && u.health < u.maxHealth) // Check for allies that are missing health
                {
                    targetFound = true;
                    target = u;
                }
            }
            if (targetFound == false)
            {
                isHealing = false;
                chosen = FindAction(false);
            }
        }

        if (targetFound == false)
        {
            int temp = 0;
            Unit[] enemies = new Unit[units.Length];

            foreach (GameObject uni in units)
            {
                Unit u = uni.GetComponent<Unit>();
                if (u.faction != unit.faction && !u.downed)
                {
                    enemies[temp] = u;
                    temp++;
                }
            }
            foreach (Unit enemy in enemies)
            {
                if (enemy != null && enemy.health < enemy.maxHealth) // Check for enemies that are missing health
                {
                    targetFound = true;
                    target = enemy;
                }
            }
            if (targetFound == false)
            {
                target = enemies[UnityEngine.Random.Range(0, temp)];
            }
        }
        PerformActionOnTargets(chosen, units, target);
    }

    public CombatAction FindAction(bool includeHealingSkills)
    {
        CombatAction[] useableSkills = new CombatAction[unit.skillList.Length];
        int i = 0;

        foreach (CombatAction ca in unit.skillList)
        {
            if (ca.levelObtained <= unit.level)
            {
                bool isHealing = false;
                if (!includeHealingSkills)
                {
                    foreach (string t in ca.actionTags)
                    {
                        if (t == "Healing")
                        {
                            isHealing = true;
                        }
                    }
                }

                if (isHealing == false)
                {
                    useableSkills[i] = ca;
                    i++;
                }
            }
        }

        if (i == 0)
        {
            return FindAction(false);
        }
        else
        {
            return useableSkills[UnityEngine.Random.Range(0, i)];
        }
    }

    public void PerformActionOnTargets(CombatAction action, GameObject[] unitList, Unit primaryTarget)
    {
        action.user = unit;
        print(unit.unitName + " uses " + action.actionName + "!");
        if (action.cleave == true)
        {
            Unit[] targets = new Unit[unitList.Length];
            int i = 0;
            foreach (GameObject uni in unitList)
            {
                Unit u = uni.GetComponent<Unit>();
                if (u.faction == primaryTarget.faction || action.targetGroup == "All")
                {
                    targets[i] = u;
                    i++;
                }
            }
            action.Perform(targets);
        }
        else
        {
            Unit[] targets = new Unit[1];
            targets[0] = primaryTarget;
            action.Perform(targets);
        }
    }
}
