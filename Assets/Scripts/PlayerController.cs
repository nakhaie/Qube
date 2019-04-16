using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void DelAttackCube(ICube target, int value);
    public event DelAttackCube EvnAttackCube;

    public enum WeaponType
    {
        Normal = 0,
        Cutter = 1,
        Area   = 2,
        Linear = 3
    }

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private WeaponType _weapon;

    [Header("Area")]
    [SerializeField] private int _areaRang = 2;
    
    private Camera _camera;
    private float _rayDistance;
    private float _curCageScale;
    
    private const int SizeOffset = 2;
    
    private void Awake()
    {
        _camera      = Camera.main;
        _rayDistance = _camera.farClipPlane;
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out var hit, _rayDistance, _layerMask))
            {
                ICube cube;
                
                switch (_weapon)
                {
                    case WeaponType.Normal:
                        
                        cube = hit.transform.GetComponent<ICube>();

                        CallAttackCube(cube, 1);
                        
                        break;
                
                    case WeaponType.Cutter:
                        
                        
                        
                        break;
                
                    case WeaponType.Area:
                        
                        Collider[] units = Physics.OverlapSphere(hit.point, _areaRang);

                        foreach (var unit in units)
                        {
                            cube = unit.transform.GetComponent<ICube>();

                            CallAttackCube(cube, 1);
                        }
                        
                        break;
                
                    case WeaponType.Linear:
                        
                        cube = hit.transform.GetComponent<ICube>();

                        Vector3 startPoint = Vector3.one / 4;
                        startPoint.y = _curCageScale;

                        Collider[] hits = Physics.OverlapBox(cube.GetPosition(), startPoint,
                                                             Quaternion.identity, _layerMask);
                        
                        foreach (Collider part in hits)
                        {
                            cube = part.transform.GetComponent<ICube>();
                            
                            CallAttackCube(cube, 1);
                        }
                        
                        break;
                }
            }

            

            
        }
    }

    public void SetCageScale(float size)
    {
        _curCageScale = size + SizeOffset;
        Debug.Log(_curCageScale);
    }

    private void CallAttackCube(ICube target, int value)
    {
        EvnAttackCube?.Invoke(target, value);
    }
}