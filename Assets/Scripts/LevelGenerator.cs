using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private Transform  _groundCollider;
    [SerializeField] private float _screenScaleFactor = 8;
    
    [SerializeField] private LevelProperty _levelProperty;

    private readonly Dictionary<int, Vector3Int[]> _cubesInLayers = new Dictionary<int, Vector3Int[]>();
    private readonly Queue<ICube>                  _cubesPool     = new Queue<ICube>();

    private bool _win;
    
    
    private Vector3 _cameraZoom;
    private float _colorClamp;

    private int _curLayer;
    private int _curTotalCubes;

    private ICube[] _backLayerCubes;

    private Camera        _camera;
    private Configurables _config;
    
    private void Awake()
    {
        _camera = Camera.main;
        _config = Configurables.Instance;
        
        _colorClamp = 1.0f / (_levelProperty.CageLayers.Length - 1);

        _config.EvnAttackCube += OnAttackCube;
        _config.EvnDestroyCube += OnDestroyCube;

        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        CreatLayers(_levelProperty.CageLayers.Length);

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

        _cameraZoom = ZoomCamera(_levelProperty.CageLayers.Length);
        _camera.transform.localPosition = _cameraZoom;

        _cameraPivot.eulerAngles = new Vector3(45,45,0);
        
        _curLayer   = _cubesInLayers.Keys.Count - 1;
        MakeGroundCollider(_curLayer);
        _config.CallCageChangeSizeEvent(GetCageSize(_cubesInLayers.Keys.Count));

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
            MakeGroundCollider(_curLayer);
            _config.CallCageChangeSizeEvent(GetCageSize(_cubesInLayers.Keys.Count));

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

            _cameraZoom = ZoomCamera(_cubesInLayers.Keys.Count);
        }
        else if (Vector3.Distance(_cameraZoom,_camera.transform.localPosition) > 0.1f)
        {
            _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _cameraZoom,
                                                           Time.deltaTime * 2);
        }
    }

#region Init

    private void CreatLayers(int layerCount)
    {
        List<Vector3Int> totalLayerPos = new List<Vector3Int>();
        
        for (int i = 0; i < layerCount; i++)
        {
            List<Vector3Int> layerPos = new List<Vector3Int>();
            
            for (int x = -i; x <= i; x++)
            {
                for (int y = -i; y <= i; y++)
                {
                    for (int z = -i; z <= i; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);

                        if (!totalLayerPos.Contains(pos))
                        {
                            layerPos.Add(pos);
                            totalLayerPos.Add(pos);
                        }
                    }
                }
            }

            _cubesInLayers.Add(i, layerPos.ToArray());
        }
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

    private Vector3 ZoomCamera(int layerIndex)
    {
        Vector3 cameraPos = Vector3.zero;
        cameraPos.z = _screenScaleFactor * layerIndex * -1;

        return cameraPos;
    }

    private float GetCageSize(int layerIndex)
    {
        layerIndex--;

        Vector3 forwardEdge = Vector3.one * layerIndex;
        Vector3 rightEdge   = forwardEdge;

        rightEdge.x = layerIndex * Configurables.ReverseFactor;

        float betweenTwoEdge = Vector3.Distance(rightEdge, forwardEdge);

        return betweenTwoEdge;
    }

    private void MakeGroundCollider(int layerIndex)
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

            _groundCollider.position = Vector3.zero;
            _groundCollider.localScale = Vector3.one + (Vector3.one * (layerIndex  * Configurables.ProgressionFactor));
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