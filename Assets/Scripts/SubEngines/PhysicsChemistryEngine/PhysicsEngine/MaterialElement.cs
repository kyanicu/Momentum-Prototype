using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct MaterialElementState
{
    

    bool CheckIfSimple()
    {
        return true;
    }
}

public class MaterialElement : MonoBehaviour
{
    MaterialElementState state;

    bool simpleElement = true;


    
}
