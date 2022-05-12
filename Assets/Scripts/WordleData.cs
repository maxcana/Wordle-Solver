using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.Threading;
using BitStuff;

public class WordleData : MonoBehaviour
{
    //Global Variables
    public enum Mode { Wordle, eldroW, Hardle }
    public static bool censorBadWords = false;
    public static bool hasSolver;
    public static Mode mode;
    public static List<(int, int)> estimatedXPrimeToActual = new List<(int, int)>();
    public static volatile bool stopTask = false;
    public static volatile bool taskRunning = false;
    KeyCode[] allowedLetters = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M };
    public static int currentGuessNumber = 0;
    public static int wordLength = 5;
    public static string currentGuessWord = new string(' ', wordLength);
    public static int currentLetterToEdit = 0;
    public static List<string> guesses = new List<string> { };
    public static List<List<WordleSolver.Colour>> colours = new List<List<WordleSolver.Colour>> { };
    public IDictionary<char, WordleSolver.CharKnowledge> knowledge;
    public static string secret;
    private void Start()
    {
        var missingColours = mode == Mode.eldroW ? new List<WordleSolver.Colour>(wordLength) : null;

        if (mode == Mode.eldroW)
            for (int i = 0; i < wordLength; i++)
            {
                missingColours.Add(WordleSolver.Colour.Missing);
            }
        else{
            secret = censorBadWords ? WordleSolver.dictionaryWithoutBadWords[UnityEngine.Random.Range(0, WordleSolver.dictionaryWithoutBadWords.Count)] : WordleSolver.dictionary[UnityEngine.Random.Range(0, WordleSolver.dictionary.Count)];
        }

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

        //! ENTER IS PRESSED
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            print(PrintMap(WordleSolver.wordRankings));
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            print(currentGuessWord);
            if (!currentGuessWord.Contains(" ") && !(mode == Mode.eldroW && colours[currentGuessNumber].Contains(WordleSolver.Colour.Missing)) && WordleSolver.fastDictionary.Contains(currentGuessWord))
            {
                if (mode != Mode.eldroW)
                {
                    colours.Add(WordleSolver.CharKnowledge.getColoursForWord(currentGuessWord, secret));
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

                if (currentGuessWord == "above")
                    UnityEngine.Debug.Log("actual secret: " + secret + ", filterMask for 'above':\n" + Bits.BitsToString(filterMask, WordleSolver.alphabet));

                if (hasSolver)
                {
                    if (WordleSolver.wordRankings.ContainsKey(currentGuessWord))
                    {
                        estimatedXPrimeToActual.Add((Mathf.RoundToInt(WordleSolver.wordRankings[currentGuessWord]), WordleSolver.possibleSecrets.Count));
                    }
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
                                // else if (re.IsMatch(possibleSecrets[j]) && j != 0)
                                // {
                                //     UnityEngine.Debug.LogWarning("Regex matched but bits didn't on word " + possibleSecrets[j] + " with secret " + possiblesecret + " and with guess " + word + " and mask\n" + Bits.BitsToString(mask, alphabet) + "and with word representation\n" + Bits.BitsToString(bits, alphabet, perWord * j, '·'));
                                // }
                            }

                            totalxprime += xprime;
                        }
                        estimatedXPrimeToActual.Add((Mathf.RoundToInt(totalxprime / oldPossiblesecretsCount), WordleSolver.possibleSecrets.Count));
                    }

                    WordleSolver.wordRankings.Clear();
                    CalculateWordRankings();
                }

                currentGuessNumber++;

                //R.I.P. string bobTheBuilder: 2022-2022
                currentGuessWord = new string(' ', wordLength);
                currentLetterToEdit = 0;
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
