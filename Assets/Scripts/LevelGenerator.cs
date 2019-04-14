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
    
    [Header("Cage Layers")]
    [SerializeField] private CageLayer[] _cageLayers;

    private readonly Dictionary<int, IList<ICube>> _cubesInLayers = new Dictionary<int, IList<ICube>>();

    private const int ReverseFactor = -1;
    private const int ProgressionFactor = 2;

    private Vector2 _screenRatio;

    private float _screenScale => _cageDistance.x + _screenScaleFactor * _cubesInLayers.Keys.Count;
    private float _screenScaleFactor;
    private float _cameraZoom;

    private int _curLayer;
    
    private void Awake()
    {
        _screenRatio = new Vector2(0.0f, _camera.orthographicSize * 2);
        _screenRatio.x = Screen.width * _screenRatio.y / Screen.height;

        _screenScaleFactor = _cageDistance.y - _cageDistance.x;
        _screenScaleFactor /= _cageLayers.Length;
    }

    // Start is called before the first frame update
    private void Start()
    {
        CreatLayers(_cageLayers, _basicSize);
        
        SetupCamera(_camera, _screenRatio,_cageLayers.Length, _basicSize, _screenScale,
                    _cameraDistance);

        _cameraZoom = _camera.orthographicSize;
        _curLayer = _cubesInLayers.Keys.Count - 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _curLayer >= 0)
        {
            StartCoroutine(ClearLastLayer());
        }

        if (_cubesInLayers[_curLayer].Count < 1)
        {
            _cubesInLayers.Remove(_curLayer);
            _curLayer--;
            
            _cameraZoom = ZoomCamera(_screenRatio, _cubesInLayers.Keys.Count, _basicSize,
                _screenScale);
        }
        else if(Math.Abs(_cameraZoom - _camera.orthographicSize) > 0.1f)
        {
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _cameraZoom, Time.deltaTime * 2);
        }
        
    }

    private IEnumerator ClearLastLayer()
    {
        foreach (var part in _cubesInLayers[_curLayer])
        {
            part.DestroyBody();
            yield return new WaitForEndOfFrame();
        }
        
        _cubesInLayers[_curLayer].Clear();
    }

    private void CreatLayers(IReadOnlyList<CageLayer> layers, int basicScale)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            int rowCount = basicScale + (ProgressionFactor * i);

            Vector3Int depth = Vector3Int.one * i;

            Vector3Int[] cubesPosList = CalculateLayer(rowCount, depth);

            _cubesInLayers.Add(i, InstantiateLayer(i, layers[i], cubesPosList));
        }
    }
    
    private List<ICube> InstantiateLayer(int layerIndex, CageLayer layerProperty, IReadOnlyList<Vector3Int> cubePos)
    {
        List<ICube> result = new List<ICube>();
        
        for (int i = 0; i < cubePos.Count; i++)
        {
            ICube cube = Instantiate(_cubePrefab, cubePos[i], Quaternion.identity).GetComponent<ICube>();

            cube.Init($"Unit_{layerIndex}_{i}", layerProperty.CubeHp, layerProperty.CubeMaterial);
            
            result.Add(cube);
        }

        return result;
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

    private void SetupCamera(Camera cameraProperty, Vector2 screenRatio, int layerIndex, int basicScale,
                            float cageDistance, float cameraDistance)
    {
        Vector3 forwardEdge = Vector3.one * (layerIndex - 1);
        Vector3 cameraPos = forwardEdge + (Vector3.one * cameraDistance);

        cameraProperty.transform.position = cameraPos;
        cameraProperty.transform.LookAt(forwardEdge);

        cameraProperty.orthographicSize = ZoomCamera(screenRatio, layerIndex, basicScale, cageDistance);
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
}

[System.Serializable]
public struct CageLayer
{
    public Material CubeMaterial;
    public int CubeHp;
}
