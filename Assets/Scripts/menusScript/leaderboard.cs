using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class leaderboard : MonoBehaviour
{
    public Text scoreText;
    public Transform contentscroll;
    public GameObject[] topFields;
    public GameObject fieldPrefab;
    private float sizeOneField;
    private Player playerInstance;
    private List<GameObject> goList = new List<GameObject>();
    [System.Serializable]
    public class Player
    {

        public string playerNum;
        public string playerName;
        public string playerScore;
    }
    void Start()
    {
        sizeOneField = fieldPrefab.GetComponent<RectTransform>().sizeDelta.y;
        string jsonString = "{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}*{\"playerNum\":\"8484239823\",\"playerName\":\"Powai\",\"playerScore\":\"10000\"}";
        string[] players = jsonString.Split("*"[0]);
        createAndFillField(players);
    }
    void OnEnable()
    {
        scoreText.text = "100";
    }
    //void Update()
    //{

    //}

    //void deserialize(string jsonString)
    //{
    //    JsonUtility.FromJsonOverwrite(jsonString, playerInstance);
    //}
    void createAndFillField(string[] players)
    {
        contentscroll.GetComponent<RectTransform>().sizeDelta = new Vector2(contentscroll.GetComponent<RectTransform>().sizeDelta.x, contentscroll.GetComponent<RectTransform>().sizeDelta.y + (sizeOneField * (players.Length - 3) + 50));
        for (int i = 3; i < goList.Count; i++)
            simplePool.Despawn(goList[i]);
        goList.Clear();
        float posOne = (contentscroll.GetComponent<RectTransform>().sizeDelta.y / 2) - (sizeOneField / 2);

        for (int i = 0; i < players.Length; i++)
        {
            playerInstance = new Player();
            JsonUtility.FromJsonOverwrite(players[i], playerInstance);
            if (i < 3)
                setDataUIField(topFields[i]);
            else
            {
                GameObject fieldGO= simplePool.Spawn(fieldPrefab, fieldPrefab.GetComponent<RectTransform>().position, Quaternion.identity);
                fieldGO.SetActive(true);
                goList.Add(fieldGO);
                fieldGO.GetComponent<RectTransform>().SetParent(contentscroll, false);
                Vector3 pos = fieldGO.GetComponent<RectTransform>().localPosition;
                pos.y += posOne;
                posOne -= sizeOneField;
                fieldGO.GetComponent<RectTransform>().localPosition = pos;
                setDataUIField(fieldGO);
            }
          
        }


    }

    void setDataUIField(GameObject fieldGO)
    {
       // fieldLeaderBoard scriptField = fieldGO.AddComponent<fieldLeaderBoard>();
        fieldLeaderBoard scriptField = fieldGO.GetComponent<fieldLeaderBoard>();
        scriptField.number = playerInstance.playerNum;
        scriptField.namePlayer = playerInstance.playerName;
        scriptField.score = playerInstance.playerScore;
        scriptField.setVariable();
    }

}

