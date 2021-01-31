using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    private Dictionary<int, Dictionary<IDamageable, HashSet<Hitbox>>> hitboxGroupOverlaps = new Dictionary<int, Dictionary<IDamageable, HashSet<Hitbox>>>();

    private Dictionary<Hitbox, List<Hurtbox>> hitboxOverlaps = new Dictionary<Hitbox, List<Hurtbox>>();

    private Dictionary<(int, IDamageable), Coroutine> waitingAttacks = new Dictionary<(int, IDamageable), Coroutine>();

    ////public bool ValidHit(int hitboxGroup, Hurtbox hurtbox)
    ////{
    ////    return (!hitboxGroupOverlaps.ContainsKey(hitboxGroup) || !hitboxGroupOverlaps[hitboxGroup].ContainsKey(hurtbox.damageable));
    ////}
    
    void HandleAttack(Hitbox hitbox, Hurtbox hurtbox)
    {      
        if (waitingAttacks.ContainsKey((hitbox.hitboxGroup, hurtbox.damageable)))
        {
            StopCoroutine(waitingAttacks[(hitbox.hitboxGroup, hurtbox.damageable)]);
            waitingAttacks.Remove((hitbox.hitboxGroup, hurtbox.damageable));
        }
        
        switch (hurtbox.damageable.ValidHit(hitbox, hurtbox))
        {
            case (HitValidity.VALID):
                hitbox.HandleOutgoingAttack(hurtbox);
                hurtbox.HandleIncommingAttack(hitbox);
                if (hitbox.attackInfo.continousDamageRate != 0)
                    waitingAttacks[(hitbox.hitboxGroup, hurtbox.damageable)] = StartCoroutine(AttackAfterTime(hitbox, hurtbox, hitbox.attackInfo.continousDamageRate));
                else if (hitbox.attackInfo.allowHitAfterIFrames)
                    waitingAttacks[(hitbox.hitboxGroup, hurtbox.damageable)] = StartCoroutine(AttackAfterIFrames(hitbox, hurtbox));
                break;
            case (HitValidity.IFRAME):
                if (hitbox.attackInfo.allowHitAfterIFrames)
                    waitingAttacks[(hitbox.hitboxGroup, hurtbox.damageable)] = StartCoroutine(AttackAfterIFrames(hitbox, hurtbox));
                break;
            ////case (HitValidity.BLOCK):
            ////    break;
            ////case (HitValidity.PHASE):
            ////    break;
            ////case (HitValidity.TEMPORARY_IMMUNITY):
            ////    break;
            ////case (HitValidity.PERMANENT_IMMUNITY):
            ////    break;
            default :
                break;
        }
    }

    IEnumerator AttackAfterTime(Hitbox hitbox, Hurtbox hurtbox, float time)
    {
        yield return new WaitForSeconds(time);
        HandleAttack(hitbox, hurtbox);
    }

    IEnumerator AttackAfterIFrames(Hitbox hitbox, Hurtbox hurtbox)
    {
        yield return new WaitWhile(() => hurtbox.damageable.ValidHit(hitbox, hurtbox) == HitValidity.IFRAME);
        HandleAttack(hitbox, hurtbox);
    }

    public void RegisterOverlap(Hitbox hitbox, Hurtbox hurtbox)
    {
        Debug.Log("Overlap Registered" + hitbox);

        if (!hitboxGroupOverlaps.ContainsKey(hitbox.hitboxGroup))
            hitboxGroupOverlaps.Add(hitbox.hitboxGroup, new Dictionary<IDamageable, HashSet<Hitbox>>());

        if (!hitboxGroupOverlaps[hitbox.hitboxGroup].ContainsKey(hurtbox.damageable))
        {
            hitboxGroupOverlaps[hitbox.hitboxGroup].Add(hurtbox.damageable, new HashSet<Hitbox> { hitbox });
        
            HandleAttack(hitbox, hurtbox);
        }
        else
            hitboxGroupOverlaps[hitbox.hitboxGroup][hurtbox.damageable].Add(hitbox);

        if (!hitboxOverlaps.ContainsKey(hitbox))
            hitboxOverlaps.Add(hitbox, new List<Hurtbox> { hurtbox });
        else
            hitboxOverlaps[hitbox].Add(hurtbox);
    }

    public void DeregisterOverlap(Hitbox hitbox, Hurtbox hurtbox)
    {
        Debug.Log("Overlap Deregistered" + hitbox);

        Dictionary<IDamageable, HashSet<Hitbox>> overlaps = hitboxGroupOverlaps[hitbox.hitboxGroup];
        HashSet<Hitbox> hitboxes = overlaps[hurtbox.damageable];
        hitboxes.Remove(hitbox);
        if (hitboxGroupOverlaps[hitbox.hitboxGroup][hurtbox.damageable].Count == 0)
        {
            hitboxGroupOverlaps[hitbox.hitboxGroup].Remove(hurtbox.damageable);

            if(waitingAttacks.ContainsKey((hitbox.hitboxGroup, hurtbox.damageable)))
            {
                waitingAttacks.Remove((hitbox.hitboxGroup, hurtbox.damageable));
                StopCoroutine(waitingAttacks[(hitbox.hitboxGroup, hurtbox.damageable)]);
            }

            if (hitboxGroupOverlaps[hitbox.hitboxGroup].Count == 0)
                hitboxGroupOverlaps.Remove(hitbox.hitboxGroup);
        }

        hitboxOverlaps[hitbox].Remove(hurtbox);
        if (hitboxOverlaps[hitbox].Count == 0)
            hitboxOverlaps.Remove(hitbox);
            
    }

    public void DeregisterAllOverlaps(Hitbox hitbox)
    {
        if (hitboxOverlaps.ContainsKey(hitbox))
        {
            List<Hurtbox> hurtboxes = hitboxOverlaps[hitbox];
            int len = hurtboxes.Count;
            for (int i = 0; i > len; i++)
            {
                DeregisterOverlap(hitbox, hurtboxes[i]);
            }
        }
    }
}