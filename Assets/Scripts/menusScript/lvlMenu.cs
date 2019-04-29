using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lvlMenu : MonoBehaviour
{
    public int lvl, chapterMax;
    private int chapter;
    public int noLvlInPage;
    public Button[] lvlBtns;
    public Text[] lvlTexts;
    public Text chapterText;
    public Button nextBtn,prevBtn;


    void OnEnable()
    {
        chapter = (lvl / noLvlInPage) + 1;
        setLvlsBaseOnChapter();
    }

    void Start()
    {
        nextBtn.onClick.AddListener(() => { onClickPageChange(0); });
        prevBtn.onClick.AddListener(() => { onClickPageChange(1); });

        for (int i = 0; i < lvlBtns.Length; i++)
            clickLvlBtns(lvlBtns[i], i);
    }
    void clickLvlBtns(Button btn, int indexMode)
    {
        btn.onClick.AddListener(() => { onClickLvlBtns(indexMode); });
    }
    void onClickPageChange(int index)
    {
        if(index==0)
        {
            if (chapter < chapterMax)
            {
                chapter++;
                setLvlsBaseOnChapter();
            }
        }
        else
        {
            if (chapter >1)
            {
                chapter--;
                setLvlsBaseOnChapter();
            }

        }
    }
    void onClickLvlBtns(int indexMode)
    {
        int clickLvl = findLvlFromIndexLvlBtn(indexMode);
        Debug.Log(clickLvl);
    }


    void setLvlsBaseOnChapter()
    {
        for (int i = 0; i < noLvlInPage; i++)
            lvlTexts[i].text = ((chapter - 1) * noLvlInPage + (i + 1)).ToString();

        chapterText.text = chapter + "/" + chapterMax;
    }
   
    int findLvlFromIndexLvlBtn(int index)
    {
        return ((chapter - 1) * noLvlInPage) + (index + 1);
    }
    //void Update()
    //{

    //}
}
