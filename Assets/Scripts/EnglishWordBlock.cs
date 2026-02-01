using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 英単語ブロックの表示・動作管理
/// </summary>
public class EnglishWordBlock : MonoBehaviour
{
    [Header("表示設定")]
    public string englishWord;
    public int row;

    [Header("UI参照")]
    public RectTransform rectTransform;
    public Image backgroundImage;
    public TextMeshProUGUI textComponent;

    // 色設定
    private static readonly Color BACKGROUND_COLOR = new Color(0.204f, 0.596f, 0.859f); // #3498db
    private static readonly Color TEXT_COLOR = Color.white;

    private float width;

    public int Row => row;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// ブロックを初期化
    /// </summary>
    public void Initialize(string word, int rowIndex, float cellSize)
    {
        englishWord = word;
        row = rowIndex;

        // 文字数に応じた幅を計算（1文字 = 1セル）
        width = word.Length * cellSize;

        // RectTransformのサイズを設定
        rectTransform.sizeDelta = new Vector2(width, cellSize);

        // 背景色を設定
        if (backgroundImage != null)
        {
            backgroundImage.color = BACKGROUND_COLOR;
        }

        // テキストを設定
        if (textComponent != null)
        {
            textComponent.text = word;
            textComponent.color = TEXT_COLOR;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontSize = cellSize * 0.4f;
        }

        // 位置を設定（PlayArea中央を基準に配置）
        // X: 左端からスタート（PlayAreaの左端 + ブロック幅の半分）
        float playAreaWidth = GridManager.Instance.GetPlayAreaWidth();
        float playAreaHeight = GridManager.Instance.GetPlayAreaHeight();
        float x = -playAreaWidth / 2f + width / 2f;
        // Y: 上から順に配置（PlayAreaの上端から各行へ）
        float y = playAreaHeight / 2f - rowIndex * cellSize - cellSize / 2f;
        rectTransform.anchoredPosition = new Vector2(x, y);

        // GridManagerに登録
        if (GridManager.Instance != null)
        {
            GridManager.Instance.RegisterEnglishBlock(this);
        }
    }

    /// <summary>
    /// ブロックの幅を取得
    /// </summary>
    public float GetWidth()
    {
        return width;
    }

    /// <summary>
    /// 左端のX座標を取得
    /// </summary>
    public float GetLeftEdgeX()
    {
        return rectTransform.anchoredPosition.x - width / 2f;
    }

    /// <summary>
    /// 右端のX座標を取得
    /// </summary>
    public float GetRightEdgeX()
    {
        return rectTransform.anchoredPosition.x + width / 2f;
    }

    /// <summary>
    /// X座標を設定（左端基準）
    /// </summary>
    public void SetPositionX(float leftEdgeX)
    {
        float centerX = leftEdgeX + width / 2f;
        rectTransform.anchoredPosition = new Vector2(centerX, rectTransform.anchoredPosition.y);
    }

    /// <summary>
    /// ブロックを消去
    /// </summary>
    public void DestroyBlock()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterEnglishBlock(this);
        }
        Destroy(gameObject);
    }
}
