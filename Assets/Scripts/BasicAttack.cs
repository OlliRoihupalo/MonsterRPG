using UnityEditor.PackageManager;
using UnityEngine;
[CreateAssetMenu(fileName = "BasicAttack", menuName = "Scriptable Objects/BasicAttack")]
public class BasicAttack : CombatAction
{
    public override void Perform(Unit[] targets)
    {
        DetermineStat();

        if (particleOrigin != null)
        {
            if (particleOrigin == "user" || particleOrigin == "User")
            {
                originPoint = user.gameObject.transform.position;
            }
        }

        foreach (Unit target in targets)
        {
            if (target != null)
            {
                if (particleOrigin == null || particleOrigin == "target" || particleOrigin == "Target")
                {
                    originPoint = target.gameObject.transform.position;
                }
                GameObject o = Instantiate(particleSystem, originPoint + visualEffectOffset, visualEffectRotation);
                if (rotateToTarget)
                {
                    o.transform.LookAt(target.gameObject.transform.position + visualEffectOffset);
                }
                target.TakeDamage(statValue * statMultiplier);
            }
        }
        user = null;
    }
}
