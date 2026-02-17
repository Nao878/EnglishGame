using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ゲームモードの列挙型
/// </summary>
public enum GameMode
{
    Easy,           // かんたんモード
    JuniorHigh1     // 中学1年生モード
}

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
    public List<WordPair> easyWordPairs = new List<WordPair>();
    public List<WordPair> juniorHigh1WordPairs = new List<WordPair>();

    /// <summary>
    /// 指定モードの単語リストを取得
    /// </summary>
    public List<WordPair> GetWordPairs(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Easy:
                return easyWordPairs;
            case GameMode.JuniorHigh1:
                return juniorHigh1WordPairs;
            default:
                return easyWordPairs;
        }
    }

    /// <summary>
    /// 英単語から日本語を取得
    /// </summary>
    public string GetJapanese(string english, GameMode mode)
    {
        foreach (var pair in GetWordPairs(mode))
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
    public string GetEnglish(string japanese, GameMode mode)
    {
        foreach (var pair in GetWordPairs(mode))
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
        // すべてのモードで検索
        foreach (var pair in easyWordPairs)
        {
            if (pair.englishWord.Equals(english, StringComparison.OrdinalIgnoreCase) &&
                pair.japaneseWord == japanese)
            {
                return true;
            }
        }
        foreach (var pair in juniorHigh1WordPairs)
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
    public WordPair GetRandomPair(GameMode mode)
    {
        var wordPairs = GetWordPairs(mode);
        if (wordPairs.Count == 0) return null;
        return wordPairs[UnityEngine.Random.Range(0, wordPairs.Count)];
    }

    /// <summary>
    /// 指定数のランダムな単語ペアを取得（重複なし）
    /// </summary>
    public List<WordPair> GetRandomPairs(int count, GameMode mode)
    {
        List<WordPair> result = new List<WordPair>();
        List<WordPair> available = new List<WordPair>(GetWordPairs(mode));

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
    /// かんたんモードの辞書データを初期化
    /// </summary>
    public void InitializeEasyData()
    {
        easyWordPairs.Clear();
        easyWordPairs.Add(new WordPair("apple", "りんご"));
        easyWordPairs.Add(new WordPair("book", "本"));
        easyWordPairs.Add(new WordPair("cat", "ねこ"));
        easyWordPairs.Add(new WordPair("dog", "いぬ"));
        easyWordPairs.Add(new WordPair("fish", "さかな"));
        easyWordPairs.Add(new WordPair("egg", "たまご"));
        easyWordPairs.Add(new WordPair("hand", "手"));
        easyWordPairs.Add(new WordPair("ice", "氷"));
        easyWordPairs.Add(new WordPair("jump", "とぶ"));
        easyWordPairs.Add(new WordPair("king", "王様"));
        easyWordPairs.Add(new WordPair("lion", "ライオン"));
        easyWordPairs.Add(new WordPair("moon", "月"));
        easyWordPairs.Add(new WordPair("nose", "鼻"));
        easyWordPairs.Add(new WordPair("orange", "オレンジ"));
        easyWordPairs.Add(new WordPair("pen", "ペン"));
        easyWordPairs.Add(new WordPair("queen", "女王"));
        easyWordPairs.Add(new WordPair("rain", "雨"));
        easyWordPairs.Add(new WordPair("sun", "太陽"));
        easyWordPairs.Add(new WordPair("tree", "木"));
        easyWordPairs.Add(new WordPair("water", "水"));
    }

    /// <summary>
    /// 中学1年生モードの辞書データを初期化
    /// </summary>
    public void InitializeJuniorHigh1Data()
    {
        juniorHigh1WordPairs.Clear();
        // 基本的な名詞
        juniorHigh1WordPairs.Add(new WordPair("school", "学校"));
        juniorHigh1WordPairs.Add(new WordPair("student", "生徒"));
        juniorHigh1WordPairs.Add(new WordPair("teacher", "先生"));
        juniorHigh1WordPairs.Add(new WordPair("friend", "友達"));
        juniorHigh1WordPairs.Add(new WordPair("family", "家族"));
        juniorHigh1WordPairs.Add(new WordPair("mother", "母"));
        juniorHigh1WordPairs.Add(new WordPair("father", "父"));
        juniorHigh1WordPairs.Add(new WordPair("sister", "姉妹"));
        juniorHigh1WordPairs.Add(new WordPair("brother", "兄弟"));
        juniorHigh1WordPairs.Add(new WordPair("morning", "朝"));
        // 基本的な動詞
        juniorHigh1WordPairs.Add(new WordPair("study", "勉強する"));
        juniorHigh1WordPairs.Add(new WordPair("speak", "話す"));
        juniorHigh1WordPairs.Add(new WordPair("listen", "聞く"));
        juniorHigh1WordPairs.Add(new WordPair("write", "書く"));
        juniorHigh1WordPairs.Add(new WordPair("read", "読む"));
        juniorHigh1WordPairs.Add(new WordPair("play", "遊ぶ"));
        juniorHigh1WordPairs.Add(new WordPair("walk", "歩く"));
        juniorHigh1WordPairs.Add(new WordPair("run", "走る"));
        juniorHigh1WordPairs.Add(new WordPair("eat", "食べる"));
        juniorHigh1WordPairs.Add(new WordPair("drink", "飲む"));
        // 基本的な形容詞
        juniorHigh1WordPairs.Add(new WordPair("good", "良い"));
        juniorHigh1WordPairs.Add(new WordPair("bad", "悪い"));
        juniorHigh1WordPairs.Add(new WordPair("big", "大きい"));
        juniorHigh1WordPairs.Add(new WordPair("small", "小さい"));
        juniorHigh1WordPairs.Add(new WordPair("new", "新しい"));
        juniorHigh1WordPairs.Add(new WordPair("old", "古い"));
        juniorHigh1WordPairs.Add(new WordPair("happy", "幸せ"));
        juniorHigh1WordPairs.Add(new WordPair("sad", "悲しい"));
        // 曜日・時間
        juniorHigh1WordPairs.Add(new WordPair("Monday", "月曜日"));
        juniorHigh1WordPairs.Add(new WordPair("Tuesday", "火曜日"));
        juniorHigh1WordPairs.Add(new WordPair("Wednesday", "水曜日"));
        juniorHigh1WordPairs.Add(new WordPair("Thursday", "木曜日"));
        juniorHigh1WordPairs.Add(new WordPair("Friday", "金曜日"));
        juniorHigh1WordPairs.Add(new WordPair("Saturday", "土曜日"));
        juniorHigh1WordPairs.Add(new WordPair("Sunday", "日曜日"));
        juniorHigh1WordPairs.Add(new WordPair("today", "今日"));
        juniorHigh1WordPairs.Add(new WordPair("tomorrow", "明日"));
        juniorHigh1WordPairs.Add(new WordPair("yesterday", "昨日"));
        // 数字
        juniorHigh1WordPairs.Add(new WordPair("one", "1"));
        juniorHigh1WordPairs.Add(new WordPair("two", "2"));
        juniorHigh1WordPairs.Add(new WordPair("three", "3"));
        juniorHigh1WordPairs.Add(new WordPair("four", "4"));
        juniorHigh1WordPairs.Add(new WordPair("five", "5"));
        juniorHigh1WordPairs.Add(new WordPair("six", "6"));
        juniorHigh1WordPairs.Add(new WordPair("seven", "7"));
        juniorHigh1WordPairs.Add(new WordPair("eight", "8"));
        juniorHigh1WordPairs.Add(new WordPair("nine", "9"));
        juniorHigh1WordPairs.Add(new WordPair("ten", "10"));
        // 場所・物
        juniorHigh1WordPairs.Add(new WordPair("house", "家"));
        juniorHigh1WordPairs.Add(new WordPair("room", "部屋"));
        juniorHigh1WordPairs.Add(new WordPair("desk", "机"));
        juniorHigh1WordPairs.Add(new WordPair("chair", "椅子"));
        juniorHigh1WordPairs.Add(new WordPair("window", "窓"));
        juniorHigh1WordPairs.Add(new WordPair("door", "ドア"));
        juniorHigh1WordPairs.Add(new WordPair("bag", "かばん"));
        juniorHigh1WordPairs.Add(new WordPair("clock", "時計"));
    }

    /// <summary>
    /// すべてのモードのデータを初期化
    /// </summary>
    public void InitializeAllData()
    {
        InitializeEasyData();
        InitializeJuniorHigh1Data();
    }

    /// <summary>
    /// モード名を取得
    /// </summary>
    public static string GetModeName(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Easy:
                return "かんたん";
            case GameMode.JuniorHigh1:
                return "中学1年生";
            default:
                return "かんたん";
        }
    }
}
