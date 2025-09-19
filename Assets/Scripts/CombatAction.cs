using UnityEngine;

[CreateAssetMenu(fileName = "CombatAction", menuName = "Scriptable Objects/CombatAction")]
public class CombatAction : ScriptableObject
{
    public string actionName;
    public string[] actionTags;
    public int levelObtained;
    public string targetGroup;
    public bool cleave;
    public Unit user;
    //public ParticleSystem ps;

    public virtual void Perform(Unit[] targets)
    {
        Debug.Log(targets.Length);
    }
}
