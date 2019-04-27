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
        List<Vector3> posList = new List<Vector3>();

        for (int i = 0; i < 4; i++)
        {
            int RowCount = i;
        
            for (int x = -RowCount; x <= RowCount; x++)
            {
                for (int y = -RowCount; y <= RowCount; y++)
                {
                    for (int z = -RowCount; z <= RowCount; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);

                        if (!posList.Contains(pos))
                        {
                            ICube cube = Instantiate(_cubePrefab).GetComponent<ICube>();
                    
                            cube.Init(i.ToString(),pos,1,_material[0]);
                    
                            posList.Add(pos);
                        }
                        
                        
                    }
                }
            }
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
