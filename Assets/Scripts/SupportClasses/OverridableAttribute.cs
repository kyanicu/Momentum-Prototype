using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines how a value set modifies another when used as an override
/// </summary>
public enum PlayerOverrideType { Set, Addition, Multiplier }

/// <summary>
/// Holds and hand handles overrideable values 
/// </summary>
[System.Serializable]
public abstract class PlayerOverridableValues
{
    /// <summary>
    /// Number of float based values
    /// </summary>
    protected int floatValuesCount;
    /// <summary>
    /// Number of int based values
    /// </summary>
    protected int intValuesCount;
    /// <summary>
    /// Number of Vector3 based values
    /// </summary>
    protected int vector3ValuesCount;

    /// <summary>
    /// Sets all values to their default based on override type
    /// </summary>
    /// <param name="overrideType"> The type of override </param>
    public void SetDefaultValues(PlayerOverrideType overrideType)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            switch (overrideType)
            {
                case PlayerOverrideType.Addition :
                    SetFloatValue(i, 0);
                    break;
                case PlayerOverrideType.Multiplier :
                    SetFloatValue(i, 1);
                    break;
                case PlayerOverrideType.Set :
                    SetFloatValue(i, float.PositiveInfinity);
                    break;
                default :
                    SetFloatValue(i, 0);
                    break;
            }
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            switch (overrideType)
            {
                case PlayerOverrideType.Addition :
                    SetIntValue(i, 0);
                    break;
                case PlayerOverrideType.Multiplier :
                    SetIntValue(i, 1);
                    break;
                case PlayerOverrideType.Set :
                    SetIntValue(i, int.MaxValue);
                    break;
                default :
                    SetIntValue(i, 0);
                    break;
            }
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            switch (overrideType)
            {
                case PlayerOverrideType.Addition :
                    SetVector3Value(i, Vector3.zero);
                    break;
                case PlayerOverrideType.Multiplier :
                    SetVector3Value(i, Vector3.one);
                    break;
                case PlayerOverrideType.Set :
                    SetVector3Value(i, Vector3.positiveInfinity);
                    break;
                default :
                    SetVector3Value(i, Vector3.zero);
                    break;
            }
        }
    }

    /// <summary>
    /// Adds to this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void AddBy(PlayerOverridableValues v)
    {   
        for (int i = 0; i < floatValuesCount; i++)
        {
            SetFloatValue(i, GetFloatValue(i) + v.GetFloatValue(i));
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            SetIntValue(i, GetIntValue(i) + v.GetIntValue(i));
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            SetVector3Value(i, GetVector3Value(i) + v.GetVector3Value(i));
        }
    }

    /// <summary>
    /// Subtracts this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void SubtractBy(PlayerOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            SetFloatValue(i, GetFloatValue(i) - v.GetFloatValue(i));
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            SetIntValue(i, GetIntValue(i) - v.GetIntValue(i));
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            SetVector3Value(i, GetVector3Value(i) - v.GetVector3Value(i));
        }
    }

    /// <summary>
    /// Multiplies this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void MultiplyBy(PlayerOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            SetFloatValue(i, GetFloatValue(i) * v.GetFloatValue(i));
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            SetIntValue(i, GetIntValue(i) * v.GetIntValue(i));
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            SetVector3Value(i, Vector3.Scale(GetVector3Value(i), v.GetVector3Value(i)));
        }
    }

    /// <summary>
    /// Divides this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void DivideBy(PlayerOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            SetFloatValue(i, GetFloatValue(i) / v.GetFloatValue(i));
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            SetIntValue(i, GetIntValue(i) / v.GetIntValue(i));
        }
        
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            SetVector3Value(i, Vector3.Scale(GetVector3Value(i), new Vector3(1/v.GetVector3Value(i).x, 1/v.GetVector3Value(i).y, 1/v.GetVector3Value(i).z)));
        }
    }

    /// <summary>
    /// Sets this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void SetBy(PlayerOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            SetFloatValue(i, (!float.IsInfinity(v.GetFloatValue(i))) ? v.GetFloatValue(i) : GetFloatValue(i));
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            SetIntValue(i, (v.GetIntValue(i) != int.MaxValue) ? v.GetIntValue(i) : GetIntValue(i));
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            SetVector3Value(i, (!float.IsInfinity(v.GetVector3Value(i).x)) ? v.GetVector3Value(i) : GetVector3Value(i));
        }
    }

    /// <summary>
    /// Unsets this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void UnsetBy(PlayerOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            SetFloatValue(i, (!float.IsInfinity(v.GetFloatValue(i))) ? float.PositiveInfinity : GetFloatValue(i));
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            SetIntValue(i, (v.GetIntValue(i) != int.MaxValue) ? int.MaxValue : GetIntValue(i));
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            SetVector3Value(i, (!float.IsInfinity(v.GetVector3Value(i).x)) ? Vector3.positiveInfinity : GetVector3Value(i));
        }
    }

    /// <summary>
    /// Default constructor used to set value counts by concrete class
    /// </summary>
    public PlayerOverridableValues()
    {
        SetValueCounts();
    }

    /// <summary>
    /// Used by concrete class to set the value counts
    /// </summary>
    protected abstract void SetValueCounts();

    /// <summary>
    /// Returns the appropriate float value 
    /// </summary>
    /// <param name="i"> The float value index</param>
    /// <returns></returns>
    protected abstract float GetFloatValue(int i);
    /// <summary>
    /// Sets the appropriate float value
    /// </summary>
    /// <param name="i"> The float value index </param>
    /// <param name="value"> The value to be set to </param>
    protected abstract void SetFloatValue(int i, float value);
    /// <summary>
    /// Returns the appropriate int value 
    /// </summary>
    /// <param name="i"> The int value index</param>
    /// <returns></returns>
    protected abstract int GetIntValue(int i);
    /// <summary>
    /// Sets the appropriate int value
    /// </summary>
    /// <param name="i"> The float value index </param>
    /// <param name="value"> The value to be set to </param>
    protected abstract void SetIntValue(int i, int value);
    /// <summary>
    /// Returns the appropriate Vector3 value 
    /// </summary>
    /// <param name="i"> The Vector3 value index</param>
    /// <returns></returns>
    protected abstract Vector3 GetVector3Value(int i);
    /// <summary>
    /// Sets the appropriate float value
    /// </summary>
    /// <param name="i"> The float value index </param>
    /// <param name="value"> The value to be set to </param>
    protected abstract void SetVector3Value(int i, Vector3 value);

}

