using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class errorMenu : MonoBehaviour
{
    public Text textBox;
    public Button btn1, btn2;
    public bool isTowBtns=true;

    public int noErr;

    static errorMenu _instance;
    public static errorMenu Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
         }
    void OnEnable()
    {
        if(isTowBtns)
        {
            btn2.gameObject.SetActive(true);
        }
        else
        {
            btn2.gameObject.SetActive(false);
        }
       // StartCoroutine(waitforEn());
    }
    //IEnumerator waitforEn()
    //{
    //    yield return new WaitForSeconds(Time.deltaTime);
    //    if (noErr == 1)
    //        textBox.text = NBidi.NBidi.LogicalToVisual(" اینترنتت قطعه داداش " + "\n" + " نمی شه وصل شد " + "\n" + " به سرور! ");
    //    if (noErr == 2)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("تایم اوت لطفا چند دقیقه" + "\n" + "دیگر دوباره سعی کنید");
    //    if (noErr == 3)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("اینترنت شما مشکل دارد " + "\n" + " لطفا پراکسی خود " + "\n" + " را خاموش کنید");
    //    if (noErr == 4)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("اکانت شما به هر دلیل " + "\n" + " مشکل پیدا کرده.لطفا  " + "\n" + " با ما تماس بگیرید");
    //    if (noErr == 5)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("دسترسی غیر مجاز");
    //    if (noErr == 6)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("مشکل در ارتباط با سرور" + "\n" + " دوباره سعی کنید");
    //    if (noErr == 7)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("مشکل در دریافت اطلاعات" + "\n" + " از سرور" + " دوباره سعی کنید");
    //    if (noErr == 8)
    //        textBox.text = NBidi.NBidi.LogicalToVisual("مشکل در ارسال اطلاعات" + "\n" + " از سرور" + " دوباره سعی کنید");

    //}
    void Start()
    {
        btn1.onClick.AddListener(() => { onClickbt1(); });
        btn2.onClick.AddListener(() => { onClickbt2(); });
    }

    void onClickbt1()
    {
        gameObject.SetActive(false);
    }
    void onClickbt2()
    {
    }
    //IEnumerator waitforAnimationReset()
    //{
    //    yield return new WaitForSeconds(0.3f);
    //}
}
