using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lvlMenu : MonoBehaviour
{
    public int lvl;
    private int chapter,chapterMax;
    public int noLvlInPage;
    public Text[] lvlTexts;
    public Text chapterText;

    void OnEnable()
    {
        chapter = (lvl / 20) + 1;
        for (int i = 0; i < noLvlInPage; i++)
            lvlTexts[i].text = ((chapter-1)*20 + (i + 1)).ToString();

        chapterText.text = chapter + "/" + chapterMax;
    }

    void Start()
    {
        
    }

    //void Update()
    //{
        
    //}
}
