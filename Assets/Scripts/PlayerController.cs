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

    private Camera       _camera;
    private float        _rayDistance;
    private float        _curCageScale;
    private float        _curAreaTime;
    private bool         _firstTouch;
    private Vector3      _forwardEdge;
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
        _config.EvnForwardEdge    += OnForwardEdge;
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
                }
            }
            
            //Debug.Log("GetMouseButtonDown: " + _touchEffect);
        }
        else if (Input.GetMouseButtonUp(0) && Input.touchCount < 1)
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
            //Debug.Log("Hold: " + _touchEffect);
            
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
                LinearAttack(cube);
                return ETouchEffect.None;
        }

        return ETouchEffect.None;
    }

    private void Release(ICube selectedCube)
    {
        switch (_weapon)
        {
            case EWeaponType.Cutter:
                CutterAttack(selectedCube);
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
                    AreaAttack(selectedCube);
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

    private void CutterAttack(ICube cube)
    {
        if(cube.GetPosition().y < _forwardEdge.y)
        {
            if (cube.GetPosition().x < _forwardEdge.x)
            {
                _lastPoint.z = cube.GetPosition().z;
            }
            else if(cube.GetPosition().z < _forwardEdge.z)
            {
                _lastPoint.x = cube.GetPosition().x;
            }
            else
            {
                if(Math.Abs(_lastPoint.x - _forwardEdge.x) <= 0.5f)
                {
                    _lastPoint.x = cube.GetPosition().x;
                }
            
                if(Math.Abs(_lastPoint.z - _forwardEdge.z) <= 0.5f)
                {
                    _lastPoint.z = cube.GetPosition().z;
                }
            }
        }
        else
        {
            if(cube.GetPosition() == _forwardEdge)
            {
                if (Math.Abs(_lastPoint.y - _forwardEdge.y) <= 0.5f)
                {
                    _lastPoint.y = cube.GetPosition().y;
                }
                else
                {
                    if(Math.Abs(_lastPoint.z - _forwardEdge.z) <= 0.5f)
                    {
                        _lastPoint.z = cube.GetPosition().z;
                    }
                    
                    if(Math.Abs(_lastPoint.x - _forwardEdge.x) <= 0.5f)
                    {
                        _lastPoint.x = cube.GetPosition().x;
                    }
                }
            }
            else if(Math.Abs(cube.GetPosition().x - _forwardEdge.x) < 0.5f)
            {
                if (Math.Abs(_lastPoint.x - _forwardEdge.x) <= 0.5f)
                {
                    _lastPoint.x = cube.GetPosition().x;
                }
                
                if(Math.Abs(_lastPoint.y - _forwardEdge.y) <= 0.5f)
                {
                    _lastPoint.y = cube.GetPosition().y;
                }
            }
            else if(Math.Abs(cube.GetPosition().z - _forwardEdge.z) < 0.5f)
            {
                if (Math.Abs(_lastPoint.z - _forwardEdge.z) <= 0.5f)
                {
                    _lastPoint.z = cube.GetPosition().z;
                }
                
                if(Math.Abs(_lastPoint.y - _forwardEdge.y) <= 0.5f)
                {
                    _lastPoint.y = cube.GetPosition().y;
                }
            }
            else
            {
                _lastPoint.y = cube.GetPosition().y;
            }
        }

        Vector3 dir = _lastPoint - cube.GetPosition();

        var distance  = dir.magnitude;
        var direction = dir / distance;

        _config.CallAttackCubeEvent(cube, 1);
        
        GizmoCenter = cube.GetPosition();
        GizmoSize   = direction;
        
        Ray ray = new Ray(cube.GetPosition(), direction);

        var hits = Physics.RaycastAll(ray, _cutterRange, _layerMask);
        
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                cube = hit.transform.GetComponent<ICube>();
                
                _config.CallAttackCubeEvent(cube, 1);
            }
        }
        
    }
    
    private void AreaAttack(ICube cube)
    {
        Collider[] units = Physics.OverlapSphere(cube.GetPosition(), _areaRange, _layerMask);
       
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

    private void AreaDismiss()
    {
        _curAreaTime = 0;
        _explosionArea.gameObject.SetActive(false);
        _config.CallAreaTimeEvent(_curAreaTime);
        _touchEffect = ETouchEffect.None;
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

                /*GizmoSize.x = GizmoCenter.x;
                
                Vector3 dir = GizmoSize - GizmoCenter;
                
                var distance  = dir.magnitude;
                var direction = dir / distance;

                direction *= _cutterRange;*/
                
                var direction = GizmoSize * _cutterRange;
                
                Gizmos.DrawRay(GizmoCenter, direction);
                
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

        
    }
}