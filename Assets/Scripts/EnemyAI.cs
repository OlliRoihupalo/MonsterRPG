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
        if (playerController.activeUnit == unit)
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
        //int[] useableSkills;
        int i = 0;
        foreach (CombatAction ca in unit.skillList)
        {
            if (ca.levelObtained <= unit.level)
            {
                // Select the skill as a candidate for the skill to use
                i++;
            }
        }
    }
}
