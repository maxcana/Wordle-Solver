using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System.Linq;

public class statistic : MonoBehaviour
{
    [SerializeField] public StatisticMode statToGet;
    public enum StatisticMode { Accuracy, Bar, Iterations, WordsTried, TopXWords, SecretsLeft, PossibleWords, CurrentlyTriedWord, None }
    TextMeshProUGUI textMesh;
    Image img;
    AddTexts addTexts;
    float moveTowards = WordleSolver.possibleSecrets.Count;
    private void Start()
    {
        TryGetComponent<TextMeshProUGUI>(out textMesh);
        TryGetComponent<Image>(out img);
        addTexts = GameObject.FindGameObjectWithTag("Instantiater").GetComponent<AddTexts>();
    }
    void Update()
    {
        switch (statToGet)
        {
            case StatisticMode.Accuracy:
                textMesh.SetText(string.Format("Accuracy: <mspace=mspace=36>{0:n7}</mspace>", ((double)WordleSolver.wordComparisons / (double)WordleSolver.totalWordComparisons) * 100) + "%");
                break;
            case StatisticMode.Bar:
                img.fillAmount = (float)WordleSolver.wordComparisons / (float)WordleSolver.totalWordComparisons;
                break;

            case StatisticMode.Iterations:
                textMesh.text = WordleSolver.wordComparisons + "/" + WordleSolver.totalWordComparisons + " comparisons";
                break;

            case StatisticMode.WordsTried:
                textMesh.text = WordleSolver.currentWordsTried + "/" + WordleSolver.dictionary.Count + " words tried";
                break;

            case StatisticMode.TopXWords:
                if (addTexts.showOnlyTop != int.MaxValue)
                {
                    textMesh.text = "Showing top " + addTexts.showOnlyTop.ToString() + " words";
                }
                else
                {
                    textMesh.text = "Showing all words";
                }
                break;

            case StatisticMode.SecretsLeft:
                moveTowards += (WordleSolver.possibleSecrets.Count - moveTowards) * Time.deltaTime * 30;
                textMesh.text = Mathf.Round(moveTowards).ToString();
                break;

            case StatisticMode.PossibleWords:
                textMesh.text = WordleSolver.possibleSecrets.Count > 400 ? string.Join(", ", WordleData.censorBadWords ? WordleSolver.censoredPossibleSecrets.Take(400)  : WordleSolver.possibleSecrets.Take(400)) + " + " + (WordleSolver.possibleSecrets.Count - 400).ToString() + " more" : string.Join(", ", WordleSolver.possibleSecrets);
                break;
            case StatisticMode.CurrentlyTriedWord:
                if(WordleSolver.currentlyTriedWord != ""){
                    if(WordleData.censorBadWords && WordleSolver.badWords.Contains(WordleSolver.currentlyTriedWord)){
                        textMesh.text = "Currently trying: " + string.Format("{0:n2}", WordleSolver.currentlyTriedWord) + "***";
                    }
                    textMesh.text = "Currently trying: " + WordleSolver.currentlyTriedWord;
                }
                else textMesh.text = "";
                break;

            case StatisticMode.None:
                break;

            default:
                throw new System.Exception("holy shit");

        }
    }
}
