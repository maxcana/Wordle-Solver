using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace BitStuff
{
    public static class Bits
    {
        public static bool AndIs0(ulong[] bitMask, ulong[] bitMaps, int offset)
        {
            for (int i = 0; i < bitMask.Length; i++)
            {
                if ((bitMask[i] & bitMaps[offset + i]) != 0)
                    return false;
            }
            return true;
        }
        public static ulong[] BitMapTo(string word, char[] alphabet, ulong[] target, int offset = 0)
        {
            var charOrder = AlphabetPositions(alphabet);
            for (int i = 0; i < WordleData.wordLength; i++)
            {
                SetBits(target, offset, charOrder[word[i]] * WordleData.wordLength * 3 + i);
            }
            for (int i = 0; i < alphabet.Length; i++)
            {
                int occurences = 0;
                foreach (char c in word)
                {
                    if (c == alphabet[i]) occurences++;
                }
                SetBits(target, offset, i * WordleData.wordLength * 3 + WordleData.wordLength + occurences, WordleData.wordLength);
            }

            return target;
        }
        public static ulong[] BitMap(string word, char[] alphabet)
        {
            return BitMapTo(word, alphabet, blank(alphabet));
        }
        public static int LongsPerWord(char[] alphabet)
        {
            int size = alphabet.Length * WordleData.wordLength * 3;
            return size == 0 ? 0 : ((size - 1) >> 6) + 1;
        }
        public static ulong[] blank(char[] alphabet)
        {
            return new ulong[LongsPerWord(alphabet)];
        }
        public static ulong[] BitMask(IDictionary<char, WordleSolver.CharKnowledge> knowledge, char[] alphabet)
        {
            var result = blank(alphabet);

            for (int i = 0; i < alphabet.Length; i++)
            {
                char c = alphabet[i];
                if (knowledge.ContainsKey(c))
                {
                    var charKnowledge = knowledge[c];
                    //top part
                    foreach (int def in charKnowledge.definitePositions)
                    {
                        for (int j = 0; j < alphabet.Length; j++)
                        {
                            if (j != i)
                                SetBits(result, 0, j * WordleData.wordLength * 3 + def);
                        }
                    }

                    //top part
                    foreach (int unavailablePosition in charKnowledge.unavailablePositions)
                    {
                        SetBits(result, 0, i * WordleData.wordLength * 3 + unavailablePosition);
                    }

                    //middle part of mask
                    SetBits(result, 0, i * WordleData.wordLength * 3 + WordleData.wordLength, charKnowledge.definitePositions.Count + charKnowledge.atLeastThisManyMore);

                    //bottom part of mask
                    if (charKnowledge.grayBeyond != null)
                    {
                        SetBits(result, 0, i * WordleData.wordLength * 3 + WordleData.wordLength * 2 + (int)charKnowledge.grayBeyond, WordleData.wordLength - (int)charKnowledge.grayBeyond);
                    }
                }
            }
            return result;
        }
        //gives you a dictionary that maps the letter to the position, so if a = 0 then a maps to 0, b maps to 1, ...
        public static IDictionary<char, int> AlphabetPositions(char[] alphabet)
        {
            ISet<char> distinct = new SortedSet<char>(alphabet);
            WordleSolver.assert(alphabet.Length == distinct.Count);
            var result = new Dictionary<char, int>(alphabet.Length);
            int position = 0;
            for (int i = 0; i < alphabet.Length; i++)
            {
                result.Add(alphabet[i], position++);
            }

            return result;
        }

        public static string BitsToString(ulong[] bits, char[] alphabet, int offset = 0, char symbol = 'X')
        {
            int size = alphabet.Length * WordleData.wordLength * 3;
            int longs = size == 0 ? 0 : ((size - 1) >> 6) + 1;
            WordleSolver.assert(bits.Length % longs == 0);

            var sb = new StringBuilder();
            for (int longIndex = offset; longIndex < (offset == 0 ? bits.Length : offset + longs); longIndex += longs)
            {
                for (int i = 0; i < alphabet.Length; i++)
                    sb.Append(alphabet[i]);
                sb.AppendLine();
                for (int line = 0; line < WordleData.wordLength * 3; line++)
                {
                    if (line > 0 && (line % WordleData.wordLength) == 0)
                    {
                        sb.Append(new string('-', alphabet.Length));
                        sb.AppendLine();
                    }
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        int bitIndex = i * WordleData.wordLength * 3 + line;
                        ulong bitPack = bits[longIndex + (bitIndex >> 6)];
                        bool bitOn = ((bitPack >> (bitIndex & 63)) & 1) != 0;
                        sb.Append(bitOn ? symbol : ' ');
                    }
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static void SetBits(ulong[] arrayToEdit, int offset, int startBitIndex, int amount = 1)
        {
            if (amount > 0)
            {
                int firstLongIndex = startBitIndex >> 6;
                int lastLongIndex = (startBitIndex + amount - 1) >> 6;
                ulong firstMask = ulong.MaxValue << (startBitIndex & 63);
                int endBitIndex = (startBitIndex + amount) & 63;
                ulong lastMask = ~(endBitIndex == 0 ? 0L : ulong.MaxValue << endBitIndex);
                if (firstLongIndex == lastLongIndex)
                    arrayToEdit[offset + firstLongIndex] |= (firstMask & lastMask);
                else
                {
                    arrayToEdit[offset + firstLongIndex] |= firstMask;
                    arrayToEdit[offset + lastLongIndex] |= lastMask;
                    for (int i = firstLongIndex + 1; i < lastLongIndex; i++)
                    {
                        arrayToEdit[offset + i] = ulong.MaxValue;
                    }
                }
            }
        }
    }
}