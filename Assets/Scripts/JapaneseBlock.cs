using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 日本語ブロック（操作対象）の管理
/// </summary>
public class JapaneseBlock : MonoBehaviour
{
    [Header("表示設定")]
    public string japaneseWord;
    public string correspondingEnglish;
    public int row;

    [Header("移動設定")]
    public float moveSpeed = 100f;
    public float acceleratedSpeed = 300f;

    [Header("UI参照")]
    public RectTransform rectTransform;
    public Image backgroundImage;
    public TextMeshProUGUI textComponent;

    // 色設定
    private static readonly Color ACTIVE_BG_COLOR = new Color(0.906f, 0.298f, 0.235f); // #e74c3c
    private static readonly Color FIXED_BG_COLOR = new Color(0.584f, 0.647f, 0.651f); // #95a5a6
    private static readonly Color ACTIVE_TEXT_COLOR = Color.white;
    private static readonly Color FIXED_TEXT_COLOR = new Color(0.173f, 0.243f, 0.314f); // #2c3e50

    private float cellSize;
    private bool isFixed = false;
    private bool isAccelerating = false;
    private GameManager gameManager;

    // Input System用
    private bool upKeyPressed = false;
    private bool downKeyPressed = false;

    public int Row => row;
    public bool IsFixed => isFixed;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (isFixed || gameManager == null || !gameManager.IsPlaying)
            return;

        HandleInput();
        MoveLeft();
        CheckCollision();
    }

    /// <summary>
    /// ブロックを初期化
    /// </summary>
    public void Initialize(string japanese, string english, int rowIndex, float size, float startX)
    {
        japaneseWord = japanese;
        correspondingEnglish = english;
        row = rowIndex;
        cellSize = size;

        // サイズを設定（1×1マス）
        rectTransform.sizeDelta = new Vector2(cellSize, cellSize);

        // 背景色を設定
        if (backgroundImage != null)
        {
            backgroundImage.color = ACTIVE_BG_COLOR;
        }

        // テキストを設定
        if (textComponent != null)
        {
            textComponent.text = japanese;
            textComponent.color = ACTIVE_TEXT_COLOR;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontSize = cellSize * 0.35f;
        }

        // 位置を設定（PlayArea中央を基準に配置）
        float playAreaHeight = GridManager.Instance.GetPlayAreaHeight();
        float y = playAreaHeight / 2f - rowIndex * cellSize - cellSize / 2f;
        rectTransform.anchoredPosition = new Vector2(startX, y);

        // GridManagerに登録
        if (GridManager.Instance != null)
        {
            GridManager.Instance.RegisterJapaneseBlock(this);
        }
    }

    /// <summary>
    /// 入力処理（Input System対応）
    /// </summary>
    private void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 上キーで上の行へ移動
        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            MoveRow(-1);
        }
        // 下キーで下の行へ移動
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            MoveRow(1);
        }

        // スペースキーで加速
        isAccelerating = keyboard.spaceKey.isPressed;
    }

    /// <summary>
    /// 行を移動
    /// </summary>
    private void MoveRow(int direction)
    {
        int newRow = row + direction;
        if (newRow >= 0 && newRow < GridManager.Instance.rows)
        {
            row = newRow;
            float playAreaHeight = GridManager.Instance.GetPlayAreaHeight();
            float y = playAreaHeight / 2f - row * cellSize - cellSize / 2f;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
        }
    }

    /// <summary>
    /// 左に移動
    /// </summary>
    private void MoveLeft()
    {
        float speed = isAccelerating ? acceleratedSpeed : moveSpeed;
        float newX = rectTransform.anchoredPosition.x - speed * Time.deltaTime;
        rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);
    }

    /// <summary>
    /// 衝突判定
    /// </summary>
    private void CheckCollision()
    {
        float leftEdge = GetLeftEdgeX();

        // 英単語ブロックとの衝突チェック
        EnglishWordBlock englishBlock = GridManager.Instance.GetCollidingEnglishBlock(row, leftEdge);
        if (englishBlock != null)
        {
            // 衝突位置で停止
            float stopX = englishBlock.GetRightEdgeX() + cellSize / 2f;
            rectTransform.anchoredPosition = new Vector2(stopX, rectTransform.anchoredPosition.y);

            // 意味照合
            if (BlockMatcher.Instance != null)
            {
                BlockMatcher.Instance.CheckMatch(this, englishBlock);
            }
            return;
        }

        // 固定された日本語ブロックとの衝突チェック
        JapaneseBlock fixedBlock = GridManager.Instance.GetCollidingJapaneseBlock(row, leftEdge);
        if (fixedBlock != null && fixedBlock != this)
        {
            // 衝突位置で停止して固定
            float stopX = fixedBlock.GetRightEdgeX() + cellSize / 2f;
            rectTransform.anchoredPosition = new Vector2(stopX, rectTransform.anchoredPosition.y);
            FixBlock();
            return;
        }

        // 左端到達チェック（PlayArea左端は -playAreaWidth/2）
        float playAreaWidth = GridManager.Instance.GetPlayAreaWidth();
        float leftBoundary = -playAreaWidth / 2f;
        if (leftEdge <= leftBoundary)
        {
            rectTransform.anchoredPosition = new Vector2(leftBoundary + cellSize / 2f, rectTransform.anchoredPosition.y);
            FixBlock();
        }
    }

    /// <summary>
    /// ブロックを固定
    /// </summary>
    public void FixBlock()
    {
        if (isFixed) return;

        isFixed = true;

        // 色を変更
        if (backgroundImage != null)
        {
            backgroundImage.color = FIXED_BG_COLOR;
        }
        if (textComponent != null)
        {
            textComponent.color = FIXED_TEXT_COLOR;
        }

        // ゲームオーバーチェック
        if (GridManager.Instance != null && GridManager.Instance.IsGameOverCondition())
        {
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
        else
        {
            // 次のブロックを生成
            if (gameManager != null)
            {
                gameManager.SpawnNextBlock();
            }
        }
    }

    /// <summary>
    /// 左端のX座標を取得
    /// </summary>
    public float GetLeftEdgeX()
    {
        return rectTransform.anchoredPosition.x - cellSize / 2f;
    }

    /// <summary>
    /// 右端のX座標を取得
    /// </summary>
    public float GetRightEdgeX()
    {
        return rectTransform.anchoredPosition.x + cellSize / 2f;
    }

    /// <summary>
    /// X座標を設定（左端基準）
    /// </summary>
    public void SetPositionX(float leftEdgeX)
    {
        float centerX = leftEdgeX + cellSize / 2f;
        rectTransform.anchoredPosition = new Vector2(centerX, rectTransform.anchoredPosition.y);
    }

    /// <summary>
    /// ブロックを消去
    /// </summary>
    public void DestroyBlock()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterJapaneseBlock(this);
        }
        Destroy(gameObject);
    }
}
