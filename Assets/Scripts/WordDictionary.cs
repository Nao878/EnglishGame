using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 英単語と日本語のペアを管理するデータ構造
/// </summary>
[Serializable]
public class WordPair
{
    public string englishWord;
    public string japaneseWord;

    public WordPair(string english, string japanese)
    {
        englishWord = english;
        japaneseWord = japanese;
    }
}

/// <summary>
/// 英単語辞書データ（ScriptableObject）
/// </summary>
[CreateAssetMenu(fileName = "WordDictionary", menuName = "EnglishWordTetris/WordDictionary")]
public class WordDictionary : ScriptableObject
{
    public List<WordPair> wordPairs = new List<WordPair>();

    /// <summary>
    /// 英単語から日本語を取得
    /// </summary>
    public string GetJapanese(string english)
    {
        foreach (var pair in wordPairs)
        {
            if (pair.englishWord.Equals(english, StringComparison.OrdinalIgnoreCase))
            {
                return pair.japaneseWord;
            }
        }
        return null;
    }

    /// <summary>
    /// 日本語から英単語を取得
    /// </summary>
    public string GetEnglish(string japanese)
    {
        foreach (var pair in wordPairs)
        {
            if (pair.japaneseWord == japanese)
            {
                return pair.englishWord;
            }
        }
        return null;
    }

    /// <summary>
    /// 指定した英単語と日本語が一致するか判定
    /// </summary>
    public bool IsMatch(string english, string japanese)
    {
        foreach (var pair in wordPairs)
        {
            if (pair.englishWord.Equals(english, StringComparison.OrdinalIgnoreCase) &&
                pair.japaneseWord == japanese)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ランダムな単語ペアを取得
    /// </summary>
    public WordPair GetRandomPair()
    {
        if (wordPairs.Count == 0) return null;
        return wordPairs[UnityEngine.Random.Range(0, wordPairs.Count)];
    }

    /// <summary>
    /// 指定数のランダムな単語ペアを取得（重複なし）
    /// </summary>
    public List<WordPair> GetRandomPairs(int count)
    {
        List<WordPair> result = new List<WordPair>();
        List<WordPair> available = new List<WordPair>(wordPairs);

        count = Mathf.Min(count, available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, available.Count);
            result.Add(available[index]);
            available.RemoveAt(index);
        }

        return result;
    }

    /// <summary>
    /// デフォルトの辞書データを初期化
    /// </summary>
    public void InitializeDefaultData()
    {
        wordPairs.Clear();
        wordPairs.Add(new WordPair("apple", "りんご"));
        wordPairs.Add(new WordPair("book", "本"));
        wordPairs.Add(new WordPair("cat", "ねこ"));
        wordPairs.Add(new WordPair("dog", "いぬ"));
        wordPairs.Add(new WordPair("fish", "さかな"));
        wordPairs.Add(new WordPair("egg", "たまご"));
        wordPairs.Add(new WordPair("hand", "手"));
        wordPairs.Add(new WordPair("ice", "氷"));
        wordPairs.Add(new WordPair("jump", "とぶ"));
        wordPairs.Add(new WordPair("king", "王様"));
        wordPairs.Add(new WordPair("lion", "ライオン"));
        wordPairs.Add(new WordPair("moon", "月"));
        wordPairs.Add(new WordPair("nose", "鼻"));
        wordPairs.Add(new WordPair("orange", "オレンジ"));
        wordPairs.Add(new WordPair("pen", "ペン"));
        wordPairs.Add(new WordPair("queen", "女王"));
        wordPairs.Add(new WordPair("rain", "雨"));
        wordPairs.Add(new WordPair("sun", "太陽"));
        wordPairs.Add(new WordPair("tree", "木"));
        wordPairs.Add(new WordPair("water", "水"));
    }
}
