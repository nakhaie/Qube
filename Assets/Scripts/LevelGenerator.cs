using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private Transform  _groundCollider;

    [SerializeField] private LevelProperty _levelProperty;

    private readonly Dictionary<int, Vector3Int[]> _cubesInLayers = new Dictionary<int, Vector3Int[]>();
    private readonly Queue<ICube>                  _cubesPool     = new Queue<ICube>();

    private bool _win;

    private float _screenScale => _levelProperty.CageDistance.x + _screenScaleFactor * _cubesInLayers.Keys.Count;
    private float _screenScaleFactor;
    private float _cameraZoom;
    private float _colorClamp;

    private int _curLayer;
    private int _curTotalCubes;

    private Vector2 _screenRatio;
    private ICube[] _backLayerCubes;

    private Camera        _camera;
    private Configurables _config;
    
    private void Awake()
    {
        _camera = Camera.main;
        _config = Configurables.Instance;

        _screenRatio   = new Vector2(0.0f, _camera.orthographicSize * 2);
        _screenRatio.x = Screen.width * _screenRatio.y / Screen.height;

        _screenScaleFactor = _levelProperty.CageDistance.y - _levelProperty.CageDistance.x;
        _screenScaleFactor /= _levelProperty.CageLayers.Length;

        _colorClamp = 1.0f / (_levelProperty.CageLayers.Length - 1);

        _config.EvnAttackCube += OnAttackCube;
        _config.EvnDestroyCube += OnDestroyCube;

        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        CreatLayers(_levelProperty.CageLayers.Length, _levelProperty.BasicSize);

        _curLayer = _levelProperty.CageLayers.Length - 1;
        int totalCubes;

        if (_levelProperty.CageLayers.Length > 1)
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

        if (_levelProperty.CageLayers.Length > 1)
        {
            _backLayerCubes = InstantiateLayer(_curLayer - 1, false);
        }

        SetupCamera(_camera, _screenRatio, _levelProperty.CageLayers.Length, _levelProperty.BasicSize,
                    _screenScale, _levelProperty.CameraDistance);

        _cameraZoom = _camera.orthographicSize;
        _curLayer   = _cubesInLayers.Keys.Count - 1;
        MakeGroundCollider(_curLayer, _levelProperty.BasicSize);
        _config.CallCageChangeSizeEvent(GetCageSize(_cubesInLayers.Keys.Count,
                                                    _levelProperty.BasicSize));

        _win = false;
    }

    private void Update()
    {
        if (_win)
            return;

        if (_curTotalCubes < 1)
        {
            _cubesInLayers.Remove(_curLayer);
            _curLayer--;
            MakeGroundCollider(_curLayer, _levelProperty.BasicSize);
            _config.CallCageChangeSizeEvent(GetCageSize(_cubesInLayers.Keys.Count,
                                                        _levelProperty.BasicSize));

            if (_cubesInLayers.Count < 1)
            {
                _win = true;
                Start();
                return;
            }

            _curTotalCubes = _cubesInLayers[_curLayer].Length;

            if (_backLayerCubes != null)
            {
                foreach (var unit in _backLayerCubes)
                {
                    unit.HasEnabled = true;
                }
            }

            _backLayerCubes = _curLayer > 0 ? InstantiateLayer(_curLayer - 1, false) : null;

            _cameraZoom = ZoomCamera(_screenRatio, _cubesInLayers.Keys.Count,_levelProperty.BasicSize,
                                     _screenScale);
        }
        else if (Mathf.Abs(_cameraZoom - _camera.orthographicSize) > 0.1f)
        {
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _cameraZoom, Time.deltaTime * 2);
        }
    }

