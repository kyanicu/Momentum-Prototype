using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void HandleIncommingAttack(AttackInfo attackInfo);
}

public class Hurtbox : MonoBehaviour
{

    private IDamageable damageable;

    public void SetDamageable(IDamageable d)
    {
        damageable = d;
    }

    public void HandleIncommingAttack(AttackInfo attackInfo)
    {
        damageable?.HandleIncommingAttack(attackInfo);
    }
}
