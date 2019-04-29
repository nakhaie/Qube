﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class shop : MonoBehaviour
{
    public Text coinText, gemText,timeText;
    public Button[] tabBtn;
    public GameObject[] pnls;
    public ScrollRect scrll;
    void OnEnable()
    {
        UIManager.Instance.setCoinAndGem(coinText, gemText);
        onClickPanles(0, 1);
    }
    void Start()
    {
        tabBtn[0].onClick.AddListener(() => { onClickPanles(0,1); });
        tabBtn[1].onClick.AddListener(() => { onClickPanles(1,0); });
    }
    void onClickPanles(int indexOpen,int indexClose)
    {
        scrll.content = pnls[indexOpen].GetComponent<RectTransform>();
        pnls[indexOpen].SetActive(true);
        pnls[indexClose].SetActive(false);
    }
    void Update()
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(PlayerPrefs.GetFloat("dailyTime"));
        timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }
}