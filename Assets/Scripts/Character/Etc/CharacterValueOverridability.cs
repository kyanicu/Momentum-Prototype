using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Determines how a value set modifies another when used as an override
/// </summary>
public enum ValueOverrideType { Set, Addition, Multiplier }

/// <summary>
/// Holds and hand handles overrideable values 
/// </summary>
[System.Serializable]
public abstract class CharacterOverridableValues
{
    /// <summary>
    /// Number of float based values
    /// </summary>
    private int floatValuesCount;
    /// <summary>
    /// Number of int based values
    /// </summary>
    private int intValuesCount;
    /// <summary>
    /// Number of Vector3 based values
    /// </summary>
    private int vector3ValuesCount;

    protected virtual float[] floatValues { get { return new float[0]; } set {  }  }
    protected virtual int[] intValues { get { return new int[0]; } set {  }  }
    protected virtual Vector3[] vector3Values { get { return new Vector3[0]; } set {  }  }

    private float[] tempFloatValues;
    private int[] tempIntValues;
    private Vector3[] tempVector3Values;

    /// <summary>
    /// Sets all values to their default based on override type
    /// </summary>
    /// <param name="overrideType"> The type of override </param>
    public void SetDefaultValues(ValueOverrideType overrideType)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            switch (overrideType)
            {
                case ValueOverrideType.Addition :
                    tempFloatValues[i] = 0;
                    break;
                case ValueOverrideType.Multiplier :
                    tempFloatValues[i] = 1;
                    break;
                case ValueOverrideType.Set :
                    tempFloatValues[i] = float.PositiveInfinity;
                    break;
                default :
                    tempFloatValues[i] = 0;
                    break;
            }
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            switch (overrideType)
            {
                case ValueOverrideType.Addition :
                    tempIntValues[i] = 0;
                    break;
                case ValueOverrideType.Multiplier :
                    tempIntValues[i] = 1;
                    break;
                case ValueOverrideType.Set :
                    tempIntValues[i] = int.MaxValue;
                    break;
                default :
                    tempIntValues[i] = 0;
                    break;
            }
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            switch (overrideType)
            {
                case ValueOverrideType.Addition :
                    tempVector3Values[i] = Vector3.zero;
                    break;
                case ValueOverrideType.Multiplier :
                    tempVector3Values[i] = Vector3.one;
                    break;
                case ValueOverrideType.Set :
                    tempVector3Values[i] = Vector3.positiveInfinity;
                    break;
                default :
                    tempVector3Values[i] = Vector3.zero;
                    break;
            }
        }
        SetValuesByTemp();
    }

    /// <summary>
    /// Adds to this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void AddBy(CharacterOverridableValues v)
    {   
        for (int i = 0; i < floatValuesCount; i++)
        {
            tempFloatValues[i] = floatValues[i] + v.floatValues[i];
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            tempIntValues[i] = intValues[i] + v.intValues[i];
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            tempVector3Values[i] = vector3Values[i] + v.vector3Values[i];
        }
        SetValuesByTemp();
    }

    /// <summary>
    /// Subtracts this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void SubtractBy(CharacterOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            tempFloatValues[i] = floatValues[i] - v.floatValues[i];
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            tempIntValues[i] = intValues[i] - v.intValues[i];
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            tempVector3Values[i] = vector3Values[i] - v.vector3Values[i];
        }
        SetValuesByTemp();
    }

    /// <summary>
    /// Multiplies this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void MultiplyBy(CharacterOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            tempFloatValues[i] = floatValues[i] * v.floatValues[i];
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            tempIntValues[i] = intValues[i] * v.intValues[i];
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            tempVector3Values[i] = Vector3.Scale(vector3Values[i], v.vector3Values[i]);
        }
        SetValuesByTemp();
    }

    /// <summary>
    /// Divides this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void DivideBy(CharacterOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            tempFloatValues[i] = floatValues[i] / v.floatValues[i];
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            tempIntValues[i] = intValues[i] / v.intValues[i];
        }
        
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            tempVector3Values[i] = Vector3.Scale(vector3Values[i], new Vector3(1/v.vector3Values[i].x, 1/vector3Values[i].y, 1/v.vector3Values[i].z));
        }
        SetValuesByTemp();
    }

    /// <summary>
    /// Sets this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void SetBy(CharacterOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            tempFloatValues[i] = (!float.IsInfinity(v.floatValues[i])) ? v.floatValues[i] : floatValues[i];
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            tempIntValues[i] = (v.intValues[i] != int.MaxValue) ? v.intValues[i] : intValues[i];
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            tempVector3Values[i] = (!float.IsInfinity(v.vector3Values[i].x)) ? v.vector3Values[i] : vector3Values[i];
        }
        SetValuesByTemp();
    }

    /// <summary>
    /// Unsets this value set by another
    /// </summary>
    /// <param name="v"> The other value set </param>
    public void UnsetBy(CharacterOverridableValues v)
    {
        for (int i = 0; i < floatValuesCount; i++)
        {
            tempFloatValues[i] = (!float.IsInfinity(v.floatValues[i])) ? float.PositiveInfinity : floatValues[i];
        }

        for (int i = 0; i < intValuesCount; i++)
        {
            tempIntValues[i] = (v.intValues[i] != int.MaxValue) ? int.MaxValue : intValues[i];
        }
        
        for (int i = 0; i < vector3ValuesCount; i++)
        {
            tempVector3Values[i] = (!float.IsInfinity(v.vector3Values[i].x)) ? Vector3.positiveInfinity : vector3Values[i];
        }
        SetValuesByTemp();
    }

    public void ResetTempValues()
    {
        tempFloatValues = floatValues;
        tempIntValues = intValues;
        tempVector3Values = vector3Values;
    }

    public void SetValuesByTemp()
    {
        floatValues = tempFloatValues;
        intValues = tempIntValues;
        vector3Values = tempVector3Values;
    }

    /// <summary>
    /// Default constructor used to set value counts by concrete class
    /// </summary>
    public CharacterOverridableValues()
    {
        floatValuesCount = floatValues.Length;
        intValuesCount = intValues.Length;
        vector3ValuesCount = vector3Values.Length;

        ResetTempValues();
    }
}

