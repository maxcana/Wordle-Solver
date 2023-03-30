using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.Threading;
using BitStuff;
using UnityEngine.SceneManagement;
using System.Linq;

public class WordleData : MonoBehaviour
{
    //Global Variables
    public enum Mode { Wordle, eldroW, Hardle }
    public static bool optimizeWordle;
    public static bool calcGuessStats;
    public static bool censorBadWords = false;
    public static bool hasSolver;
    public static Mode mode;
    public static List<(float, int)> estimatedXPrimeToActual = new List<(float, int)>();
    public static volatile bool stopTask = false;
    public static volatile bool taskRunning = false;
    KeyCode[] allowedLetters = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M };
    public static int currentGuessNumber = 0;
    public static int wordLength = 5;
    public static string currentGuessWord = new string(' ', wordLength);
    public static int currentLetterToEdit = 0;
    public static int currentColourToEdit = 0;
    public static List<string> guesses;
    public static List<List<WordleSolver.Colour>> colours;
    public IDictionary<char, WordleSolver.CharKnowledge> knowledge;
    public static string secret;
    private void Awake()
    {
        currentColourToEdit = 0;
        guesses = new List<string> { };
        colours = new List<List<WordleSolver.Colour>> { };

        if (mode != Mode.eldroW)
            secret = WordleSolver.dictionary[UnityEngine.Random.Range(0, WordleSolver.dictionary.Count)];

        InitailizeGuessesAndColours();
    }
    private void Start()
    {
    }
    public static void InitailizeGuessesAndColours()
    {
        var missingColours = GetMissingColours();

        for (int i = 0; i < 6; i++)
        {
            string mynewstring = new string(' ', wordLength);
            guesses.Add(mynewstring);
            if (mode == Mode.eldroW)
            {
                colours.Add(new List<WordleSolver.Colour>(missingColours));
            }
        }
    }
    public static List<WordleSolver.Colour> GetMissingColours()
    {
        var missingColours = mode == Mode.eldroW ? new List<WordleSolver.Colour>(wordLength) : null;

        if (mode == Mode.eldroW)
            for (int i = 0; i < wordLength; i++)
            {
                missingColours.Add(WordleSolver.Colour.Missing);
            }
        return missingColours;
    }
    private void Update()
    {

        //! ANY LETTER IS PRESSED
        if (currentLetterToEdit < wordLength)
        {
            foreach (KeyCode allowedLetter in allowedLetters)
            {
                if (Input.GetKeyDown(allowedLetter))
                {
                    var charArray = currentGuessWord.ToCharArray();
                    charArray[currentLetterToEdit] = allowedLetter.ToString().ToCharArray()[0];
                    currentGuessWord = string.Concat(charArray).ToLower();
                    currentLetterToEdit++;
                }
            }
        }
        guesses[currentGuessNumber] = currentGuessWord;

        //! NUMBER IS PRESSED
        if (mode == Mode.eldroW)
        {
            if (currentColourToEdit < wordLength)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    print("one pressed");
                    colours[currentGuessNumber][currentColourToEdit] = WordleSolver.Colour.Gray;
                    currentColourToEdit++;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    colours[currentGuessNumber][currentColourToEdit] = WordleSolver.Colour.Yellow;
                    currentColourToEdit++;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    colours[currentGuessNumber][currentColourToEdit] = WordleSolver.Colour.Green;
                    currentColourToEdit++;
                }
            }
            //! DELETE IS PRESSED
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (currentColourToEdit > 0)
                {
                    currentColourToEdit--;
                }
                colours[currentGuessNumber][currentColourToEdit] = WordleSolver.Colour.Missing;
            }
        }

        //! BACKSPACE IS PRESSED
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (currentLetterToEdit >= 0 && currentLetterToEdit > 0)
            {
                var charArray = currentGuessWord.ToCharArray();
                charArray[currentLetterToEdit - 1] = ' ';
                currentGuessWord = string.Concat(charArray);
                currentLetterToEdit--;
            }
        }
        if (Input.GetKeyDown(KeyCode.End))
        {
            //restart
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            if (currentGuessNumber == 0 && wordLength == 5)
            {
                currentGuessWord = "lares";
                currentLetterToEdit = wordLength;
            }
            else
            {
                if (WordleSolver.wordRankings != null && WordleSolver.wordRankings.Count > 0)
                {
                    currentGuessWord = WordleSolver.wordRankings.Keys.First().Item2;
                    currentLetterToEdit = currentGuessWord.Length;
                }
            }
        }

        //! ENTER IS PRESSED
        if (Input.GetKeyDown(KeyCode.Return))
        {
            print(currentGuessWord);
            if (!currentGuessWord.Contains(" ") && !(mode == Mode.eldroW && colours[currentGuessNumber].Contains(WordleSolver.Colour.Missing)) && WordleSolver.fastDictionary.Contains(currentGuessWord))
            {
                if (mode == Mode.Wordle)
                {
                    colours.Add(WordleSolver.CharKnowledge.getColoursForWord(currentGuessWord, secret));
                }
                if (mode == Mode.Hardle)
                {
                    int topXprime = -1;
                    List<WordleSolver.Colour> bestColours = new List<WordleSolver.Colour>();
                    var coloursToRemainingWords = new Dictionary<ulong, int>();
                    try
                    {
                        foreach (string secret in WordleSolver.possibleSecrets)
                        {
                            var localColours = WordleSolver.CharKnowledge.getColoursForWord(currentGuessWord, secret);
                            var numColours = WordleSolver.ColoursToNum(localColours);
                            if (!coloursToRemainingWords.ContainsKey(numColours))
                            {
                                //knowledge before info is just called "knowledge".
                                var knowledgeAfterInfo = WordleSolver.CharKnowledge.fromGuess(currentGuessWord, localColours);
                                knowledgeAfterInfo = WordleSolver.CharKnowledge.combineKnowledge(knowledgeAfterInfo, knowledge);
                                knowledgeAfterInfo = WordleSolver.CharKnowledge.inferFully(wordLength, knowledgeAfterInfo);
                                var localPerWord = Bits.LongsPerWord(WordleSolver.alphabet);

                                ulong[] localMask = Bits.BitMask(knowledgeAfterInfo, WordleSolver.alphabet);
                                ulong[] localMap = new ulong[WordleSolver.possibleSecrets.Count * localPerWord];

                                int i = 0;
                                foreach (string possibleSecret in WordleSolver.possibleSecrets)
                                    Bits.BitMapTo(possibleSecret, WordleSolver.alphabet, localMap, i++ * localPerWord);

                                int xprime = 0;
                                for (int j = 0; j < WordleSolver.possibleSecrets.Count; j++)
                                {
                                    if (Bits.AndIs0(localMask, localMap, localPerWord * j))
                                    {
                                        xprime++;
                                    }
                                }
                                coloursToRemainingWords.Add(numColours, xprime);
                                if (xprime > topXprime)
                                {
                                    topXprime = xprime;
                                    bestColours = localColours;
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                    colours.Add(bestColours);
                }
                var currentKnowledge = WordleSolver.CharKnowledge.fromGuess(currentGuessWord, colours[currentGuessNumber]);
                var oldKnowledge = knowledge;
                knowledge = WordleSolver.CharKnowledge.combineKnowledge(knowledge, currentKnowledge);
                knowledge = WordleSolver.CharKnowledge.inferFully(wordLength, knowledge);
                StringBuilder sb = new StringBuilder();
                foreach (var kv in knowledge)
                    sb.Append(kv.Key).Append(": ").Append(kv.Value).Append(", \n");
                print("{" + sb + "}");

                int oldPossiblesecretsCount = WordleSolver.possibleSecrets.Count;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! THIS IS NOT DEEP COPY (Right?)
                List<string> oldPossibleSecrets = WordleSolver.possibleSecrets;
                var perWord = Bits.LongsPerWord(WordleSolver.alphabet);

                ulong[] filterMask = Bits.BitMask(knowledge, WordleSolver.alphabet);
                ulong[] map = new ulong[WordleSolver.possibleSecrets.Count * perWord];

                int l = 0;
                foreach (string possibleSecret in WordleSolver.possibleSecrets)
                    Bits.BitMapTo(possibleSecret, WordleSolver.alphabet, map, l++ * perWord);

                //! Calculates possible secrets here
                WordleSolver.possibleSecrets = WordleSolver.BitFilter(filterMask, map, WordleSolver.possibleSecrets);
                WordleSolver.censorPossibleSecrets();

                if (hasSolver)
                {

                    if (calcGuessStats)
                    {
                        float currentGuessWordRanking = float.NaN;
                        SortedList<(float, string), bool> copy = new SortedList<(float, string), bool>();
                        if (WordleSolver.wordRankings != null)
                            lock (WordleSolver.wordRankings)
                            {
                                copy = new SortedList<(float, string), bool>(WordleSolver.wordRankings);
                            };
                        foreach (var (ranking, word) in copy.Keys)
                        {
                            if (word == currentGuessWord)
                            {
                                currentGuessWordRanking = ranking;
                                break;
                            }
                        }
                        if (!float.IsNaN(currentGuessWordRanking))
                            estimatedXPrimeToActual.Add((currentGuessWordRanking, WordleSolver.possibleSecrets.Count));
                        else
                        {
                            float totalxprime = 0;

                            ulong[] bits = new ulong[oldPossibleSecrets.Count * perWord];

                            int i = 0;
                            foreach (string possibleSecret in oldPossibleSecrets)
                                Bits.BitMapTo(possibleSecret, WordleSolver.alphabet, bits, i++ * perWord);

                            foreach (var possiblesecret in oldPossibleSecrets)
                            {
                                var colours = WordleSolver.CharKnowledge.getColoursForWord(currentGuessWord, possiblesecret);
                                var newKnowledge = WordleSolver.CharKnowledge.fromGuess(currentGuessWord, colours);
                                newKnowledge = WordleSolver.CharKnowledge.combineKnowledge(newKnowledge, oldKnowledge);
                                newKnowledge = WordleSolver.CharKnowledge.inferFully(WordleData.wordLength, newKnowledge);

                                ulong[] mask = Bits.BitMask(newKnowledge, WordleSolver.alphabet);
                                int xprime = 0;
                                for (int j = 0; j < oldPossibleSecrets.Count; j++)
                                {

                                    if (Bits.AndIs0(mask, bits, perWord * j))
                                    {
                                        xprime++;
                                    }
                                }

                                totalxprime += xprime;
                            }
                            estimatedXPrimeToActual.Add((Mathf.RoundToInt(totalxprime / oldPossiblesecretsCount), WordleSolver.possibleSecrets.Count));
                        }
                    }
                    CalculateWordRankings();
                }

                currentGuessNumber++;

                //R.I.P. string bobTheBuilder: 2022-2022
                currentGuessWord = new string(' ', wordLength);
                currentLetterToEdit = 0;
                currentColourToEdit = 0;
            }
        }
        currentLetterToEdit = Mathf.Clamp(currentLetterToEdit, 0, 9999);
    }
    void CalculateWordRankings()
    {
        print("started calculating word rankings");
        stopTask = true;
        while (taskRunning) Thread.Sleep(2);
        stopTask = false;
        GameObject.FindWithTag("Instantiater").GetComponent<AddTexts>().Clear();
        WordleSolver.rankings = new System.Collections.Concurrent.ConcurrentQueue<(int, string, float)>();
        Task<bool>.Run(() =>
        {
            return WordleSolver.calcWordRankings(knowledge);
        }
        );
    }
    public string PrintMap(IDictionary<string, float> map)
    {
        StringBuilder result = new StringBuilder();
        foreach (KeyValuePair<string, float> kv in map)
        {
            result.Append("{\"");
            result.Append(kv.Key);
            result.Append("\", ");
            result.Append(kv.Value);
            result.Append("f},\n");
        }
        return result.ToString();
    }
}
