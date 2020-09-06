using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStatus
{
    public void HandleTriggerEnter(Collider col)
    {
        if (col.tag == "Crystal")
        {
            GameObject.Destroy(col.gameObject);
        }
    }
}
