using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の状態管理
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲーム設定")]
    public int initialBlockCount = 8;
    public float spawnDelay = 0.5f;

    [Header("参照")]
    public WordDictionary wordDictionary;
    public RectTransform playArea;
    public RectTransform blockContainer;

    [Header("UI参照")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public Button restartButton;

    [Header("プレハブ")]
    public GameObject englishBlockPrefab;
    public GameObject japaneseBlockPrefab;

    private int score = 0;
    private bool isPlaying = false;
    private float cellSize = 80f;
    private List<WordPair> currentWordPairs;
    
    // 現在配置されている英単語に対応する日本語ブロックのキュー
    private Queue<WordPair> japaneseBlockQueue;
    private List<WordPair> activeEnglishWords;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        StartGame();
    }

    /// <summary>
    /// ゲームを開始
    /// </summary>
    public void StartGame()
    {
        score = 0;
        isPlaying = true;
        UpdateScoreUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // GridManagerからセルサイズを取得
        if (GridManager.Instance != null)
        {
            cellSize = GridManager.Instance.cellSize;
        }

        // 辞書からランダムな単語ペアを取得
        if (wordDictionary != null)
        {
            currentWordPairs = wordDictionary.GetRandomPairs(initialBlockCount);
            activeEnglishWords = new List<WordPair>(currentWordPairs);
            
            // 日本語ブロックのキューを作成（シャッフルして順番をランダムに）
            List<WordPair> shuffledPairs = new List<WordPair>(currentWordPairs);
            ShuffleList(shuffledPairs);
            japaneseBlockQueue = new Queue<WordPair>(shuffledPairs);
            
            SpawnInitialBlocks();
        }
        else
        {
            Debug.LogError("WordDictionary is not assigned!");
        }
    }

    /// <summary>
    /// リストをシャッフル
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    /// <summary>
    /// 初期英単語ブロックを配置
    /// </summary>
    private void SpawnInitialBlocks()
    {
        if (currentWordPairs == null || currentWordPairs.Count == 0)
            return;

        for (int i = 0; i < Mathf.Min(currentWordPairs.Count, GridManager.Instance.rows); i++)
        {
            SpawnEnglishBlock(currentWordPairs[i], i);
        }

        // 最初の日本語ブロックを生成
        Invoke(nameof(SpawnNextBlock), spawnDelay);
    }

    /// <summary>
    /// 英単語ブロックを生成
    /// </summary>
    private void SpawnEnglishBlock(WordPair pair, int row)
    {
        if (englishBlockPrefab == null || blockContainer == null)
            return;

        GameObject blockObj = Instantiate(englishBlockPrefab, blockContainer);
        EnglishWordBlock block = blockObj.GetComponent<EnglishWordBlock>();

        if (block != null)
        {
            block.Initialize(pair.englishWord, row, cellSize);
        }
    }

    /// <summary>
    /// 次の日本語ブロックを生成
    /// </summary>
    public void SpawnNextBlock()
    {
        if (!isPlaying)
            return;

        // キューが空の場合はゲームクリア（すべて消去した）
        if (japaneseBlockQueue == null || japaneseBlockQueue.Count == 0)
        {
            // ゲームクリア処理
            GameClear();
            return;
        }

        // キューから次の単語ペアを取得
        WordPair pair = japaneseBlockQueue.Dequeue();

        // 対応する英単語の行を探す（または最初の空いている行）
        int row = FindRowForEnglishWord(pair.englishWord);
        if (row == -1)
        {
            row = Random.Range(0, GridManager.Instance.rows);
        }

        SpawnJapaneseBlock(pair, row);
    }

    /// <summary>
    /// 指定した英単語が配置されている行を探す
    /// </summary>
    private int FindRowForEnglishWord(string englishWord)
    {
        // GridManagerから英単語ブロックを探す
        // 注: 実際には行を指定せず、プレイヤーに選ばせる
        return Random.Range(0, GridManager.Instance.rows);
    }

    /// <summary>
    /// 日本語ブロックを生成
    /// </summary>
    private void SpawnJapaneseBlock(WordPair pair, int row)
    {
        if (japaneseBlockPrefab == null || blockContainer == null)
            return;

        GameObject blockObj = Instantiate(japaneseBlockPrefab, blockContainer);
        JapaneseBlock block = blockObj.GetComponent<JapaneseBlock>();

        if (block != null)
        {
            // 右端からスタート（PlayArea中央基準で右端は +playAreaWidth/2）
            float playAreaWidth = GridManager.Instance.GetPlayAreaWidth();
            float startX = playAreaWidth / 2f - cellSize / 2f;
            block.Initialize(pair.japaneseWord, pair.englishWord, row, cellSize, startX);
        }
    }

    /// <summary>
    /// スコアを加算
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    /// <summary>
    /// スコアUIを更新
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    /// <summary>
    /// ゲームクリア
    /// </summary>
    public void GameClear()
    {
        isPlaying = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"CLEAR! Score: {score}";
        }
    }

    /// <summary>
    /// ゲームオーバー
    /// </summary>
    public void GameOver()
    {
        isPlaying = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"Final Score: {score}";
        }
    }

    /// <summary>
    /// ゲームを再開
    /// </summary>
    public void RestartGame()
    {
        // すべてのブロックをクリア
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllBlocks();
        }

        StartGame();
    }
}
