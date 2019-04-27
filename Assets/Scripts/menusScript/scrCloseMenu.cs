using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scrCloseMenu : MonoBehaviour
{
    public GameObject closePnl;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => { onClickCloseBtn(); });
    }

    void onClickCloseBtn()
    {
        closePnl.SetActive(false);
    }
}
