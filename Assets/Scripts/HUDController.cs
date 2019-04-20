using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Toggle[] _weaponOptions;
    [SerializeField] private Image _areaTime;
    [SerializeField] private Vector2 _timerOffset;
    
    private Configurables _config;

    private void Awake()
    {
        _config = Configurables.Instance;

        _config.EvnAreaTimer += OnAreaTimer;
        _config.EvnUseWeapon += OnUseWeapon;
    }

    private void Start()
    {
        foreach (Toggle option in _weaponOptions)
        {
            option.onValueChanged.AddListener(OnSelectWeapon);
        }

        _areaTime.enabled = false;

        OnSelectWeapon(true);
    }

    private void OnSelectWeapon(bool value)
    {
        if(!value)
            return;
        
        int index = 0;
        
        for (int i = 0; i < _weaponOptions.Length; i++)
        {
            if (!_weaponOptions[i].isOn)
                continue;
            
            index = i;
            break;
        }
        
        
        _config.CallChangeWeaponEvent(index);
    }

    private void OnAreaTimer(float time)
    {
        if (time > 0)
        {
            if(!_areaTime.enabled)
            {
                _areaTime.enabled = true;
            }
        }
        else if(_areaTime.enabled)
        {
            _areaTime.enabled = false;
        }
        
        _areaTime.fillAmount = time;
    }

    private void OnUseWeapon(EWeaponType weapon, Vector3 pos)
    {
        switch (weapon)
        {
            case EWeaponType.Normal:
                
                break;
            
            case EWeaponType.Cutter:
                
                break;
            
            case EWeaponType.Area:

                Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta;
                
                Vector2 viewportPosition = Camera.main.WorldToViewportPoint(pos);
                viewportPosition = new Vector2(((viewportPosition.x * sizeDelta.x) - (sizeDelta.x * 0.5f)),
                                               ((viewportPosition.y * sizeDelta.y) - (sizeDelta.y * 0.5f)));

                viewportPosition += _timerOffset;
                
                _areaTime.rectTransform.anchoredPosition = viewportPosition;
                
                break;
            
            case EWeaponType.Linear:
                
                break;
        }
    }
}