/// <summary>
/// Abstract class representing an attribute of the player that holds values that can be overriden without losing any info on it's base values, or the applied modifications
/// </summary>
/// <typeparam name="Values"> The value set that can be overridden</typeparam>
[System.Serializable]
public sealed class CharacterOverridableAttribute<Values> :ISerializationCallbackReceiver where Values : CharacterOverridableValues, new()
{
    /// <summary>
    /// The base, unmodified values
    /// </summary>
    [SerializeField]
    private Values _baseValues = new Values();
    public Values baseValues { get { return _baseValues; } private set { _baseValues = value; } }

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
    public Values values { get; private set; }

    /// <summary>
    /// Default constructor
    /// Sets all values
    /// </summary>
    public CharacterOverridableAttribute()
    {
        // Instatiate values
        //baseValues = new Values();
        addedValues = new Values();
        multipliedValues = new Values();
        setValues = new List<Values>() { new Values() };
        values = new Values();

        // Set Override values to default 
        addedValues.SetDefaultValues(ValueOverrideType.Addition);
        multipliedValues.SetDefaultValues(ValueOverrideType.Multiplier);
        setValues[0].SetDefaultValues(ValueOverrideType.Set);
        values.SetDefaultValues(ValueOverrideType.Addition);
    }

    /// <summary>
    /// Calculates the current values based on the base values and their overrides
    /// </summary>
    public void CalculateValues()
    {
        Values set = new Values();
        set.SetDefaultValues(ValueOverrideType.Set);
        foreach (Values s in setValues)
        {
            set.SetBy(s);
        }

        values.SetDefaultValues(ValueOverrideType.Addition);
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
    public void ApplyOverride(Values overrideValues, ValueOverrideType overrideType)
    {
        switch (overrideType) 
        {
            case (ValueOverrideType.Addition) :
                addedValues.AddBy(overrideValues); 
                break;
            case (ValueOverrideType.Multiplier) :
                multipliedValues.MultiplyBy(overrideValues); 
                break;
            case (ValueOverrideType.Set) :
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
    public void RemoveOverride(Values overrideValues, ValueOverrideType overrideType)
    {
        switch (overrideType) 
        {
            case (ValueOverrideType.Addition) :
               addedValues.SubtractBy(overrideValues); 
                break;
            case (ValueOverrideType.Multiplier) :
                multipliedValues.DivideBy(overrideValues); 
                break;
            case (ValueOverrideType.Set) :
                setValues.Remove(overrideValues); 
                break;
        }
        CalculateValues();
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        // Set calculated values (currently equivalent to base values)
        CalculateValues();
    }

}

public interface ICharacterValueOverridabilityCommunication
{
    void RegisterOverridability<Values>(CharacterOverridableAttribute<Values> attribute) where Values : CharacterOverridableValues, new();

    void DeregisterOverridability<Values>(CharacterOverridableAttribute<Values> attribute) where Values : CharacterOverridableValues, new();

    void ApplyOverride<Values>(Values values, ValueOverrideType type) where Values : CharacterOverridableValues, new();

    void RemoveOverride<Values>(Values values, ValueOverrideType type) where Values : CharacterOverridableValues, new();

    void ApplyFullMovementOverride(FullMovementOverride overrides);

    void RemoveFullMovementOverride(FullMovementOverride overrides);
}

[System.Serializable]
public struct MutableTuple<i,j>
{
    public i item1;
    public j item2;

    public MutableTuple(i i1,j i2)
    {
        item1 = i1;
        item2 = i2;
    }
}

[System.Serializable]
public struct FullMovementOverride
{
    [SerializeField]
    public List<MutableTuple<CharacterMovementValues, ValueOverrideType>> movementOverrides;

    [SerializeField]
    public List<MutableTuple<MomentumMovementPhysicsValues, ValueOverrideType>> physicsOverrides;

    [SerializeField]
    public List<MutableTuple<MomentumMovementActionValues, ValueOverrideType>> actionOverrides;


    [SerializeField]
    public List<MutableTuple<AlestaMovementAbilityValues, ValueOverrideType>> alestaAbilityOverrides;

    [SerializeField]
    public List<MutableTuple<NephuiMovementAbilityValues, ValueOverrideType>> nephuiAbilityOverrides;

    [SerializeField]
    public List<MutableTuple<CartiaMovementAbilityValues, ValueOverrideType>> cartiaAbilityOverrides;

    [SerializeField]
    public List<MutableTuple<IlphineMovementAbilityValues, ValueOverrideType>> ilphineAbilityOverrides;
}

public class CharacterValueOverridability : MonoBehaviour, ICharacterValueOverridabilityCommunication
{
    private Dictionary<Type, object> attributeByType = new Dictionary<Type, object>();

    public void RegisterOverridability<Values>(CharacterOverridableAttribute<Values> attribute) where Values : CharacterOverridableValues, new()
    {
        attributeByType.Add(attribute.GetType(), attribute);
    }

    public void DeregisterOverridability<Values>(CharacterOverridableAttribute<Values> attribute) where Values : CharacterOverridableValues, new()
    {
        attributeByType.Remove(attribute.GetType());
    }

    public void ApplyOverride<Values>(Values values, ValueOverrideType type) where Values : CharacterOverridableValues, new() 
    {
        (attributeByType[typeof(CharacterOverridableAttribute<Values>)] as CharacterOverridableAttribute<Values>)?.ApplyOverride(values, type);
    }

    public void RemoveOverride<Values>(Values values, ValueOverrideType type) where Values : CharacterOverridableValues, new() 
    {
        (attributeByType[typeof(CharacterOverridableAttribute<Values>)] as CharacterOverridableAttribute<Values>)?.ApplyOverride(values, type);
    }

    public void ApplyFullMovementOverride(FullMovementOverride overrides)
    {
        foreach (MutableTuple<CharacterMovementValues, ValueOverrideType> o in overrides.movementOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<MomentumMovementPhysicsValues, ValueOverrideType> o in overrides.physicsOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<MomentumMovementActionValues, ValueOverrideType> o in overrides.actionOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<AlestaMovementAbilityValues, ValueOverrideType> o in overrides.alestaAbilityOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<NephuiMovementAbilityValues, ValueOverrideType> o in overrides.nephuiAbilityOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<CartiaMovementAbilityValues, ValueOverrideType> o in overrides.cartiaAbilityOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<IlphineMovementAbilityValues, ValueOverrideType> o in overrides.ilphineAbilityOverrides)
        {
            ApplyOverride(o.item1, o.item2);
        }
    }

    public void RemoveFullMovementOverride(FullMovementOverride overrides)
    {
        foreach (MutableTuple<CharacterMovementValues, ValueOverrideType> o in overrides.movementOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<MomentumMovementPhysicsValues, ValueOverrideType> o in overrides.physicsOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<MomentumMovementActionValues, ValueOverrideType> o in overrides.actionOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<AlestaMovementAbilityValues, ValueOverrideType> o in overrides.alestaAbilityOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<NephuiMovementAbilityValues, ValueOverrideType> o in overrides.nephuiAbilityOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<CartiaMovementAbilityValues, ValueOverrideType> o in overrides.cartiaAbilityOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
        
        foreach (MutableTuple<IlphineMovementAbilityValues, ValueOverrideType> o in overrides.ilphineAbilityOverrides)
        {
            RemoveOverride(o.item1, o.item2);
        }
    }

}