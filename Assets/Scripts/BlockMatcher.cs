using UnityEngine;

/// <summary>
/// 衝突したブロックペアの意味照合と消去処理
/// </summary>
public class BlockMatcher : MonoBehaviour
{
    public static BlockMatcher Instance { get; private set; }

    [Header("参照")]
    public WordDictionary wordDictionary;

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

    /// <summary>
    /// 日本語ブロックと英単語ブロックの照合
    /// </summary>
    public void CheckMatch(JapaneseBlock japaneseBlock, EnglishWordBlock englishBlock)
    {
        if (wordDictionary == null)
        {
            Debug.LogError("WordDictionary is not assigned!");
            japaneseBlock.FixBlock();
            return;
        }

        bool isMatch = wordDictionary.IsMatch(englishBlock.englishWord, japaneseBlock.japaneseWord);

        if (isMatch)
        {
            // 一致した場合：両ブロック消去
            OnMatch(japaneseBlock, englishBlock);
        }
        else
        {
            // 不一致の場合：日本語ブロックを固定
            OnMismatch(japaneseBlock);
        }
    }

    /// <summary>
    /// 一致時の処理
    /// </summary>
    private void OnMatch(JapaneseBlock japaneseBlock, EnglishWordBlock englishBlock)
    {
        int row = englishBlock.Row;

        // スコア加算
        if (GameManager.Instance != null)
        {
            int points = englishBlock.englishWord.Length * 10;
            GameManager.Instance.AddScore(points);
        }

        // エフェクト表示（オプション）
        // TODO: パーティクルエフェクトなど

        // ブロック消去
        englishBlock.DestroyBlock();
        japaneseBlock.DestroyBlock();

        // 右側のブロックを左に詰める
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ShiftBlocksLeft(row);
        }

        // 次のブロックを生成
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnNextBlock();
        }
    }

    /// <summary>
    /// 不一致時の処理
    /// </summary>
    private void OnMismatch(JapaneseBlock japaneseBlock)
    {
        // 日本語ブロックを固定（障害物化）
        japaneseBlock.FixBlock();
    }
}
