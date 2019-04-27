using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private enum ETouchEffect
    {
        None      = 0,
        OnRelease = 1,
        OnHold    = 2
    }

    [SerializeField] private LayerMask _touchMask;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private EWeaponType _weapon;

    [Header("Area")]
    [SerializeField] private Transform _explosionArea;
    [SerializeField] private int       _areaRange = 2;
    [SerializeField] private float     _areaTime;
    
    [Header("Linear")]
    [SerializeField] private int _linearRange = 2;
    
    [Header("Cutter")]
    [SerializeField] private int _cutterRange = 2;
    [SerializeField] private int _cutterRadius = 1;

    private Camera       _camera;
    private float        _rayDistance;
    private float        _curCageScale;
    private float        _curAreaTime;
    private bool         _firstTouch;
    private Vector3      _lastPoint;
    private ICube        _selectedCube;
    private ETouchEffect _touchEffect;
    
    private const int SizeOffset = 2;

    private Configurables _config;
    
    private void Awake()
    {
        _config      = Configurables.Instance;
        _camera      = Camera.main;
        _rayDistance = _camera.farClipPlane;

        _curAreaTime = 0;
        _touchEffect = ETouchEffect.None;
        _firstTouch = false;

        Input.multiTouchEnabled = false;

        _explosionArea.gameObject.SetActive(false);
        
        _config.EvnCageChangeSize += OnCageChangeSize;
        _config.EvnChangeWeapon   += OnChangeWeapon;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        if (Input.GetMouseButtonDown(0) && !_firstTouch)
        {
            _firstTouch = true;
            _touchEffect = ETouchEffect.None;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out var hit, _rayDistance, _touchMask))
            {
                if (hit.transform.CompareTag("Cube"))
                {
                    _selectedCube = hit.transform.GetComponent<ICube>();
                    _touchEffect = Fire(_selectedCube);
                }
                else
                {
                    Collider[] targetFound = new Collider[1];
                    
                    if (Physics.OverlapSphereNonAlloc(hit.point,1.0f, targetFound,_layerMask) > 0)
                    {
                        _selectedCube = targetFound[0].GetComponent<ICube>();
                        _touchEffect = Fire(_selectedCube);
                    }
                    else
                    {
                        if (_weapon != EWeaponType.Normal)
                        {
                            _selectedCube = GetSimpleCube(hit.point);
                            _touchEffect  = Fire(_selectedCube);
                        }
                    }
                }
            }
            
            //Debug.Log("GetMouseButtonDown: " + _touchEffect);
        }
        else if (Input.GetMouseButtonUp(0) && Input.touchCount < 2)
        {
            _firstTouch = false;

            Debug.Log("GetMouseButtonUp: " + _touchEffect);
            
            switch (_touchEffect)
            {
                case ETouchEffect.OnRelease:

                    Release(_selectedCube);
                    
                    break;
                case ETouchEffect.OnHold:

                    AreaDismiss();
                    
                    break;
            }
        }
        else if(Input.GetMouseButton(0) && _firstTouch)
        {
            switch (_touchEffect)
            {
                case ETouchEffect.OnRelease:

                    Hold(_selectedCube);
                    
                    break;
                case ETouchEffect.OnHold:

                    Hold(_selectedCube);
                    
                    break;
            }
        }
    }

