using UnityEditor.PackageManager;
using UnityEngine;
[CreateAssetMenu(fileName = "BasicAttack", menuName = "Scriptable Objects/BasicAttack")]
public class BasicAttack : CombatAction
{
    public override void Perform(Unit[] targets)
    {
        DetermineStat();

        foreach (Unit target in targets)
        {
            if (target != null)
            {
                target.TakeDamage(statValue * statMultiplier);
            }
        }
    }
}
