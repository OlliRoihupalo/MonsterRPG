using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public string unitName;
    public PlayerController playerController;
    public GameObject timeline;
    public GameObject cam;
    //public GameObject sprite;
    public string faction;
    public float maxHealth;
    public float health;
    public int speed;

    // health, attack, defense, healthGrowthPerLevel, attackGrowthPerLevel, defenseGrowthPerLevel
    // A list of actions that the unit can obtain / use

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeline = GameObject.Find("Timeline");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        cam = Camera.main.gameObject;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        //sprite.transform.LookAt(cam.transform.position);
    }

    public void TakeDamage(float damage)
    {
        /*if (health == maxHealth)
        {
            canvas.gameObject.SetActive(true);
        }*/
        health -= damage;
        //healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Ceil((health / maxHealth) * 30f));
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
