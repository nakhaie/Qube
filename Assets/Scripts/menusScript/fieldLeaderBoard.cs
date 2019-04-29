using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class fieldLeaderBoard : MonoBehaviour {

    public string number, namePlayer, score;
    public Text numText, namePlayerText, scoreText;
   public void setVariable()
    {
        numText.text = number;
        namePlayerText.text = namePlayer;
        scoreText.text = score;
    }
}
