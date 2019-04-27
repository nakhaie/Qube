using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private Material[] _material;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreatBox());
    }

    IEnumerator CreatBox()
    {
        List<Vector3Int> totalLayerPos = new List<Vector3Int>();

        int rowCount = _material.Length;
        
        for (int i = 0; i < rowCount; i++)
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
            
            Debug.Log(layerPos.Count);

            yield return new WaitForEndOfFrame();
            
            foreach (var cubePos in layerPos)
            {
                ICube cube = Instantiate(_cubePrefab).GetComponent<ICube>();
                
                cube.Init(i.ToString(),cubePos + (Vector3.right * (rowCount * 2) * i),1,_material[i]);
            }
        }
    }
}
