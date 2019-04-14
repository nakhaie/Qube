using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private int _basicSize = 2;
    [SerializeField] private Vector2 _cageDistance;
    [SerializeField] private float _cameraDistance = 1;
    [SerializeField] private LayerMask _layerMask;
    
    [Header("Cage Layers")]
    [SerializeField] private CageLayer[] _cageLayers;

    private readonly Dictionary<int, Vector3Int[]> _cubesInLayers = new Dictionary<int, Vector3Int[]>();
    private readonly Queue<ICube> _cubesPool = new Queue<ICube>();
    
    private const int ReverseFactor = -1;
    private const int ProgressionFactor = 2;

    private Vector2 _screenRatio;

    private float _screenScale => _cageDistance.x + _screenScaleFactor * _cubesInLayers.Keys.Count;
    private float _screenScaleFactor;
    private float _cameraZoom;

    private int _curLayer;
    private int _curTotalCubes;

    private bool _win;
    
    private ICube[] _backLayerCubes;
    
    private void Awake()
    {
        _screenRatio = new Vector2(0.0f, _camera.orthographicSize * 2);
        _screenRatio.x = Screen.width * _screenRatio.y / Screen.height;

        _screenScaleFactor = _cageDistance.y - _cageDistance.x;
        _screenScaleFactor /= _cageLayers.Length;
    }

    private void Start()
    {
        CreatLayers(_cageLayers, _basicSize);

        _curLayer = _cageLayers.Length - 1;
        int totalCubes;
        
        if (_cageLayers.Length > 1)
        {
            totalCubes = _cubesInLayers[_curLayer].Length + _cubesInLayers[_curLayer - 1].Length;
        }
        else
        {
            totalCubes = _cubesInLayers[_curLayer].Length;
        }
        
        InstantiatePool(totalCubes);

        InstantiateLayer(_curLayer, true);

        _curTotalCubes = _cubesInLayers[_curLayer].Length;
        
        if (_cageLayers.Length > 1)
        {
            _backLayerCubes = InstantiateLayer(_curLayer - 1, false);
        }

        SetupCamera(_camera, _screenRatio,_cageLayers.Length, _basicSize, _screenScale,
                    _cameraDistance);

        _cameraZoom = _camera.orthographicSize;
        _curLayer = _cubesInLayers.Keys.Count - 1;
    }

    private void Update()
    {
        if(_win)
            return;
        
        if (Input.GetMouseButton(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 100, _layerMask))
            {
                Collider[] units = Physics.OverlapSphere(hit.point, 5);

                foreach (var unit in units)
                {
                    ICube cube = unit.transform.GetComponent<ICube>();
                    cube.DestroyBody();

                    _curTotalCubes--;
                
                    _cubesPool.Enqueue(cube);
                }
            }
        }
        
        if (_curTotalCubes < 1)
        {
            _cubesInLayers.Remove(_curLayer);
            _curLayer--;

            if (_cubesInLayers.Count < 1)
            {
                _win = true;
                return;
            }
            
            Debug.Log(_cubesInLayers.Count);
            
            _curTotalCubes = _cubesInLayers[_curLayer].Length;
  
            if (_backLayerCubes != null)
            {
                foreach (var unit in _backLayerCubes)
                {
                    unit.HasEnabled = true;
                }
            }
            
            _backLayerCubes = _curLayer > 0 ? InstantiateLayer(_curLayer - 1, false) : null;

            _cameraZoom = ZoomCamera(_screenRatio, _cubesInLayers.Keys.Count, _basicSize,
                                    _screenScale);
        }
        else if(Math.Abs(_cameraZoom - _camera.orthographicSize) > 0.1f)
        {
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _cameraZoom, Time.deltaTime * 2);
        }
        
    }
    
    #region Init
    
    private void CreatLayers(IReadOnlyCollection<CageLayer> layers, int basicScale)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            int rowCount = basicScale + (ProgressionFactor * i);

            Vector3Int depth = Vector3Int.one * i;

            Vector3Int[] cubesPosList = CalculateLayer(rowCount, depth);

            _cubesInLayers.Add(i, cubesPosList);
        }
    }
    
    private Vector3Int[] CalculateLayer(int layerScale, Vector3Int layerDepth)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        for (int j = 0; j < layerScale; j++)
        {
            for (int k = 0; k < layerScale; k++)
            {
                Vector3Int pos = (new Vector3Int(j, k, 0) * ReverseFactor) + layerDepth;

                if (!result.Contains(pos))
                {
                    result.Add(pos);
                }

                pos = (new Vector3Int(0, j, k) * ReverseFactor) + layerDepth;

                if (!result.Contains(pos))
                {
                    result.Add(pos);
                }

                pos = (new Vector3Int(k, 0, j) * ReverseFactor) + layerDepth;

                if (!result.Contains(pos))
                {
                    result.Add(pos);
                }
            }
        }

        return result.ToArray();
    }
    
    private void InstantiatePool(int totalCubes)
    {
        for (int i = 0; i < totalCubes; i++)
        {
            ICube cube = Instantiate(_cubePrefab, Vector3.zero , Quaternion.identity).GetComponent<ICube>();

            cube.HasActive = false;
            
            _cubesPool.Enqueue(cube);
        }
    }

    private void SetupCamera(Camera cameraProperty, Vector2 screenRatio, int layerIndex, int basicScale,
                            float cageDistance, float cameraDistance)
    {
        Vector3 forwardEdge = Vector3.one * (layerIndex - 1);
        Vector3 cameraPos = forwardEdge + (Vector3.one * cameraDistance);

        cameraProperty.transform.position = cameraPos;
        cameraProperty.transform.LookAt(forwardEdge);

        cameraProperty.orthographicSize = ZoomCamera(screenRatio, layerIndex, basicScale, cageDistance);
    }

    #endregion

    #region Action
    
    private ICube[] InstantiateLayer(int layerIndex, bool cubesEnabled)
    {     
        ICube[] result = new ICube[_cubesInLayers[layerIndex].Length];
        
        for (int i = 0; i < result.Length; i++)
        {
            ICube cube = _cubesPool.Dequeue();
            
            cube.Init($"Unit_{layerIndex}_{i}", _cubesInLayers[layerIndex][i], _cageLayers[layerIndex].CubeHp,
                _cageLayers[layerIndex].CubeMaterial);

            cube.HasEnabled = cubesEnabled;

            result[i] = cube;
        }

        return result;
    }
    
    private float ZoomCamera(Vector2 screenRatio, int layerIndex, int basicScale,
                            float cageDistance)
    {
        layerIndex--;
        basicScale--;
        
        Vector3 forwardEdge = Vector3.one * layerIndex;
        Vector3 rightEdge = forwardEdge;
        Vector3 leftEdge = forwardEdge;

        rightEdge.x = (layerIndex + basicScale) * ReverseFactor;
        leftEdge.z = (layerIndex + basicScale) * ReverseFactor;
        
        float betweenTwoEdge = Vector3.Distance(rightEdge, leftEdge) + cageDistance;
        float cameraSize = (betweenTwoEdge * screenRatio.y) / screenRatio.x;

        cameraSize /= ProgressionFactor;

        return cameraSize;
    }
    
    #endregion
}

[System.Serializable]
public struct CageLayer
{
    public Material CubeMaterial;
    public int CubeHp;
}
