using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Toggle[] WeaponOptions;
    
    private Configurables _config;

    private void Awake()
    {
        _config = Configurables.Instance;
    }

    private void Start()
    {
        foreach (Toggle option in WeaponOptions)
        {
            option.onValueChanged.AddListener(OnSelectWeapon);
        }

        OnSelectWeapon(true);
    }

    private void OnSelectWeapon(bool value)
    {
        if(!value)
            return;
        
        int index = 0;
        
        for (int i = 0; i < WeaponOptions.Length; i++)
        {
            if (!WeaponOptions[i].isOn)
                continue;
            
            index = i;
            break;
        }
        
        
        _config.CallChangeWeaponEvent(index);
    }
}
