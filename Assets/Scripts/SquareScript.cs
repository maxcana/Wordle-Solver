using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SquareScript : MonoBehaviour
{
    TextMeshPro textMesh;
    WordleData data;
    SpriteRenderer sr;
    [SerializeField] bool isNormalWordle;
    string text;
    [SerializeField] public int myrow;
    [SerializeField] public int mycolumn;
    [SerializeField] public Material green;
    [SerializeField] public Material yellow;
    [SerializeField] public Material gray;
    [SerializeField] public Material white;
    private void Awake()
    {
        data = GameObject.FindGameObjectWithTag("Wordle Data").GetComponent<WordleData>();
        sr = GetComponent<SpriteRenderer>();
        textMesh = transform.GetChild(0).GetComponent<TextMeshPro>();
    }
    private void Start()
    {
        textMesh.fontSize = 8;
        sr.material = white;
    }
    private void Update()
    {
        if (myrow < WordleData.currentGuessNumber || WordleData.mode == WordleData.Mode.eldroW)
        {
            sr.material = toMaterial(WordleData.colours[myrow][mycolumn]);
        }
        string oldText = textMesh.text;
        textMesh.text = WordleData.guesses[myrow][mycolumn].ToString().ToUpper();
        textMesh.fontSize += (8 - textMesh.fontSize) * Time.deltaTime * 5;
        textMesh.color += new Color(0, 0, 0, (1 - textMesh.color.a) * Time.deltaTime * 5);
        if (oldText != textMesh.text)
        {
            textMesh.fontSize = 10;
            textMesh.alpha = 0;
        }
    }
    private Material toMaterial(WordleSolver.Colour colour)
    {
        switch (colour)
        {
            case WordleSolver.Colour.Gray:
                return gray;

            case WordleSolver.Colour.Yellow:
                return yellow;

            case WordleSolver.Colour.Green:
                return green;

            default:
                return white;
        }

    }
    private void OnMouseDown()
    {
        if(WordleData.mode == WordleData.Mode.eldroW && myrow == WordleData.currentGuessNumber){
             WordleData.colours[myrow][mycolumn] = nextColour(WordleData.colours[myrow][mycolumn]);
        }
    }

    private WordleSolver.Colour nextColour(WordleSolver.Colour c)
    {
        switch (c)
        {
            case WordleSolver.Colour.Gray:
                return WordleSolver.Colour.Yellow;

            case WordleSolver.Colour.Yellow:
                return WordleSolver.Colour.Green;

            default:
                return WordleSolver.Colour.Gray;
        }
    }
}
