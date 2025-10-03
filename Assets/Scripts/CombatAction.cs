using UnityEngine;

[CreateAssetMenu(fileName = "CombatAction", menuName = "Scriptable Objects/CombatAction")]
public class CombatAction : ScriptableObject
{
    public string actionName;
    public string description;
    public string[] actionTags;
    public int levelObtained;
    public string targetGroup;
    public bool cleave;
    public bool ignoreDefense;
    public string usesStat;
    public float statMultiplier;
    public float statValue;
    public Unit user;
    public GameObject particleSystem;
    public string particleOrigin;
    public bool rotateToTarget;
    public Vector3 originPoint;
    public Vector3 visualEffectOffset;
    public Quaternion visualEffectRotation;

    public virtual void Perform(Unit[] targets)
    {
        Debug.Log(targets.Length);
    }

    public void DetermineStat()
    {
        if (usesStat != null)
        {
            switch (usesStat)
            {
                case "attackPower":
                    statValue = user.attackPower;
                    break;
                case "defense":
                    statValue = user.defense;
                    break;
                case "maxHealth":
                    statValue = user.maxHealth;
                    break;
                case "health":
                    statValue = user.health;
                    break;
                case "speed":
                    statValue = user.speed;
                    break;
                default:
                    // code block
                    break;
            }
        }
        else
        {
            statValue = 0;
        }
    }
}