#region Init

    private void CreatLayers(int layerCount, int basicScale)
    {
        for (int i = 0; i < layerCount; i++)
        {
            int rowCount = basicScale + (Configurables.ProgressionFactor * i);

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
                Vector3Int pos = (new Vector3Int(j, k, 0) * Configurables.ReverseFactor) + layerDepth;

                if (!result.Contains(pos))
                {
                    result.Add(pos);
                }

                pos = (new Vector3Int(0, j, k) * Configurables.ReverseFactor) + layerDepth;

                if (!result.Contains(pos))
                {
                    result.Add(pos);
                }

                pos = (new Vector3Int(k, 0, j) * Configurables.ReverseFactor) + layerDepth;

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
        while (_cubesPool.Count <= totalCubes)
        {
            ICube cube = Instantiate(_cubePrefab, Vector3.zero, Quaternion.identity).GetComponent<ICube>();

            cube.HasActive = false;

            _cubesPool.Enqueue(cube);
        }
    }

    private void SetupCamera(Camera cameraProperty, Vector2 screenRatio, int layerIndex, int basicScale,
                                    float cageDistance, float cameraDistance)
    {
        Vector3 forwardEdge = Vector3.one * (layerIndex - 1);
        Vector3 cameraPos   = forwardEdge + (Vector3.one * cameraDistance);

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

            switch (_levelProperty.GraphicType)
            {
                case ELayerGraphicType.Material:
                    
                    Material cubeMaterial = _levelProperty.CageLayers[layerIndex];
                    
                    cube.Init($"Unit_{layerIndex}_{i}", _cubesInLayers[layerIndex][i],
                              _levelProperty.CageLayers[layerIndex,0], cubeMaterial);

                    break;

                case ELayerGraphicType.Gradient:

                    Color cubeColor = Color.LerpUnclamped(_levelProperty.CageLayers.CageGradient.StartColor,
                                                          _levelProperty.CageLayers.CageGradient.EndColor,
                                                          _colorClamp * layerIndex);

                    cube.Init($"Unit_{layerIndex}_{i}", _cubesInLayers[layerIndex][i],
                              _levelProperty.CageLayers[layerIndex,0],
                              _levelProperty.CageLayers.CageMaterials.DefaultMaterial, cubeColor);

                    break;
            }

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
        Vector3 rightEdge   = forwardEdge;
        Vector3 leftEdge    = forwardEdge;

        rightEdge.x = (layerIndex + basicScale) * Configurables.ReverseFactor;
        leftEdge.z  = (layerIndex + basicScale) * Configurables.ReverseFactor;

        float betweenTwoEdge = Vector3.Distance(rightEdge, leftEdge) + cageDistance;
        float cameraSize     = (betweenTwoEdge * screenRatio.y) / screenRatio.x;

        cameraSize /= Configurables.ProgressionFactor;
        
        _config.CallForwardEdgeEvent(forwardEdge);

        return cameraSize;
    }

    private float GetCageSize(int layerIndex, int basicScale)
    {
        layerIndex--;
        basicScale--;

        Vector3 forwardEdge = Vector3.one * layerIndex;
        Vector3 rightEdge   = forwardEdge;

        rightEdge.x = (layerIndex + basicScale) * Configurables.ReverseFactor;

        float betweenTwoEdge = Vector3.Distance(rightEdge, forwardEdge);

        return betweenTwoEdge;
    }

    private void MakeGroundCollider(int layerIndex, int basicScale)
    {
        layerIndex--;

        if (layerIndex < 0)
        {
            _groundCollider.gameObject.SetActive(false);
        }
        else
        {
            if (!_groundCollider.gameObject.activeSelf)
            {
                _groundCollider.gameObject.SetActive(true);
            }
            
            _groundCollider.position   = Vector3.one * (0.5f * (basicScale - 1)) * Configurables.ReverseFactor;
            _groundCollider.localScale = Vector3.one * (basicScale + layerIndex  * Configurables.ProgressionFactor);
        }
    }

#endregion

#region EventHandler

    private void OnDestroyCube(ICube cube)
    {
        cube.DestroyBody();

        _cubesPool.Enqueue(cube);

        _curTotalCubes--;
    }

    private void OnAttackCube(ICube cube, int damage)
    {
        if (!cube.Damaged(damage))
            return;

        _config.CallDestroyCubeEvent(cube);
    }

#endregion
}

public enum ELayerGraphicType
{
    Material = 0,
    Gradient = 1
}

[System.Serializable]
public struct LevelProperty
{
    [Header("Cage Size")]
    public int     BasicSize;
    public Vector2 CageDistance;
    public float   CameraDistance;

    [Header("Cage Layers")]
    public ELayerGraphicType GraphicType;
    public CageLayerProperty CageLayers;
}

[System.Serializable]
public struct CageLayerProperty
{
    public int Length => CubesHp.Length;

    public Material this[int i] => CageMaterials.CubeMaterial.Length > i
                                   ? CageMaterials.CubeMaterial[i]
                                   : CageMaterials.DefaultMaterial;
    
    public int this[int i, int defaultValue] => Length > i
                                                ? CubesHp[i]
                                                : defaultValue;

    public int[]              CubesHp;
    public CageLayerGradient  CageGradient;
    public CageLayerMaterials CageMaterials;
}

[System.Serializable]
public struct CageLayerMaterials
{
    public Material   DefaultMaterial;
    public Material[] CubeMaterial;
}

[System.Serializable]
public struct CageLayerGradient
{
    public Color StartColor;
    public Color EndColor;
}