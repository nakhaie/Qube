using UnityEngine;

public class Configurables
{

    public static Configurables Instance => _instance ?? (_instance = new Configurables());

    private static Configurables _instance;

    public delegate void DelAttackCube(ICube target, int value);
    public delegate void DelFloatValue(float size);
    public delegate void DelForwardEdge(Vector3 pos);
    public delegate void DelChangeWeapon(int index);
    public delegate void DelUseWeapon(EWeaponType weapon, Vector3 pos);

    public event DelAttackCube EvnAttackCube;
    public event DelFloatValue EvnCageChangeSize;
    public event DelForwardEdge EvnForwardEdge;
    public event DelChangeWeapon EvnChangeWeapon;
    public event DelFloatValue EvnAreaTimer;
    public event DelUseWeapon EvnUseWeapon;
    
    public const int ReverseFactor     = -1;
    public const int ProgressionFactor = 2;
    
    public void CallAttackCubeEvent(ICube target, int value)
    {
        EvnAttackCube?.Invoke(target, value);
    }

    public void CallCageChangeSizeEvent(float size)
    {
        EvnCageChangeSize?.Invoke(size);
    }

    public void CallForwardEdgeEvent(Vector3 pos)
    {
        EvnForwardEdge?.Invoke(pos);
    }
    
    public void CallChangeWeaponEvent(int index)
    {
        EvnChangeWeapon?.Invoke(index);
    }

    public void CallAreaTimeEvent(float time)
    {
        EvnAreaTimer?.Invoke(time);
    }
    
    public void CallUseWeaponEvent(EWeaponType weapon, Vector3 pos)
    {
        EvnUseWeapon?.Invoke(weapon, pos);
    }
    
}

public enum EWeaponType
{
    Normal = 0,
    Cutter = 1,
    Area   = 2,
    Linear = 3
}
