using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : Observer
{
    public GameObject fadeSquare;
    public TextMeshProUGUI goldText;
    private PlayerController player;
    public bool fade = true;
    public int fadeSpeed = 5;

    //instance
    public static GameUI instance;
    
    void Awake()
    {
        instance = this;
    }

    public void UpdateGoldText (int gold)
    {
        goldText.text = "<b>Gold:</b> " + gold;
    }

    public IEnumerator FadeToBlack(bool fader, int speed)
    {
        Color objectColor = fadeSquare.GetComponent<Image>().color;
        float fadeAmount;

        

        if (fader)
        {
            Debug.Log("Yee");

            while (fadeSquare.GetComponent<Image>().color.a < 1)
            {
                fadeAmount = objectColor.a + (speed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                fadeSquare.GetComponent<Image>().color = objectColor;
                
               
                yield return null;
            }
        }
        else
        {
            Debug.Log("Nah");

            while (fadeSquare.GetComponent<Image>().color.a > 0)
            {
                fadeAmount = objectColor.a - (speed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                fadeSquare.GetComponent<Image>().color = objectColor;

               
                yield return null;
            }
        }
    }

    public override void Notify(Subject subject)
    {
        if(!player)
        {
            player = subject.GetComponent<PlayerController>();
        }

        if(player && player.stairs == true)
        {
            Debug.Log("Heard 1");

            Debug.Log("Start Fade");
            StartCoroutine(FadeToBlack(fade, fadeSpeed));
            fade = false;

            Debug.Log("Reverse Fade");
            StartCoroutine(FadeToBlack(fade, fadeSpeed));
            fade = true;
        }
    }
}