/// <summary>
/// Abstract class representing an attribute of the player that holds values that can be overriden without losing any info on it's base values, or the applied modifications
/// </summary>
/// <typeparam name="Values"> The value set that can be overridden</typeparam>
[System.Serializable]
public abstract class PlayerOverridableAttribute<Values> where Values : PlayerOverridableValues, new()
{
    /// <summary>
    /// The base, unmodified values
    /// </summary>
    [SerializeField]
    protected Values baseValues;

    /// <summary>
    /// The added values
    /// </summary>
    private Values addedValues;

    /// <summary>
    /// The multiplied values
    /// </summary>
    private Values multipliedValues;

    /// <summary>
    /// The set values
    /// Held in a list so multiple sets can be applied without losing any info
    /// </summary>
    private List<Values> setValues;

    /// <summary>
    /// The current total applied values 
    /// </summary>
    protected Values values;

    /// <summary>
    /// Default constructor
    /// Sets all values
    /// </summary>
    public PlayerOverridableAttribute()
    {
        // Instatiate values
        baseValues = new Values();
        addedValues = new Values();
        multipliedValues = new Values();
        setValues = new List<Values>() { new Values() };
        values = new Values();

        // Set Override values to default 
        addedValues.SetDefaultValues(PlayerOverrideType.Addition);
        multipliedValues.SetDefaultValues(PlayerOverrideType.Multiplier);
        setValues[0].SetDefaultValues(PlayerOverrideType.Set);
        values.SetDefaultValues(PlayerOverrideType.Addition);

        // Set base values to their concretely defined default
        SetDefaultBaseValues();
        // Set calculated values (currently euqivalent to base values)
        CalculateValues();
    }

    /// <summary>
    /// Used by concrete class to define the default values of the base values
    /// </summary>
    protected abstract void SetDefaultBaseValues();

    /// <summary>
    /// Calculates the current values based on the base values and their overrides
    /// </summary>
    public void CalculateValues()
    {
        Values set = new Values();
        set.SetDefaultValues(PlayerOverrideType.Set);
        foreach (Values s in setValues)
        {
            set.SetBy(s);
        }

        values.SetDefaultValues(PlayerOverrideType.Addition);
        values.AddBy(baseValues);
        values.SetBy(set);
        values.MultiplyBy(multipliedValues);
        values.AddBy(addedValues);
    }

    /// <summary>
    /// Applies an override to the attribute
    /// </summary>
    /// <param name="overrideValues"> The value set to be applied</param>
    /// <param name="overrideType"> The override type to determine how the value set should be applied</param>
    public void ApplyOverride(Values overrideValues, PlayerOverrideType overrideType)
    {
        switch (overrideType) 
        {
            case (PlayerOverrideType.Addition) :
                addedValues.AddBy(overrideValues); 
                break;
            case (PlayerOverrideType.Multiplier) :
                multipliedValues.MultiplyBy(overrideValues); 
                break;
            case (PlayerOverrideType.Set) :
                setValues.Add(overrideValues); 
                break;
        }
        CalculateValues();
    }

    /// <summary>
    /// Removes an override from the attribute
    /// </summary>
    /// <param name="overrideValues"> The value set to be removed</param>
    /// <param name="overrideType"> The override type to determine how the value set should be removed</param>
    public void RemoveOverride(Values overrideValues, PlayerOverrideType overrideType)
    {
        switch (overrideType) 
        {
            case (PlayerOverrideType.Addition) :
               addedValues.SubtractBy(overrideValues); 
                break;
            case (PlayerOverrideType.Multiplier) :
                multipliedValues.DivideBy(overrideValues); 
                break;
            case (PlayerOverrideType.Set) :
                setValues.Remove(overrideValues); 
                break;
        }
        CalculateValues();
    }

    /// <summary>
    /// To be manually called on MonoBehavior OnValidate() message
    /// </summary>
    public void OnValidate()
    {
        ValidateBaseValues();
        CalculateValues();
    }

    /// <summary>
    /// Used to make sure the base values are set to their default values
    /// </summary>
    protected abstract void ValidateBaseValues();

}