using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum WeaponType
    {
        Normal = 0,
        Cutter = 1,
        Area   = 2,
        Linear = 3
    }

    [SerializeField] private LayerMask _touchMask;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private WeaponType _weapon;

    [Header("Area")]
    [SerializeField] private int _areaRange = 2;
    [SerializeField] private int _linearRange = 2;

    private Camera  _camera;
    private float   _rayDistance;
    private float   _curCageScale;
    private Vector3 _forwardEdge;
    
    private const int SizeOffset = 2;

    private Configurables _config;
    
    private void Awake()
    {
        _config      = Configurables.Instance;
        _camera      = Camera.main;
        _rayDistance = _camera.farClipPlane;

        _config.EvnCageChangeSize += OnCageChangeSize;
        _config.EvnForwardEdge    += OnForwardEdge;
        _config.EvnChangeWeapon   += OnChangeWeapon;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out var hit, _rayDistance, _touchMask))
            {
                if (hit.transform.CompareTag("Cube"))
                {
                    Fire(hit.transform.GetComponent<ICube>());
                }
                else
                {
                    Collider[] targetFound = new Collider[1];
                
                    if (Physics.OverlapSphereNonAlloc(hit.point,1.0f, targetFound,_layerMask) > 0)
                    {
                        Fire(targetFound[0].GetComponent<ICube>());
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            
        }
    }

#region Action
    
    private void Fire(ICube cube)
    {
        switch (_weapon)
        {
            case WeaponType.Normal:
                NormalAttack(cube);
                break;

            case WeaponType.Cutter:
                CutterAttack(cube);
                break;

            case WeaponType.Area:
                AreaAttack(cube);
                break;

            case WeaponType.Linear:
                LinearAttack(cube);
                break;
        }
    }

    private void NormalAttack(ICube cube)
    {
        _config.CallAttackCubeEvent(cube, 1);

        GizmoCenter = cube.GetPosition();
        GizmoSize   = Vector3.one;
    }

    private void CutterAttack(ICube cube)
    {
        
    }
    
    private void AreaAttack(ICube cube)
    {
        Collider[] units = Physics.OverlapSphere(cube.GetPosition(), _areaRange, _layerMask);

        GizmoCenter = cube.GetPosition();
        GizmoSize.x = _areaRange;
                
        foreach (var unit in units)
        {
            cube = unit.transform.GetComponent<ICube>();

            _config.CallAttackCubeEvent(cube, 1);
        }
    }

    private void LinearAttack(ICube cube)
    {
        const float extentsFactor = 2.0f;
                
        Vector3 startPoint = (Vector3.one / extentsFactor) - (Vector3.one / 10);
                
        float linearScale = (_linearRange / extentsFactor) - 0.05f;
                
        if (cube.GetPosition().y < _forwardEdge.y)
        {
            startPoint.y = _curCageScale;

            if (cube.GetPosition().x < cube.GetPosition().z)
            {
                startPoint.x = linearScale;
            }
            else
            {
                startPoint.z = linearScale;
            }
        }
        else
        {
            if (cube.GetPosition().x > cube.GetPosition().z)
            {
                startPoint.x = _curCageScale;
                startPoint.z = linearScale;
            }
            else
            {
                startPoint.z = _curCageScale;
                startPoint.x = linearScale;
            }
        }


        GizmoCenter = cube.GetPosition();
        GizmoSize   = startPoint * 2;
                
        Collider[] hits = Physics.OverlapBox(cube.GetPosition(), startPoint,
                                             Quaternion.identity, _layerMask);

        foreach (Collider part in hits)
        {
            cube = part.transform.GetComponent<ICube>();

            _config.CallAttackCubeEvent(cube, 1);
        }
    }

#endregion
    
#region EventHandler

    private void OnCageChangeSize(float size)
    {
        _curCageScale = size + SizeOffset;
    }

    private void OnForwardEdge(Vector3 pos)
    {
        _forwardEdge = pos;
    }

    private void OnChangeWeapon(int index)
    {
        _weapon = (WeaponType) index;
    }
    
    
#endregion

    private Vector3 GizmoCenter;
    private Vector3 GizmoSize;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        
        switch (_weapon)
        {
            case WeaponType.Normal:
                
                Gizmos.DrawWireCube(GizmoCenter, GizmoSize);
                
                break;
            
            case WeaponType.Cutter:
                break;
            
            case WeaponType.Area:
                Gizmos.DrawWireSphere(GizmoCenter, GizmoSize.x);
                break;
            
            case WeaponType.Linear:
                
                Gizmos.DrawWireCube(GizmoCenter, GizmoSize);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        
    }
}