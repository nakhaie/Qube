using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeController : MonoBehaviour
{
    public float initTimeDaily;
    void Start()
    {
        if (PlayerPrefs.GetInt("initVar") == 0)
        {
            PlayerPrefs.SetInt("initVar", 1);
            PlayerPrefs.SetFloat("dailyTime", initTimeDaily);
            PlayerPrefs.SetString("qTime", System.DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss/"));
            if (PlayerPrefs.GetString("qTime") == "")
                PlayerPrefs.SetString("qTime", "1/1/1/1/1/1");
        }
    }
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log(System.DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss/"));
            PlayerPrefs.SetString("qTime", System.DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss/"));

        }
        else
        {
            string[] mytime = PlayerPrefs.GetString("qTime").Split("/"[0]);
            System.DateTime startPause = new System.DateTime(int.Parse(mytime[0]), int.Parse(mytime[1]), int.Parse(mytime[2]), int.Parse(mytime[3]), int.Parse(mytime[4]), int.Parse(mytime[5]));
            System.TimeSpan totalTime = System.DateTime.Now - startPause;
            try
            {
                Debug.Log(totalTime.TotalSeconds);
                PlayerPrefs.SetFloat("dailyTime", PlayerPrefs.GetFloat("dailyTime") - (float)totalTime.TotalSeconds);
            }
            catch (System.Exception ex)
            {
                string ms = ex.Message;
                PlayerPrefs.SetFloat("dailyTime", 0);
            }
        }
    }
    void Update()
    {

        PlayerPrefs.SetFloat("dailyTime", PlayerPrefs.GetFloat("dailyTime") - Time.deltaTime);
        if (PlayerPrefs.GetFloat("dailyTime") <= 0)
        {
            setDailyTime();
        }

    }

    void setDailyTime()
    {
        PlayerPrefs.SetFloat("dailyTime", initTimeDaily);
    }
    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("qTime", System.DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss/"));
    }
    void OnGUI()
    {
        GUI.color = Color.red;
        GUIStyle myStyle = new GUIStyle();
        myStyle.normal.textColor = Color.red;
        myStyle.fontSize = 40;
        GUI.Label(new Rect(10, 10, 400, 100), PlayerPrefs.GetFloat("dailyTime") + " :XP", myStyle);
    }

}
