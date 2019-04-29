using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static UIManager _instance;
    public static UIManager Instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        _instance = this;
    }
    //void Start()
    //{
        
    //}

    //void Update()
    //{
        
    //}
   public void setCoinAndGem(Text coinText,Text gemText)
    {
        coinText.text = "1100";
        gemText.text = "1100";
    }
}