#region Action
    
    private ETouchEffect Fire(ICube cube)
    {
        _config.CallUseWeaponEvent(_weapon, cube.GetPosition());
        
        switch (_weapon)
        {
            case EWeaponType.Normal:
                NormalAttack(cube);
                return ETouchEffect.None;

            case EWeaponType.Cutter:
                _lastPoint = cube.GetPosition();
                
                GizmoCenter = cube.GetPosition();
                GizmoSize = cube.GetPosition();
                
                return ETouchEffect.OnRelease;

            case EWeaponType.Area:
                _explosionArea.gameObject.SetActive(true);
                _explosionArea.position = cube.GetPosition();
                _explosionArea.localScale = Vector3.one * _areaRange * 2;
                _curAreaTime = 0;
                
                GizmoCenter = cube.GetPosition();
                GizmoSize.x = _areaRange;
                
                return ETouchEffect.OnHold;

            case EWeaponType.Linear:
                LinearAttack(cube.GetPosition());
                return ETouchEffect.None;
        }

        return ETouchEffect.None;
    }

    private void Release(ICube selectedCube)
    {
        switch (_weapon)
        {
            case EWeaponType.Cutter:
                CutterAttack(selectedCube.GetPosition());
                break;

            case EWeaponType.Area:
                AreaDismiss();
                break;
        }
    }

    private void Hold(ICube selectedCube)
    {
        switch (_weapon)
        {
            case EWeaponType.Cutter:

                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out var hit, _rayDistance, _touchMask))
                {
                    _lastPoint = hit.point;
                    GizmoSize  = _lastPoint;
                }

                break;

            case EWeaponType.Area:
                _curAreaTime += Time.deltaTime;

                float timer = _curAreaTime / _areaTime;

                timer = 1 - timer;
                
                _config.CallAreaTimeEvent(timer);
                
                if (_curAreaTime >= _areaTime)
                {
                    AreaAttack(selectedCube.GetPosition());
                    AreaDismiss();
                }
                
                break;
        }
    }
    
    private void NormalAttack(ICube cube)
    {
        _config.CallAttackCubeEvent(cube, 1);

        GizmoCenter = cube.GetPosition();
        GizmoSize   = Vector3.one;
    }

    private Vector3 _forwardEdge;
    
    private void CutterAttack(Vector3 start)
    {
        if(start.y < _forwardEdge.y)
        {
            if (start.x < _forwardEdge.x)
            {
                _lastPoint.z = start.z;
            }
            else if(start.z < _forwardEdge.z)
            {
                _lastPoint.x = start.x;
            }
            else
            {
                if(Math.Abs(_lastPoint.x - _forwardEdge.x) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.x = start.x;
                }
            
                if(Math.Abs(_lastPoint.z - _forwardEdge.z) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.z = start.z;
                }
            }
        }
        else
        {
            if(start == _forwardEdge)
            {
                if (Math.Abs(_lastPoint.y - _forwardEdge.y) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.y = start.y;
                }
                else
                {
                    if(Math.Abs(_lastPoint.z - _forwardEdge.z) <= Configurables.HalfCubeSize)
                    {
                        _lastPoint.z = start.z;
                    }
                    
                    if(Math.Abs(_lastPoint.x - _forwardEdge.x) <= Configurables.HalfCubeSize)
                    {
                        _lastPoint.x = start.x;
                    }
                }
            }
            else if(Math.Abs(start.x - _forwardEdge.x) < Configurables.HalfCubeSize)
            {
                if (Math.Abs(_lastPoint.x - _forwardEdge.x) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.x = start.x;
                }
                
                if(Math.Abs(_lastPoint.y - _forwardEdge.y) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.y = start.y;
                }
            }
            else if(Math.Abs(start.z - _forwardEdge.z) < Configurables.HalfCubeSize)
            {
                if (Math.Abs(_lastPoint.z - _forwardEdge.z) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.z = start.z;
                }
                
                if(Math.Abs(_lastPoint.y - _forwardEdge.y) <= Configurables.HalfCubeSize)
                {
                    _lastPoint.y = start.y;
                }
            }
            else
            {
                _lastPoint.y = start.y;
            }
        }

        Vector3 dir = _lastPoint - start;

        var distance  = dir.magnitude;
        var direction = dir / distance;

        GizmoCenter = start;
        GizmoSize   = direction;

        Ray ray = new Ray(start, direction);

        var hits = Physics.SphereCastAll(ray, _cutterRadius * Configurables.CutterFactor,
                                         _cutterRange - 1, _layerMask);
        //var hits = Physics.RaycastAll(ray, _cutterRange - 1, _layerMask);
        
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                ICube cube = hit.transform.GetComponent<ICube>();
                
                _config.CallAttackCubeEvent(cube, 1);
            }
        }
        
    }
    
    private void AreaAttack(Vector3 center)
    {
        Collider[] units = Physics.OverlapSphere(center, _areaRange, _layerMask);
       
        foreach (var unit in units)
        {
            ICube cube = unit.transform.GetComponent<ICube>();

            _config.CallAttackCubeEvent(cube, 1);
        }
    }

    private void LinearAttack(Vector3 center)
    {
        const float extentsFactor = 2.0f;
        Vector3 offsetSize = Vector3.one / 10;
                
        Vector3 halfExtents = (Vector3.one / extentsFactor) - offsetSize;
                
        float linearScale = (_linearRange / extentsFactor) - 0.05f;
                
        if (center.y < _forwardEdge.y)
        {
            halfExtents.y = _curCageScale;

            if (center.x < center.z)
            {
                halfExtents.x = linearScale;
            }
            else
            {
                halfExtents.z = linearScale;
            }
        }
        else
        {
            if (center.x > center.z)
            {
                halfExtents.x = _curCageScale;
                halfExtents.z = linearScale;
            }
            else
            {
                halfExtents.z = _curCageScale;
                halfExtents.x = linearScale;
            }
        }


        GizmoCenter = center;
        GizmoSize   = halfExtents * 2;
                
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, _layerMask);

        foreach (Collider part in hits)
        {
            ICube cube = part.transform.GetComponent<ICube>();

            _config.CallAttackCubeEvent(cube, 1);
        }
    }

    private void AreaDismiss()
    {
        _curAreaTime = 0;
        _explosionArea.gameObject.SetActive(false);
        _config.CallAreaTimeEvent(_curAreaTime);
        _touchEffect = ETouchEffect.None;
    }

    private ICube GetSimpleCube(Vector3 point)
    {
        point.x = Mathf.CeilToInt(point.x);
        point.y = Mathf.CeilToInt(point.y);
        point.z = Mathf.CeilToInt(point.z);
        
        return new SimpleCube(point);
    }
    
