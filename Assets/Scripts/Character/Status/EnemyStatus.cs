using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct EnemyDropInfo
{
     float dropRate;
}

public class EnemyStatus : CharacterStatus
{

    [SerializeField]
    EnemyDropInfo dropInfo;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Down()
    {
        Debug.Log("The enemy \"\"Fainted\"\"");
        //Destroy(gameObject);
    }
}
