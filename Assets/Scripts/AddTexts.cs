using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AddTexts : MonoBehaviour
{
    // "/Assets/resources/prefabs/prefab1.prefab"
    [SerializeField] public int showOnlyTop = 100;
    [SerializeField] public GameObject text;
    [SerializeField] public RectTransform rt;
    public void AddText(int index, string word, float rank)
    {
        if(index < showOnlyTop){
            GameObject textObject = Instantiate(text, Vector3.zero, Quaternion.identity, rt);
            var textMesh = textObject.GetComponent<TextMeshProUGUI>();
            textObject.GetComponent<RectTransform>().SetSiblingIndex(index);
            textObject.name = word;
            textMesh.text = word + "\t" + rank;
            for (int i = showOnlyTop; i < rt.childCount; i++)
                Destroy(rt.GetChild(i).gameObject);
        }

    }
    public void Clear()
    {
        foreach (var child in rt.GetComponentsInChildren<Transform>())
        {
            if (child != rt)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void Start()
    {

    }
    void Update()
    {
        WordleSolver.assert(rt != null, "rt is null");
        (int, string, float) item;

        while (WordleSolver.rankings.TryDequeue(out item))
            AddText(item.Item1, item.Item2, item.Item3);
    }
}