#endregion
    
#region EventHandler

    private void OnCageChangeSize(float size)
    {
        _curCageScale = size + SizeOffset;
    }

    private void OnChangeWeapon(int index)
    {
        _weapon = (EWeaponType) index;
    }
    
    
#endregion

    private Vector3 GizmoCenter;
    private Vector3 GizmoSize;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        
        switch (_weapon)
        {
            case EWeaponType.Normal:
                
                Gizmos.DrawWireCube(GizmoCenter, GizmoSize);
                
                break;
            
            case EWeaponType.Cutter:

                float rang = _cutterRange - 1;
                
                var direction = GizmoSize * rang;
                
                Gizmos.DrawRay(GizmoCenter, direction);

                rang *= 2;
                direction /= rang;
                
                for (int i = 0; i < rang; i++)
                {
                    Vector3 pos = GizmoCenter + (direction * i);
                    Gizmos.DrawWireSphere(pos, _cutterRadius * Configurables.CutterFactor);
                }
                
                Gizmos.DrawWireSphere(GizmoCenter + direction * rang, _cutterRadius * Configurables.CutterFactor);
                
                
                break;
            
            case EWeaponType.Area:
                Gizmos.DrawWireSphere(GizmoCenter, GizmoSize.x);
                break;
            
            case EWeaponType.Linear:
                
                Gizmos.DrawWireCube(GizmoCenter, GizmoSize);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Gizmos.color = Color.white;

        if (_selectedCube != null)
        {
            Gizmos.DrawCube(_selectedCube.GetPosition(), Vector3.one);
        }
        
        

        
    }
}