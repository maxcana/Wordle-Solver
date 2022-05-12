using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public WordleData.Mode mode;
    bool hasSolver;
    Toggle solverToggle;
    Toggle badWordsToggle;
    public string sceneToLoad;
    Button button;
    private void Awake()
    {
        badWordsToggle = GameObject.FindWithTag("BadWordsToggle").GetComponent<Toggle>();
        solverToggle = GameObject.FindWithTag("SolverToggle").GetComponent<Toggle>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        hasSolver = solverToggle.isOn || mode == WordleData.Mode.eldroW;
        if (mode == WordleData.Mode.eldroW)
            solverToggle.isOn = true;

        WordleData.mode = mode;

        transform.parent.DetachChildren();
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(sceneToLoad);
        StartCoroutine(ThisUsedToBeLiterallyTheStupidestThingIHadToDoEverOnUnityButThankfullyICanYeildReturnNullIAmDoingThisBecauseSceneManagerLoadSceneLoadsItOnTheNextFrame());
    }
    public IEnumerator ThisUsedToBeLiterallyTheStupidestThingIHadToDoEverOnUnityButThankfullyICanYeildReturnNullIAmDoingThisBecauseSceneManagerLoadSceneLoadsItOnTheNextFrame()
    {
        yield return null;
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        GetComponent<Image>().enabled = false;
        if(badWordsToggle.isOn){
            WordleData.censorBadWords = true;
        }
        if (!hasSolver)
        {
            WordleData.hasSolver = false;
            print(SceneManager.GetActiveScene().name);
            GameObject.FindWithTag("SolverStuff").SetActive(false);
            GameObject.FindWithTag("Squares").transform.position = new Vector2(5.5f, -1);
        }
        else WordleData.hasSolver = true;
        Destroy(gameObject);
    }
}
