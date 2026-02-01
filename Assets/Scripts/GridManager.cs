using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// グリッドベースの座標管理とブロック配置
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("グリッド設定")]
    public int columns = 16;
    public int rows = 8;
    public float cellSize = 80f;

    [Header("参照")]
    public RectTransform playArea;

    // グリッド上のブロック管理（各セルにどのブロックがあるか）
    private GameObject[,] grid;

    // 全ブロックのリスト
    private List<EnglishWordBlock> englishBlocks = new List<EnglishWordBlock>();
    private List<JapaneseBlock> japaneseBlocks = new List<JapaneseBlock>();

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

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new GameObject[columns, rows];
    }

    /// <summary>
    /// グリッド座標からワールド座標を取得
    /// </summary>
    public Vector2 GridToWorld(int col, int row)
    {
        float x = col * cellSize + cellSize / 2f;
        float y = -row * cellSize - cellSize / 2f;
        return new Vector2(x, y);
    }

    /// <summary>
    /// ワールド座標からグリッド座標を取得
    /// </summary>
    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int col = Mathf.FloorToInt(worldPos.x / cellSize);
        int row = Mathf.FloorToInt(-worldPos.y / cellSize);
        return new Vector2Int(col, row);
    }

    /// <summary>
    /// 英単語ブロックを登録
    /// </summary>
    public void RegisterEnglishBlock(EnglishWordBlock block)
    {
        if (!englishBlocks.Contains(block))
        {
            englishBlocks.Add(block);
        }
    }

    /// <summary>
    /// 英単語ブロックを登録解除
    /// </summary>
    public void UnregisterEnglishBlock(EnglishWordBlock block)
    {
        englishBlocks.Remove(block);
    }

    /// <summary>
    /// 日本語ブロックを登録
    /// </summary>
    public void RegisterJapaneseBlock(JapaneseBlock block)
    {
        if (!japaneseBlocks.Contains(block))
        {
            japaneseBlocks.Add(block);
        }
    }

    /// <summary>
    /// 日本語ブロックを登録解除
    /// </summary>
    public void UnregisterJapaneseBlock(JapaneseBlock block)
    {
        japaneseBlocks.Remove(block);
    }

    /// <summary>
    /// 指定行の最も右にあるブロックのX座標を取得
    /// </summary>
    public float GetRightmostBlockX(int row)
    {
        float maxX = 0;

        // 英単語ブロックをチェック
        foreach (var block in englishBlocks)
        {
            if (block.Row == row)
            {
                float rightEdge = block.GetRightEdgeX();
                if (rightEdge > maxX)
                {
                    maxX = rightEdge;
                }
            }
        }

        // 固定された日本語ブロックをチェック
        foreach (var block in japaneseBlocks)
        {
            if (block.Row == row && block.IsFixed)
            {
                float rightEdge = block.GetRightEdgeX();
                if (rightEdge > maxX)
                {
                    maxX = rightEdge;
                }
            }
        }

        return maxX;
    }

    /// <summary>
    /// 指定位置で衝突するブロックを取得
    /// </summary>
    public EnglishWordBlock GetCollidingEnglishBlock(int row, float xPos)
    {
        foreach (var block in englishBlocks)
        {
            if (block.Row == row)
            {
                float leftEdge = block.GetLeftEdgeX();
                float rightEdge = block.GetRightEdgeX();
                if (xPos >= leftEdge && xPos <= rightEdge + cellSize)
                {
                    return block;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 指定位置で衝突する固定日本語ブロックを取得
    /// </summary>
    public JapaneseBlock GetCollidingJapaneseBlock(int row, float xPos)
    {
        foreach (var block in japaneseBlocks)
        {
            if (block.Row == row && block.IsFixed)
            {
                float leftEdge = block.GetLeftEdgeX();
                float rightEdge = block.GetRightEdgeX();
                if (xPos >= leftEdge && xPos <= rightEdge + cellSize)
                {
                    return block;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// ブロック消去後、右側のブロックを左に詰める
    /// </summary>
    public void ShiftBlocksLeft(int row)
    {
        // 英単語ブロックを左に詰める
        List<EnglishWordBlock> rowBlocks = new List<EnglishWordBlock>();
        foreach (var block in englishBlocks)
        {
            if (block.Row == row)
            {
                rowBlocks.Add(block);
            }
        }

        // X座標でソート
        rowBlocks.Sort((a, b) => a.GetLeftEdgeX().CompareTo(b.GetLeftEdgeX()));

        float currentX = 0;
        foreach (var block in rowBlocks)
        {
            block.SetPositionX(currentX);
            currentX += block.GetWidth();
        }

        // 固定された日本語ブロックも左に詰める
        List<JapaneseBlock> fixedBlocks = new List<JapaneseBlock>();
        foreach (var block in japaneseBlocks)
        {
            if (block.Row == row && block.IsFixed)
            {
                fixedBlocks.Add(block);
            }
        }

        fixedBlocks.Sort((a, b) => a.GetLeftEdgeX().CompareTo(b.GetLeftEdgeX()));

        foreach (var block in fixedBlocks)
        {
            block.SetPositionX(currentX);
            currentX += cellSize;
        }
    }

    /// <summary>
    /// 右端に到達したかチェック（ゲームオーバー判定）
    /// </summary>
    public bool IsGameOverCondition()
    {
        float rightBoundary = (columns - 1) * cellSize;

        foreach (var block in japaneseBlocks)
        {
            if (block.IsFixed && block.GetRightEdgeX() >= rightBoundary)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// すべてのブロックをクリア
    /// </summary>
    public void ClearAllBlocks()
    {
        foreach (var block in englishBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        englishBlocks.Clear();

        foreach (var block in japaneseBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        japaneseBlocks.Clear();

        InitializeGrid();
    }

    /// <summary>
    /// プレイエリアの幅を取得
    /// </summary>
    public float GetPlayAreaWidth()
    {
        return columns * cellSize;
    }

    /// <summary>
    /// プレイエリアの高さを取得
    /// </summary>
    public float GetPlayAreaHeight()
    {
        return rows * cellSize;
    }
}
