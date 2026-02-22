using System;
using UnityEngine;

[Serializable]
public class WordPair
{
    public string word1;
    public string word2;

    public WordPair(string w1, string w2)
    {
        word1 = w1;
        word2 = w2;
    }
}
