using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class settingMenu : MonoBehaviour
{
    public Button sfxBtn,musicBtn;
    void Start()
    {
        sfxBtn.onClick.AddListener(() => { onClickSfxBtn(); });
        musicBtn.onClick.AddListener(() => { onClickMusicBtn(); });
    }
    void onClickSfxBtn()
    {

    }
    void onClickMusicBtn()
    {

    }
   
    void Update()
    {
        
    }
}
