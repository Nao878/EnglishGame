using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 英単語テトリスのシーンセットアップツール
/// </summary>
public class GameSetupTool : EditorWindow
{
    private static TMP_FontAsset appFont;

    [MenuItem("Tools/Setup English Word Tetris")]
    public static void SetupScene()
    {
        // フォントを読み込む
        appFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/AppFont SDF.asset");
        if (appFont == null)
        {
            Debug.LogWarning("AppFont SDF not found. Using default font.");
        }

        // 既存のゲームオブジェクトを削除
        CleanupExistingObjects();

        // WordDictionaryアセットを作成
        WordDictionary wordDictionary = CreateWordDictionary();

        // Canvas作成
        GameObject canvasObj = CreateCanvas();
        Canvas canvas = canvasObj.GetComponent<Canvas>();

        // GridManager作成
        GameObject gridManagerObj = CreateGridManager(canvasObj.transform);
        GridManager gridManager = gridManagerObj.GetComponent<GridManager>();

        // プレイエリア作成
        GameObject playAreaObj = CreatePlayArea(canvasObj.transform, gridManager);
        RectTransform playAreaRect = playAreaObj.GetComponent<RectTransform>();
        gridManager.playArea = playAreaRect;

        // ブロックコンテナ作成
        GameObject blockContainerObj = CreateBlockContainer(playAreaObj.transform);
        RectTransform blockContainerRect = blockContainerObj.GetComponent<RectTransform>();

        // BlockMatcher作成
        GameObject blockMatcherObj = CreateBlockMatcher(canvasObj.transform, wordDictionary);

        // GameManager作成
        GameObject gameManagerObj = CreateGameManager(canvasObj.transform, wordDictionary, playAreaRect, blockContainerRect);
        GameManager gameManager = gameManagerObj.GetComponent<GameManager>();

        // UI作成
        CreateUI(canvasObj.transform, gameManager);

        // プレハブ作成
        CreatePrefabs(gameManager);

        Debug.Log("English Word Tetris setup complete!");
        
        // シーンを保存
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    private static void CleanupExistingObjects()
    {
        string[] objectNames = { "GameCanvas", "GridManager", "BlockMatcher", "GameManager" };
        foreach (string name in objectNames)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                DestroyImmediate(existing);
            }
        }
    }

    private static WordDictionary CreateWordDictionary()
    {
        string path = "Assets/Data/WordDictionary.asset";
        
        // Dataフォルダがなければ作成
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        // 既存のアセットを取得または新規作成
        WordDictionary wordDictionary = AssetDatabase.LoadAssetAtPath<WordDictionary>(path);
        if (wordDictionary == null)
        {
            wordDictionary = ScriptableObject.CreateInstance<WordDictionary>();
            wordDictionary.InitializeDefaultData();
            AssetDatabase.CreateAsset(wordDictionary, path);
            AssetDatabase.SaveAssets();
        }

        return wordDictionary;
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem作成（なければ）
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    private static GameObject CreateGridManager(Transform parent)
    {
        GameObject obj = new GameObject("GridManager");
        obj.transform.SetParent(parent);
        
        GridManager gridManager = obj.AddComponent<GridManager>();
        gridManager.columns = 16;
        gridManager.rows = 8;
        gridManager.cellSize = 80f;

        return obj;
    }

    private static GameObject CreatePlayArea(Transform parent, GridManager gridManager)
    {
        GameObject obj = new GameObject("PlayArea");
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(gridManager.columns * gridManager.cellSize, gridManager.rows * gridManager.cellSize);

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        // 境界線を作成
        CreateBorders(obj.transform, gridManager);

        return obj;
    }

    private static void CreateBorders(Transform parent, GridManager gridManager)
    {
        float width = gridManager.columns * gridManager.cellSize;
        float height = gridManager.rows * gridManager.cellSize;
        float borderThickness = 4f;
        Color borderColor = new Color(0.3f, 0.5f, 0.8f);

        // 上
        CreateBorder(parent, "TopBorder", new Vector2(0, height / 2 + borderThickness / 2), new Vector2(width + borderThickness * 2, borderThickness), borderColor);
        // 下
        CreateBorder(parent, "BottomBorder", new Vector2(0, -height / 2 - borderThickness / 2), new Vector2(width + borderThickness * 2, borderThickness), borderColor);
        // 左
        CreateBorder(parent, "LeftBorder", new Vector2(-width / 2 - borderThickness / 2, 0), new Vector2(borderThickness, height), borderColor);
        // 右（ゲームオーバーライン）
        CreateBorder(parent, "RightBorder", new Vector2(width / 2 + borderThickness / 2, 0), new Vector2(borderThickness, height), new Color(0.9f, 0.3f, 0.3f));
    }

    private static void CreateBorder(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;
    }

    private static GameObject CreateBlockContainer(Transform parent)
    {
        GameObject obj = new GameObject("BlockContainer");
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        // 中央を基準にしてブロックを配置（PlayAreaと同じアンカー）
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        return obj;
    }

    private static GameObject CreateBlockMatcher(Transform parent, WordDictionary wordDictionary)
    {
        GameObject obj = new GameObject("BlockMatcher");
        obj.transform.SetParent(parent);

        BlockMatcher matcher = obj.AddComponent<BlockMatcher>();
        matcher.wordDictionary = wordDictionary;

        return obj;
    }

    private static GameObject CreateGameManager(Transform parent, WordDictionary wordDictionary, RectTransform playArea, RectTransform blockContainer)
    {
        GameObject obj = new GameObject("GameManager");
        obj.transform.SetParent(parent);

        GameManager manager = obj.AddComponent<GameManager>();
        manager.wordDictionary = wordDictionary;
        manager.playArea = playArea;
        manager.blockContainer = blockContainer;
        manager.initialBlockCount = 8;
        manager.spawnDelay = 0.5f;

        return obj;
    }

    private static void CreateUI(Transform parent, GameManager gameManager)
    {
        // スコア表示
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(parent);

        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0, -20);
        scoreRect.sizeDelta = new Vector2(400, 60);

        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "Score: 0";
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        if (appFont != null) scoreText.font = appFont;

        gameManager.scoreText = scoreText;

        // 操作説明
        GameObject instructionObj = new GameObject("InstructionText");
        instructionObj.transform.SetParent(parent);

        RectTransform instructionRect = instructionObj.AddComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.5f, 0f);
        instructionRect.anchorMax = new Vector2(0.5f, 0f);
        instructionRect.pivot = new Vector2(0.5f, 0f);
        instructionRect.anchoredPosition = new Vector2(0, 20);
        instructionRect.sizeDelta = new Vector2(600, 40);

        TextMeshProUGUI instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.text = "↑↓: 行移動　Space: 加速";
        instructionText.fontSize = 24;
        instructionText.color = new Color(0.7f, 0.7f, 0.7f);
        instructionText.alignment = TextAlignmentOptions.Center;
        if (appFont != null) instructionText.font = appFont;

        // ゲームオーバーパネル
        CreateGameOverPanel(parent, gameManager);
    }

    private static void CreateGameOverPanel(Transform parent, GameManager gameManager)
    {
        GameObject panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(parent);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        gameManager.gameOverPanel = panelObj;
        panelObj.SetActive(false);

        // ゲームオーバーテキスト
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(panelObj.transform);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 80);
        titleRect.sizeDelta = new Vector2(400, 80);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 56;
        titleText.color = new Color(0.9f, 0.3f, 0.3f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        if (appFont != null) titleText.font = appFont;

        // 最終スコア
        GameObject finalScoreObj = new GameObject("FinalScoreText");
        finalScoreObj.transform.SetParent(panelObj.transform);

        RectTransform finalScoreRect = finalScoreObj.AddComponent<RectTransform>();
        finalScoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalScoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalScoreRect.pivot = new Vector2(0.5f, 0.5f);
        finalScoreRect.anchoredPosition = new Vector2(0, 10);
        finalScoreRect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI finalScoreText = finalScoreObj.AddComponent<TextMeshProUGUI>();
        finalScoreText.text = "Final Score: 0";
        finalScoreText.fontSize = 32;
        finalScoreText.color = Color.white;
        finalScoreText.alignment = TextAlignmentOptions.Center;
        if (appFont != null) finalScoreText.font = appFont;

        gameManager.gameOverScoreText = finalScoreText;

        // リスタートボタン
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(panelObj.transform);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -60);
        buttonRect.sizeDelta = new Vector2(200, 50);

        Image buttonBg = buttonObj.AddComponent<Image>();
        buttonBg.color = new Color(0.2f, 0.6f, 0.9f);

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.7f);
        button.colors = colors;

        gameManager.restartButton = button;

        // ボタンテキスト
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Restart";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        if (appFont != null) buttonText.font = appFont;
    }

    private static void CreatePrefabs(GameManager gameManager)
    {
        // Prefabsフォルダがなければ作成
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 英単語ブロックプレハブ
        gameManager.englishBlockPrefab = CreateEnglishBlockPrefab();

        // 日本語ブロックプレハブ
        gameManager.japaneseBlockPrefab = CreateJapaneseBlockPrefab();
    }

    private static GameObject CreateEnglishBlockPrefab()
    {
        string prefabPath = "Assets/Prefabs/EnglishWordBlock.prefab";

        GameObject blockObj = new GameObject("EnglishWordBlock");

        RectTransform rect = blockObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);

        Image bg = blockObj.AddComponent<Image>();
        bg.color = new Color(0.204f, 0.596f, 0.859f);

        EnglishWordBlock block = blockObj.AddComponent<EnglishWordBlock>();
        block.rectTransform = rect;
        block.backgroundImage = bg;

        // テキスト
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(blockObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 32;
        if (appFont != null) text.font = appFont;

        block.textComponent = text;

        // プレハブとして保存
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(blockObj, prefabPath);
        DestroyImmediate(blockObj);

        return prefab;
    }

    private static GameObject CreateJapaneseBlockPrefab()
    {
        string prefabPath = "Assets/Prefabs/JapaneseBlock.prefab";

        GameObject blockObj = new GameObject("JapaneseBlock");

        RectTransform rect = blockObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);

        Image bg = blockObj.AddComponent<Image>();
        bg.color = new Color(0.906f, 0.298f, 0.235f);

        JapaneseBlock block = blockObj.AddComponent<JapaneseBlock>();
        block.rectTransform = rect;
        block.backgroundImage = bg;
        block.moveSpeed = 100f;
        block.acceleratedSpeed = 300f;

        // テキスト
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(blockObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 28;
        if (appFont != null) text.font = appFont;

        block.textComponent = text;

        // プレハブとして保存
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(blockObj, prefabPath);
        DestroyImmediate(blockObj);

        return prefab;
    }
}
