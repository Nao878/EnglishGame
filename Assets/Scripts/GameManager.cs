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
            SpawnInitialBlocks();
        }
        else
        {
            Debug.LogError("WordDictionary is not assigned!");
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
        if (!isPlaying || wordDictionary == null)
            return;

        // ランダムな単語ペアを取得
        WordPair pair = wordDictionary.GetRandomPair();
        if (pair == null)
            return;

        // ランダムな行に配置
        int row = Random.Range(0, GridManager.Instance.rows);

        SpawnJapaneseBlock(pair, row);
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
            // 右端からスタート
            float startX = GridManager.Instance.GetPlayAreaWidth() - cellSize / 2f;
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
