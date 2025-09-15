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
    public float health;
    public int speed;

    // health, attack, defense, healthGrowthPerLevel, attackGrowthPerLevel, defenseGrowthPerLevel
    // A list of actions that the unit can obtain / use

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeline = GameObject.Find("Timeline");
        cam = Camera.main.gameObject;
        CreateTimelineEvent();
    }

    // Update is called once per frame
    void Update()
    {
        //sprite.transform.LookAt(cam.transform.position);
    }

    public void CreateTimelineEvent()
    {
        TimelineEvent te = timeline.AddComponent<TimelineEvent>();
        te.timelinePosition = RollInitiative();
        te.owner = this;
    }

    public int RollInitiative()
    {
        int initiative = UnityEngine.Random.Range(1, 6) - speed;
        if (initiative < 0) initiative = 0;
        return initiative;
    }
}
