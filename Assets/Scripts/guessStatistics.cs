using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class guessStatistics : MonoBehaviour
{
    [SerializeField] int guessNumber;
    TextMeshProUGUI estimatedText;
    TextMeshProUGUI actualText;
    Image img0;
    Image img2;
    Image img3;
    float estLerp = 0;
    float actLerp = 0;
    private void Start() {
        estimatedText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        actualText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        estimatedText.color = new Color(1, 1, 1, 0);
        actualText.color = new Color(1, 1, 1, 0);
        img0 = GetComponent<Image>();
        img0.color = new Color(img0.color.r, img0.color.g ,img0.color.b, 0);
        img2 = transform.GetChild(2).GetComponent<Image>();
        img2.color = new Color(img2.color.r, img2.color.g ,img2.color.b, 0);
        img3 = transform.GetChild(3).GetComponent<Image>();
        img3.color = new Color(img3.color.r, img3.color.g ,img3.color.b, 0);

    }
    private void Update() {
        if(WordleData.currentGuessNumber > guessNumber){
                actualText.color += new Color(0, 0, 0, (1 - actualText.color.a) * Time.deltaTime * 5);
                estimatedText.color += new Color(0, 0, 0, (1 - estimatedText.color.a) * Time.deltaTime * 5);
                img0.color += new Color(0, 0, 0, (0.4f - img0.color.a) * Time.deltaTime * 5);
                img2.color += new Color(0, 0, 0, (1f - img2.color.a) * Time.deltaTime * 5);
                img3.color += new Color(0, 0, 0, (1f - img3.color.a) * Time.deltaTime * 5);

        }
        if(WordleData.estimatedXPrimeToActual.Count >= guessNumber + 1){
        estLerp += (WordleData.estimatedXPrimeToActual[guessNumber].Item1 - estLerp) * Time.deltaTime * 3;
        actLerp += (WordleData.estimatedXPrimeToActual[guessNumber].Item2 - actLerp) * Time.deltaTime * 3;
        estimatedText.text = (Mathf.Round(estLerp * 10) / 10).ToString();
        actualText.text = Mathf.Round(actLerp).ToString();}
    }

}
