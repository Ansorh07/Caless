using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Premium UI Manager for Caless.
/// Tab-based navigation: Play, Education, Settings.
/// Dark fantasy theme with gold accents matching the "Royal Chess" style.
/// </summary>
public class UIManager : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    private Canvas canvas;
    private CanvasScaler canvasScaler;

    // Panels
    private GameObject menuPanel;
    private GameObject playSubMenu;
    private GameObject settingsPanel;
    private GameObject gameUIPanel;
    private GameObject gameOverPanel;
    private GameObject trainingUIPanel;
    private GameObject difficultyPanel;
    private GameObject sidePanel;
    private GameObject dialogPanel;

    // Tab system
    private enum Tab { Play, Education, Settings }
    private Tab currentTab = Tab.Play;
    private GameObject tabBar;
    private GameObject tabContentArea;

    // Game UI texts
    private Text turnText;
    private Text moveCountText;
    private Text captureText;
    private Text statusText;
    private Text thinkingText;

    private Font cachedFont;

    // Theme colors
    private static readonly Color BG_DARK = new Color(0.06f, 0.05f, 0.04f, 1f);
    private static readonly Color BG_PANEL = new Color(0.10f, 0.08f, 0.07f, 0.98f);
    private static readonly Color BG_CARD = new Color(0.12f, 0.10f, 0.09f, 0.95f);
    private static readonly Color BG_SECTION = new Color(0.14f, 0.12f, 0.10f, 0.90f);
    private static readonly Color ACCENT_GOLD = new Color(0.85f, 0.70f, 0.30f);
    private static readonly Color ACCENT_GOLD_DIM = new Color(0.65f, 0.50f, 0.20f);
    private static readonly Color ACCENT_LIGHT = new Color(0.90f, 0.85f, 0.75f);
    private static readonly Color BORDER_GOLD = new Color(0.70f, 0.55f, 0.25f, 0.6f);
    private static readonly Color BTN_GREEN = new Color(0.12f, 0.35f, 0.18f);
    private static readonly Color BTN_ORANGE = new Color(0.50f, 0.32f, 0.10f);
    private static readonly Color BTN_BLUE = new Color(0.12f, 0.22f, 0.45f);
    private static readonly Color BTN_RED = new Color(0.45f, 0.12f, 0.12f);
    private static readonly Color BTN_PURPLE = new Color(0.30f, 0.12f, 0.45f);
    private static readonly Color BTN_GRAY = new Color(0.22f, 0.20f, 0.18f);
    private static readonly Color TEXT_DIM = new Color(0.55f, 0.50f, 0.42f);
    private static readonly Color TEXT_SUBTITLE = new Color(0.70f, 0.65f, 0.55f);
    private static readonly Color TAB_ACTIVE = new Color(0.85f, 0.70f, 0.30f);
    private static readonly Color TAB_INACTIVE = new Color(0.45f, 0.40f, 0.35f);
    private static readonly Color TOGGLE_ON = new Color(0.75f, 0.60f, 0.20f);
    private static readonly Color TOGGLE_OFF = new Color(0.30f, 0.28f, 0.25f);
    private static readonly Color SLIDER_BG = new Color(0.20f, 0.18f, 0.15f);
    private static readonly Color SLIDER_FILL = new Color(0.75f, 0.60f, 0.22f);

    void Awake()
    {
        LoadFont();
        CreateCanvas();
        EnsureEventSystem();
    }

    private void LoadFont()
    {
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null)
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (cachedFont == null)
            cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
    }

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
    }

    // ==================== MAIN MENU (TAB-BASED) ====================

    public void ShowMainMenu()
    {
        HideAll();
        currentTab = Tab.Play;
        BuildTabLayout();
    }

    private void BuildTabLayout()
    {
        menuPanel = CreateFullPanel("MainMenu");
        menuPanel.GetComponent<Image>().color = BG_DARK;

        // Content area (above tab bar)
        tabContentArea = new GameObject("TabContent");
        tabContentArea.transform.SetParent(menuPanel.transform, false);
        RectTransform contentRT = tabContentArea.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0.08f);
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        // Tab bar
        BuildTabBar(menuPanel.transform);

        // Build content for current tab
        switch (currentTab)
        {
            case Tab.Play: BuildPlayTab(); break;
            case Tab.Education: BuildEducationTab(); break;
            case Tab.Settings: BuildSettingsTab(); break;
        }
    }

    private void BuildTabBar(Transform parent)
    {
        tabBar = new GameObject("TabBar");
        tabBar.transform.SetParent(parent, false);
        RectTransform barRT = tabBar.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0);
        barRT.anchorMax = new Vector2(1, 0.08f);
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = Vector2.zero;

        Image barBg = tabBar.AddComponent<Image>();
        barBg.color = new Color(0.08f, 0.07f, 0.06f, 1f);

        // Top border line
        GameObject topLine = new GameObject("TabBarTopLine");
        topLine.transform.SetParent(tabBar.transform, false);
        RectTransform lineRT = topLine.AddComponent<RectTransform>();
        lineRT.anchorMin = new Vector2(0, 1);
        lineRT.anchorMax = new Vector2(1, 1);
        lineRT.offsetMin = Vector2.zero;
        lineRT.offsetMax = Vector2.zero;
        lineRT.sizeDelta = new Vector2(0, 2);
        lineRT.pivot = new Vector2(0.5f, 1);
        Image lineImg = topLine.AddComponent<Image>();
        lineImg.color = BORDER_GOLD;

        // Tab buttons
        string[] tabIcons = { "\u265E", "\uD83D\uDCD6", "\u2699" };
        string[] tabLabels = { "ИГРАТЬ", "ОБУЧЕНИЕ", "НАСТРОЙКИ" };
        Tab[] tabs = { Tab.Play, Tab.Education, Tab.Settings };

        for (int i = 0; i < 3; i++)
        {
            float xMin = i / 3f;
            float xMax = (i + 1) / 3f;
            Tab tab = tabs[i];
            bool isActive = (tab == currentTab);

            GameObject tabBtn = new GameObject("Tab_" + tabLabels[i]);
            tabBtn.transform.SetParent(tabBar.transform, false);
            RectTransform tabRT = tabBtn.AddComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(xMin, 0);
            tabRT.anchorMax = new Vector2(xMax, 1);
            tabRT.offsetMin = Vector2.zero;
            tabRT.offsetMax = Vector2.zero;

            Image tabBg = tabBtn.AddComponent<Image>();
            tabBg.color = Color.clear;

            Button btn = tabBtn.AddComponent<Button>();
            Tab capturedTab = tab;
            btn.onClick.AddListener(() => SwitchTab(capturedTab));

            // Icon text
            CreateText(tabIcons[i], tabBtn.transform,
                new Vector2(0, 0.40f), new Vector2(1, 0.85f),
                36, isActive ? TAB_ACTIVE : TAB_INACTIVE,
                TextAnchor.MiddleCenter);

            // Label
            CreateText(tabLabels[i], tabBtn.transform,
                new Vector2(0, 0.02f), new Vector2(1, 0.42f),
                20, isActive ? TAB_ACTIVE : TAB_INACTIVE,
                TextAnchor.MiddleCenter, FontStyle.Bold);

            // Active indicator line
            if (isActive)
            {
                GameObject indicator = new GameObject("ActiveIndicator");
                indicator.transform.SetParent(tabBtn.transform, false);
                RectTransform indRT = indicator.AddComponent<RectTransform>();
                indRT.anchorMin = new Vector2(0.15f, 0.92f);
                indRT.anchorMax = new Vector2(0.85f, 1f);
                indRT.offsetMin = Vector2.zero;
                indRT.offsetMax = Vector2.zero;
                Image indImg = indicator.AddComponent<Image>();
                indImg.color = TAB_ACTIVE;
            }
        }
    }

    private void SwitchTab(Tab tab)
    {
        if (tab == currentTab) return;
        currentTab = tab;
        HideAll();
        BuildTabLayout();
    }

    // ==================== PLAY TAB ====================

    private void BuildPlayTab()
    {
        Transform parent = tabContentArea.transform;

        // Top icons (profile left, trophy right)
        CreateCircleButton("\u263A", parent,
            new Vector2(0.02f, 0.92f), new Vector2(0.10f, 0.97f),
            BORDER_GOLD, ACCENT_GOLD, null);

        CreateCircleButton("\uD83C\uDFC6", parent,
            new Vector2(0.90f, 0.92f), new Vector2(0.98f, 0.97f),
            BORDER_GOLD, ACCENT_GOLD, null);

        // Title: "Caless" with decorative crown
        CreateText("\u2655", parent,
            new Vector2(0.40f, 0.92f), new Vector2(0.60f, 0.98f),
            40, ACCENT_GOLD, TextAnchor.MiddleCenter);

        // Decorative line left of title
        CreateDecorativeLine(parent, new Vector2(0.10f, 0.905f), new Vector2(0.38f, 0.915f));
        CreateDecorativeLine(parent, new Vector2(0.62f, 0.905f), new Vector2(0.90f, 0.915f));

        CreateText("CALESS", parent,
            new Vector2(0.10f, 0.84f), new Vector2(0.90f, 0.93f),
            64, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("СТРАТЕГИЯ. ТАКТИКА. ПОБЕДА.", parent,
            new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.85f),
            24, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        // Decorative board preview (empty 10x10 board)
        BuildDecorativeBoardPreview(parent,
            new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.79f));

        // Mode selection cards
        float cardY1 = 0.05f;
        float cardY2 = 0.28f;

        // Card: VS COMPUTER
        BuildModeCard(parent,
            new Vector2(0.02f, cardY1), new Vector2(0.33f, cardY2),
            "\uD83D\uDCBB", "С КОМПЬЮТЕРОМ", "БРОСЬТЕ ВЫЗОВ\nИСКУССТВЕННОМУ\nИНТЕЛЛЕКТУ",
            new Color(0.08f, 0.18f, 0.12f, 0.95f),
            () => ShowDifficultySelect());

        // Card: VS HUMAN
        BuildModeCard(parent,
            new Vector2(0.345f, cardY1), new Vector2(0.665f, cardY2),
            "\u263A", "С ЧЕЛОВЕКОМ", "ИГРАЙТЕ С ДРУГОМ\nНА ОДНОМ\nУСТРОЙСТВЕ",
            new Color(0.08f, 0.15f, 0.18f, 0.95f),
            () => gameManager.StartLocalMultiplayer());

        // Card: BLUETOOTH
        BuildModeCard(parent,
            new Vector2(0.69f, cardY1), new Vector2(0.98f, cardY2),
            "\u2726", "ЧЕРЕЗ BLUETOOTH", "ПОДКЛЮЧИСЬ И\nИГРАЙ С ДРУГОМ\nРЯДОМ",
            new Color(0.12f, 0.08f, 0.18f, 0.95f),
            () => gameManager.StartBluetoothGame());
    }

    private void BuildModeCard(Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        string icon, string title, string subtitle, Color cardColor, Action onClick)
    {
        // Card container
        GameObject card = new GameObject("Card_" + title);
        card.transform.SetParent(parent, false);
        RectTransform cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = anchorMin;
        cardRT.anchorMax = anchorMax;
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = cardColor;

        Button cardBtn = card.AddComponent<Button>();
        ColorBlock cb = cardBtn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        cardBtn.colors = cb;
        cardBtn.onClick.AddListener(() => onClick?.Invoke());

        // Card border
        GameObject border = new GameObject("Border");
        border.transform.SetParent(card.transform, false);
        RectTransform borderRT = border.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;
        Outline borderOutline = border.AddComponent<Outline>();
        borderOutline.effectColor = BORDER_GOLD;
        borderOutline.effectDistance = new Vector2(2, 2);

        // Icon
        CreateText(icon, card.transform,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.90f),
            42, ACCENT_GOLD, TextAnchor.MiddleCenter);

        // Title
        CreateText(title, card.transform,
            new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.55f),
            16, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);

        // Subtitle
        CreateText(subtitle, card.transform,
            new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.35f),
            12, TEXT_DIM, TextAnchor.MiddleCenter);

        // Decorative diamond
        CreateText("\u2666", card.transform,
            new Vector2(0.40f, 0.00f), new Vector2(0.60f, 0.08f),
            12, ACCENT_GOLD_DIM, TextAnchor.MiddleCenter);
    }

    private void BuildDecorativeBoardPreview(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject boardFrame = new GameObject("BoardFrame");
        boardFrame.transform.SetParent(parent, false);
        RectTransform frameRT = boardFrame.AddComponent<RectTransform>();
        frameRT.anchorMin = anchorMin;
        frameRT.anchorMax = anchorMax;
        frameRT.offsetMin = Vector2.zero;
        frameRT.offsetMax = Vector2.zero;

        Image frameBg = boardFrame.AddComponent<Image>();
        frameBg.color = new Color(0.15f, 0.12f, 0.08f, 0.9f);

        // Gold border
        Outline frameOutline = boardFrame.AddComponent<Outline>();
        frameOutline.effectColor = BORDER_GOLD;
        frameOutline.effectDistance = new Vector2(3, 3);

        // Board grid inside frame
        GameObject boardGrid = new GameObject("BoardGrid");
        boardGrid.transform.SetParent(boardFrame.transform, false);
        RectTransform gridRT = boardGrid.AddComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.06f, 0.04f);
        gridRT.anchorMax = new Vector2(0.94f, 0.96f);
        gridRT.offsetMin = Vector2.zero;
        gridRT.offsetMax = Vector2.zero;

        SettingsManager s = (gameManager != null) ? gameManager.settings : null;
        Color darkSq = (s != null) ? s.GetDarkSquareColor() : new Color(0.22f, 0.18f, 0.15f);
        Color lightSq = (s != null) ? s.GetLightSquareColor() : new Color(0.45f, 0.38f, 0.30f);

        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                float xMin = c / 10f;
                float xMax = (c + 1) / 10f;
                float yMin = r / 10f;
                float yMax = (r + 1) / 10f;

                bool isLight = (r + c) % 2 == 0;

                GameObject cell = new GameObject("Cell_" + r + "_" + c);
                cell.transform.SetParent(boardGrid.transform, false);
                RectTransform cellRT = cell.AddComponent<RectTransform>();
                cellRT.anchorMin = new Vector2(xMin, yMin);
                cellRT.anchorMax = new Vector2(xMax, yMax);
                cellRT.offsetMin = Vector2.zero;
                cellRT.offsetMax = Vector2.zero;

                Image cellImg = cell.AddComponent<Image>();
                cellImg.color = isLight ? lightSq : darkSq;
                cellImg.raycastTarget = false;
            }
        }

        // Row labels (1-10)
        for (int i = 0; i < 10; i++)
        {
            float yCenter = 0.04f + (0.92f * (i + 0.5f) / 10f);
            CreateText((i + 1).ToString(), boardFrame.transform,
                new Vector2(0.00f, yCenter - 0.03f), new Vector2(0.06f, yCenter + 0.03f),
                16, TEXT_DIM, TextAnchor.MiddleCenter);
        }

        // Column labels (A-J)
        string[] cols = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
        for (int i = 0; i < 10; i++)
        {
            float xCenter = 0.06f + (0.88f * (i + 0.5f) / 10f);
            CreateText(cols[i], boardFrame.transform,
                new Vector2(xCenter - 0.03f, 0.00f), new Vector2(xCenter + 0.03f, 0.04f),
                16, TEXT_DIM, TextAnchor.MiddleCenter);
        }
    }

    // ==================== EDUCATION TAB ====================

    private void BuildEducationTab()
    {
        Transform parent = tabContentArea.transform;

        // Back button (top-left)
        CreateRoundButton("\u2190", parent,
            new Vector2(0.02f, 0.93f), new Vector2(0.10f, 0.98f),
            BTN_GRAY, ACCENT_LIGHT, () => SwitchTab(Tab.Play));

        // Title
        CreateText("Caless", parent,
            new Vector2(0.15f, 0.92f), new Vector2(0.85f, 0.99f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        // Decorative diamonds
        CreateText("\u2666  \u2666", parent,
            new Vector2(0.35f, 0.905f), new Vector2(0.65f, 0.925f),
            14, ACCENT_GOLD_DIM, TextAnchor.MiddleCenter);

        // Section header
        CreateText("\u2666  ОБУЧЕНИЕ  \u2666", parent,
            new Vector2(0.15f, 0.86f), new Vector2(0.85f, 0.91f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("Освойте игру шаг за шагом", parent,
            new Vector2(0.15f, 0.83f), new Vector2(0.85f, 0.87f),
            22, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        // Scrollable lesson list
        GameObject scrollArea = CreateScrollView(parent,
            new Vector2(0.03f, 0.01f), new Vector2(0.97f, 0.82f));
        Transform content = scrollArea.transform.Find("Viewport/Content");

        // Lesson cards data
        string[] lessonNumbers = { "1.", "2.", "3.", "4.", "5.", "6." };
        string[] lessonTitles = { "ОСНОВЫ", "ТАКТИКА", "СТРАТЕГИЯ", "МИТТЕЛЬШПИЛЬ", "ЭНДШПИЛЬ", "ПРАКТИКА" };
        string[] lessonDescs = {
            "Изучите доску, фигуры и их\nбазовые ходы.",
            "Узнайте о тактических приёмах\nи комбинациях.",
            "Понимайте планы, позиции\nи ключевые принципы.",
            "Научитесь планировать и находить\nлучшие продолжения.",
            "Освойте техники завершения\nпартии и выигрыша.",
            "Закрепите знания на\nпрактических примерах и задачах."
        };
        string[] lessonIcons = { "\u265E", "\u2694", "\u265C", "\u265E", "\u265A", "\u25CE" };

        float cardHeight = 160f;
        float cardSpacing = 16f;

        for (int i = 0; i < lessonTitles.Length; i++)
        {
            BuildLessonCard(content, i, cardHeight, cardSpacing,
                lessonNumbers[i], lessonTitles[i], lessonDescs[i], lessonIcons[i]);
        }

        // Set content size
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(0, lessonTitles.Length * (cardHeight + cardSpacing) + 20);
    }

    private void BuildLessonCard(Transform parent, int index, float cardHeight, float spacing,
        string number, string title, string description, string icon)
    {
        float yOffset = -(index * (cardHeight + spacing) + 10);

        GameObject card = new GameObject("Lesson_" + title);
        card.transform.SetParent(parent, false);
        RectTransform cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0, 1);
        cardRT.anchorMax = new Vector2(1, 1);
        cardRT.pivot = new Vector2(0.5f, 1);
        cardRT.offsetMin = new Vector2(10, 0);
        cardRT.offsetMax = new Vector2(-10, 0);
        cardRT.anchoredPosition = new Vector2(0, yOffset);
        cardRT.sizeDelta = new Vector2(cardRT.sizeDelta.x, cardHeight);

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = BG_CARD;

        Outline cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = BORDER_GOLD;
        cardOutline.effectDistance = new Vector2(2, 2);

        // Highlight first card
        if (index == 0)
        {
            cardOutline.effectColor = new Color(0.3f, 0.6f, 0.9f, 0.8f);
            cardOutline.effectDistance = new Vector2(3, 3);
        }

        Button cardBtn = card.AddComponent<Button>();
        cardBtn.onClick.AddListener(() => {
            if (gameManager != null)
                gameManager.StartTraining();
        });

        // Icon circle (left side)
        GameObject iconCircle = new GameObject("IconCircle");
        iconCircle.transform.SetParent(card.transform, false);
        RectTransform iconRT = iconCircle.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.02f, 0.15f);
        iconRT.anchorMax = new Vector2(0.16f, 0.85f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        Image iconBg = iconCircle.AddComponent<Image>();
        iconBg.color = new Color(0.18f, 0.15f, 0.12f, 0.9f);

        CreateText(icon, iconCircle.transform,
            new Vector2(0, 0), new Vector2(1, 1),
            38, ACCENT_GOLD, TextAnchor.MiddleCenter);

        // Title
        CreateText(number + " " + title, card.transform,
            new Vector2(0.19f, 0.50f), new Vector2(0.75f, 0.88f),
            30, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);

        // Description
        CreateText(description, card.transform,
            new Vector2(0.19f, 0.08f), new Vector2(0.75f, 0.52f),
            20, TEXT_DIM, TextAnchor.UpperLeft);

        // Arrow right
        CreateText("\u276F", card.transform,
            new Vector2(0.88f, 0.30f), new Vector2(0.98f, 0.70f),
            32, TEXT_DIM, TextAnchor.MiddleCenter);
    }

    // ==================== SETTINGS TAB ====================

    private void BuildSettingsTab()
    {
        Transform parent = tabContentArea.transform;

        // Title
        CreateText("Caless", parent,
            new Vector2(0.15f, 0.93f), new Vector2(0.85f, 0.99f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        // Section header
        CreateText("\u2666  НАСТРОЙКИ  \u2666", parent,
            new Vector2(0.10f, 0.87f), new Vector2(0.90f, 0.93f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("Настройте игру под себя", parent,
            new Vector2(0.15f, 0.84f), new Vector2(0.85f, 0.88f),
            22, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        // Scrollable settings
        GameObject scrollArea = CreateScrollView(parent,
            new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.83f));
        Transform content = scrollArea.transform.Find("Viewport/Content");

        SettingsManager s = (gameManager != null) ? gameManager.settings : null;
        float yPos = -16f;

        // ---- SECTION: APPEARANCE ----
        yPos = BuildSectionHeader(content, "ВНЕШНИЙ ВИД", yPos);

        yPos = BuildSettingRowWithOptions(content, "\uD83C\uDFA8  Тема интерфейса", yPos,
            SettingsManager.BoardThemeNames, s != null ? s.BoardTheme : 0,
            (idx) => { if (s != null) { s.BoardTheme = idx; s.Save(); } });

        yPos = BuildSettingRowWithColorSwatches(content, "\u2B1C  Стиль доски", yPos,
            new Color[] {
                new Color(0.45f, 0.38f, 0.30f),
                new Color(0.93f, 0.87f, 0.78f),
                new Color(0.18f, 0.30f, 0.15f),
                new Color(0.25f, 0.25f, 0.30f)
            }, s != null ? s.BoardTheme : 0,
            (idx) => { if (s != null) { s.BoardTheme = idx; s.Save(); } });

        yPos = BuildSettingRowWithColorSwatches(content, "\u2600  Цвет подсветки", yPos,
            new Color[] {
                ACCENT_GOLD,
                new Color(0.3f, 0.5f, 0.9f),
                new Color(0.2f, 0.7f, 0.3f),
                new Color(0.6f, 0.3f, 0.7f)
            }, s != null ? s.HighlightColor : 0,
            (idx) => { if (s != null) { s.HighlightColor = idx; s.Save(); } });

        yPos -= 20f;

        // ---- SECTION: SOUND ----
        yPos = BuildSectionHeader(content, "ЗВУК", yPos);

        yPos = BuildSliderRow(content, "\uD83C\uDFB5  Музыка", yPos,
            s != null ? s.MusicVolume : 0.7f,
            (val) => { if (s != null) { s.MusicVolume = val; s.Save(); } });

        yPos = BuildSliderRow(content, "\uD83D\uDD0A  Звуковые эффекты", yPos,
            s != null ? s.SoundVolume : 0.8f,
            (val) => { if (s != null) { s.SoundVolume = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u265E  Звук перемещения", yPos,
            s != null ? s.MoveSoundEnabled : true,
            (val) => { if (s != null) { s.MoveSoundEnabled = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u2694  Звук захвата", yPos,
            s != null ? s.CaptureSoundEnabled : true,
            (val) => { if (s != null) { s.CaptureSoundEnabled = val; s.Save(); } });

        yPos -= 20f;

        // ---- SECTION: GAME ----
        yPos = BuildSectionHeader(content, "ИГРА", yPos);

        yPos = BuildToggleRow(content, "\uD83D\uDCA1  Подсказки", yPos,
            s != null ? s.HintsEnabled : false,
            (val) => { if (s != null) { s.HintsEnabled = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u2699  Показывать возможные ходы", yPos,
            s != null ? s.ShowPossibleMoves : true,
            (val) => { if (s != null) { s.ShowPossibleMoves = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\uD83D\uDEE1  Автосохранение", yPos,
            s != null ? s.AutoSaveEnabled : true,
            (val) => { if (s != null) { s.AutoSaveEnabled = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u2714  Подтверждение хода", yPos,
            s != null ? s.MoveConfirmation : false,
            (val) => { if (s != null) { s.MoveConfirmation = val; s.Save(); } });

        yPos = BuildDropdownRow(content, "\uD83D\uDCCA  Сложность по умолчанию", yPos,
            new string[] { "Лёгкая", "Средняя" },
            s != null ? s.DefaultDifficulty : 1,
            (idx) => { if (s != null) { s.DefaultDifficulty = idx; s.Save(); } });

        yPos = BuildDropdownRow(content, "\u23F0  Время на ход по умолчанию", yPos,
            new string[] { "5 мин", "10 мин", "15 мин", "30 мин" },
            s != null ? s.DefaultMoveTime : 1,
            (idx) => { if (s != null) { s.DefaultMoveTime = idx; s.Save(); } });

        yPos -= 20f;

        // ---- SECTION: OTHER ----
        yPos = BuildSectionHeader(content, "ДРУГОЕ", yPos);

        yPos = BuildDropdownRow(content, "\uD83C\uDF10  Язык", yPos,
            SettingsManager.LanguageNames,
            s != null ? s.Language : 0,
            (idx) => { if (s != null) { s.Language = idx; s.Save(); } });

        yPos = BuildNavigationRow(content, "\uD83D\uDCCA  Статистика", yPos, null);

        yPos = BuildNavigationRow(content, "\u21BA  Сбросить настройки", yPos, () => {
            if (s != null)
            {
                PlayerPrefs.DeleteAll();
                s.Load();
                HideAll();
                BuildTabLayout();
            }
        });

        yPos = BuildNavigationRow(content, "\uD83C\uDFA7  Поддержка", yPos, null);

        yPos = BuildNavigationRow(content, "\u24D8  О приложении", yPos, () => {
            ShowDialog("О приложении",
                "Caless v1.0\n\nСтратегическая настольная игра\nс 12 уникальными фигурами\nна доске 10\u00D710.\n\n\u00A9 2025",
                "Закрыть", () => { HideAll(); BuildTabLayout(); });
        });

        yPos -= 40f;

        // Set content size
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(0, Mathf.Abs(yPos) + 20);
    }

    // ==================== SETTINGS BUILDING HELPERS ====================

    private float BuildSectionHeader(Transform parent, string title, float yPos)
    {
        float height = 50f;
        GameObject header = new GameObject("Section_" + title);
        header.transform.SetParent(parent, false);
        RectTransform rt = header.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(0, height);
        rt.offsetMin = new Vector2(20, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-20, rt.offsetMax.y);

        CreateText(title, header.transform,
            new Vector2(0, 0), new Vector2(1, 1),
            24, TEXT_DIM, TextAnchor.MiddleLeft, FontStyle.Bold);

        return yPos - height;
    }

    private float BuildToggleRow(Transform content, string label, float yPos, bool value, Action<bool> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        // Toggle button
        GameObject toggle = new GameObject("Toggle");
        toggle.transform.SetParent(row.transform, false);
        RectTransform toggleRT = toggle.AddComponent<RectTransform>();
        toggleRT.anchorMin = new Vector2(0.82f, 0.25f);
        toggleRT.anchorMax = new Vector2(0.97f, 0.75f);
        toggleRT.offsetMin = Vector2.zero;
        toggleRT.offsetMax = Vector2.zero;

        Image toggleBg = toggle.AddComponent<Image>();
        toggleBg.color = value ? TOGGLE_ON : TOGGLE_OFF;

        Button toggleBtn = toggle.AddComponent<Button>();
        bool currentVal = value;
        toggleBtn.onClick.AddListener(() => {
            onChange?.Invoke(!currentVal);
            HideAll();
            currentTab = Tab.Settings;
            BuildTabLayout();
        });

        // Toggle knob
        GameObject knob = new GameObject("Knob");
        knob.transform.SetParent(toggle.transform, false);
        RectTransform knobRT = knob.AddComponent<RectTransform>();
        if (value)
        {
            knobRT.anchorMin = new Vector2(0.55f, 0.10f);
            knobRT.anchorMax = new Vector2(0.95f, 0.90f);
        }
        else
        {
            knobRT.anchorMin = new Vector2(0.05f, 0.10f);
            knobRT.anchorMax = new Vector2(0.45f, 0.90f);
        }
        knobRT.offsetMin = Vector2.zero;
        knobRT.offsetMax = Vector2.zero;

        Image knobImg = knob.AddComponent<Image>();
        knobImg.color = value ? Color.white : new Color(0.5f, 0.48f, 0.45f);

        return yPos - height - 4;
    }

    private float BuildSliderRow(Transform content, string label, float yPos, float value, Action<float> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        // Value text
        int percent = Mathf.RoundToInt(value * 100f);
        CreateText(percent + "%", row.transform,
            new Vector2(0.85f, 0.50f), new Vector2(0.97f, 0.90f),
            22, ACCENT_GOLD, TextAnchor.MiddleRight);

        // Slider background
        GameObject sliderBg = new GameObject("SliderBg");
        sliderBg.transform.SetParent(row.transform, false);
        RectTransform sliderBgRT = sliderBg.AddComponent<RectTransform>();
        sliderBgRT.anchorMin = new Vector2(0.40f, 0.15f);
        sliderBgRT.anchorMax = new Vector2(0.83f, 0.45f);
        sliderBgRT.offsetMin = Vector2.zero;
        sliderBgRT.offsetMax = Vector2.zero;

        Image sliderBgImg = sliderBg.AddComponent<Image>();
        sliderBgImg.color = SLIDER_BG;

        // Slider fill
        GameObject sliderFill = new GameObject("SliderFill");
        sliderFill.transform.SetParent(sliderBg.transform, false);
        RectTransform fillRT = sliderFill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(value, 1);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        Image fillImg = sliderFill.AddComponent<Image>();
        fillImg.color = SLIDER_FILL;

        // Slider knob
        GameObject sliderKnob = new GameObject("SliderKnob");
        sliderKnob.transform.SetParent(sliderBg.transform, false);
        RectTransform knobRT = sliderKnob.AddComponent<RectTransform>();
        knobRT.anchorMin = new Vector2(value - 0.03f, -0.3f);
        knobRT.anchorMax = new Vector2(value + 0.03f, 1.3f);
        knobRT.offsetMin = Vector2.zero;
        knobRT.offsetMax = Vector2.zero;

        Image knobImg = sliderKnob.AddComponent<Image>();
        knobImg.color = ACCENT_LIGHT;

        // Slider interaction (+ and - buttons)
        CreateSmallButton("-", row.transform,
            new Vector2(0.36f, 0.15f), new Vector2(0.40f, 0.45f),
            BTN_GRAY, ACCENT_LIGHT, () => {
                float newVal = Mathf.Clamp01(value - 0.1f);
                onChange?.Invoke(newVal);
                HideAll();
                currentTab = Tab.Settings;
                BuildTabLayout();
            });

        CreateSmallButton("+", row.transform,
            new Vector2(0.83f, 0.15f), new Vector2(0.87f, 0.45f),
            BTN_GRAY, ACCENT_LIGHT, () => {
                float newVal = Mathf.Clamp01(value + 0.1f);
                onChange?.Invoke(newVal);
                HideAll();
                currentTab = Tab.Settings;
                BuildTabLayout();
            });

        return yPos - height - 4;
    }

    private float BuildSettingRowWithOptions(Transform content, string label, float yPos,
        string[] options, int selected, Action<int> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        float btnWidth = 0.55f / options.Length;
        for (int i = 0; i < options.Length; i++)
        {
            float xMin = 0.42f + i * btnWidth;
            float xMax = xMin + btnWidth - 0.01f;
            int idx = i;
            bool isSelected = (i == selected);

            Color bgCol = isSelected ? ACCENT_GOLD_DIM : BTN_GRAY;
            Color txtCol = isSelected ? Color.white : TEXT_DIM;

            CreateSmallButton(options[i], row.transform,
                new Vector2(xMin, 0.20f), new Vector2(xMax, 0.80f),
                bgCol, txtCol, () => {
                    onChange?.Invoke(idx);
                    HideAll();
                    currentTab = Tab.Settings;
                    BuildTabLayout();
                });
        }

        return yPos - height - 4;
    }

    private float BuildSettingRowWithColorSwatches(Transform content, string label, float yPos,
        Color[] colors, int selected, Action<int> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        float swatchWidth = 0.10f;
        float startX = 0.55f;

        for (int i = 0; i < colors.Length; i++)
        {
            float xMin = startX + i * (swatchWidth + 0.02f);
            float xMax = xMin + swatchWidth;
            int idx = i;
            bool isSelected = (i == selected);

            GameObject swatch = new GameObject("Swatch_" + i);
            swatch.transform.SetParent(row.transform, false);
            RectTransform swRT = swatch.AddComponent<RectTransform>();
            swRT.anchorMin = new Vector2(xMin, 0.20f);
            swRT.anchorMax = new Vector2(xMax, 0.80f);
            swRT.offsetMin = Vector2.zero;
            swRT.offsetMax = Vector2.zero;

            Image swImg = swatch.AddComponent<Image>();
            swImg.color = colors[i];

            if (isSelected)
            {
                Outline swOutline = swatch.AddComponent<Outline>();
                swOutline.effectColor = Color.white;
                swOutline.effectDistance = new Vector2(2, 2);

                CreateText("\u2714", swatch.transform,
                    new Vector2(0, 0), new Vector2(1, 1),
                    20, Color.white, TextAnchor.MiddleCenter);
            }

            Button swBtn = swatch.AddComponent<Button>();
            swBtn.onClick.AddListener(() => {
                onChange?.Invoke(idx);
                HideAll();
                currentTab = Tab.Settings;
                BuildTabLayout();
            });
        }

        return yPos - height - 4;
    }

    private float BuildDropdownRow(Transform content, string label, float yPos,
        string[] options, int selected, Action<int> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        string displayText = (selected >= 0 && selected < options.Length) ? options[selected] : "—";

        GameObject dropBtn = new GameObject("DropdownBtn");
        dropBtn.transform.SetParent(row.transform, false);
        RectTransform dropRT = dropBtn.AddComponent<RectTransform>();
        dropRT.anchorMin = new Vector2(0.62f, 0.20f);
        dropRT.anchorMax = new Vector2(0.97f, 0.80f);
        dropRT.offsetMin = Vector2.zero;
        dropRT.offsetMax = Vector2.zero;

        Image dropBg = dropBtn.AddComponent<Image>();
        dropBg.color = Color.clear;

        Button btn = dropBtn.AddComponent<Button>();
        int nextIdx = (selected + 1) % options.Length;
        btn.onClick.AddListener(() => {
            onChange?.Invoke(nextIdx);
            HideAll();
            currentTab = Tab.Settings;
            BuildTabLayout();
        });

        CreateText(displayText + "  \u25BE", dropBtn.transform,
            new Vector2(0, 0), new Vector2(1, 1),
            22, ACCENT_GOLD, TextAnchor.MiddleRight);

        return yPos - height - 4;
    }

    private float BuildNavigationRow(Transform content, string label, float yPos, Action onClick)
    {
        float height = 70f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        if (onClick != null)
        {
            Button btn = row.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
        }

        // Arrow
        CreateText("\u276F", row.transform,
            new Vector2(0.90f, 0.20f), new Vector2(0.98f, 0.80f),
            24, TEXT_DIM, TextAnchor.MiddleCenter);

        return yPos - height - 4;
    }

    private GameObject CreateSettingRow(Transform parent, string label, float yPos, float height)
    {
        GameObject row = new GameObject("Row_" + label);
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(0, height);
        rt.offsetMin = new Vector2(16, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-16, rt.offsetMax.y);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = BG_SECTION;

        // Label
        CreateText(label, row.transform,
            new Vector2(0.03f, 0.10f), new Vector2(0.60f, 0.90f),
            22, ACCENT_LIGHT, TextAnchor.MiddleLeft);

        return row;
    }

    // ==================== PLAY SUB-MENU ====================

    public void ShowPlaySubMenu()
    {
        HideAll();

        playSubMenu = CreateFullPanel("PlaySubMenu");
        playSubMenu.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", playSubMenu.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  РЕЖИМ ИГРЫ  \u2666", playSubMenu.transform,
            new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.88f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Против компьютера", "\uD83D\uDCBB", playSubMenu.transform,
            new Vector2(0.08f, 0.60f), new Vector2(0.92f, 0.74f),
            BTN_GREEN, () => ShowDifficultySelect());

        CreatePremiumButton("2 игрока (локально)", "\u263A", playSubMenu.transform,
            new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.59f),
            BTN_BLUE, () => gameManager.StartLocalMultiplayer());

        CreatePremiumButton("Bluetooth мультиплеер", "\u2726", playSubMenu.transform,
            new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.44f),
            BTN_PURPLE, () => gameManager.StartBluetoothGame());

        CreatePremiumButton("Назад", "\u2190", playSubMenu.transform,
            new Vector2(0.20f, 0.12f), new Vector2(0.80f, 0.24f),
            BTN_GRAY, () => ShowMainMenu());
    }

    // ==================== DIFFICULTY SELECT ====================

    private void ShowDifficultySelect()
    {
        HideAll();

        difficultyPanel = CreateFullPanel("DifficultySelect");
        difficultyPanel.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", difficultyPanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  СЛОЖНОСТЬ  \u2666", difficultyPanel.transform,
            new Vector2(0.10f, 0.76f), new Vector2(0.90f, 0.86f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Лёгкий", "\u2605", difficultyPanel.transform,
            new Vector2(0.10f, 0.55f), new Vector2(0.90f, 0.69f),
            BTN_GREEN, () => ShowSideSelection(CalessAI.Difficulty.Easy));

        CreatePremiumButton("Средний", "\u2605\u2605", difficultyPanel.transform,
            new Vector2(0.10f, 0.38f), new Vector2(0.90f, 0.52f),
            BTN_ORANGE, () => ShowSideSelection(CalessAI.Difficulty.Medium));

        CreatePremiumButton("Назад", "\u2190", difficultyPanel.transform,
            new Vector2(0.20f, 0.18f), new Vector2(0.80f, 0.30f),
            BTN_GRAY, () => ShowPlaySubMenu());
    }

    // ==================== SIDE SELECTION ====================

    private void ShowSideSelection(CalessAI.Difficulty difficulty)
    {
        HideAll();

        sidePanel = CreateFullPanel("SideSelect");
        sidePanel.GetComponent<Image>().color = BG_DARK;

        string diffName = difficulty == CalessAI.Difficulty.Easy ? "Лёгкий" : "Средний";

        CreateText("Caless", sidePanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("Сложность: " + diffName, sidePanel.transform,
            new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.88f),
            28, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        CreateText("\u2666  ВЫБЕРИТЕ СТОРОНУ  \u2666", sidePanel.transform,
            new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.80f),
            34, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Белые", "\u2654", sidePanel.transform,
            new Vector2(0.10f, 0.50f), new Vector2(0.90f, 0.64f),
            new Color(0.60f, 0.58f, 0.52f),
            () => gameManager.StartGame(difficulty, GameManager.PlayerSide.White));

        CreatePremiumButton("Чёрные", "\u265A", sidePanel.transform,
            new Vector2(0.10f, 0.33f), new Vector2(0.90f, 0.47f),
            new Color(0.12f, 0.10f, 0.08f),
            () => gameManager.StartGame(difficulty, GameManager.PlayerSide.Black));

        CreatePremiumButton("Назад", "\u2190", sidePanel.transform,
            new Vector2(0.20f, 0.14f), new Vector2(0.80f, 0.26f),
            BTN_GRAY, () => ShowDifficultySelect());
    }

    // ==================== SETTINGS (LEGACY) ====================

    public void ShowSettings()
    {
        HideAll();
        currentTab = Tab.Settings;
        BuildTabLayout();
    }

    // ==================== GAME UI ====================

    public void ShowGameUI()
    {
        HideAll();

        gameUIPanel = CreatePanel("GameUI", canvas.transform);
        RectTransform rt = gameUIPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.18f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        gameUIPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.96f);

        // Top border
        GameObject topBorder = new GameObject("GameUIBorder");
        topBorder.transform.SetParent(gameUIPanel.transform, false);
        RectTransform borderRT = topBorder.AddComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0, 1);
        borderRT.anchorMax = new Vector2(1, 1);
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;
        borderRT.sizeDelta = new Vector2(0, 2);
        borderRT.pivot = new Vector2(0.5f, 1);
        Image borderImg = topBorder.AddComponent<Image>();
        borderImg.color = BORDER_GOLD;

        // Turn text
        turnText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.78f), new Vector2(0.60f, 0.96f),
            30, Color.white, TextAnchor.MiddleLeft).GetComponent<Text>();

        // Move count
        moveCountText = CreateText("", gameUIPanel.transform,
            new Vector2(0.62f, 0.78f), new Vector2(0.98f, 0.96f),
            26, TEXT_DIM, TextAnchor.MiddleRight).GetComponent<Text>();

        // Captures
        captureText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.76f),
            22, ACCENT_GOLD, TextAnchor.MiddleLeft).GetComponent<Text>();

        // Status (Kal, special moves)
        statusText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.55f),
            28, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter).GetComponent<Text>();

        // Thinking...
        thinkingText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.55f),
            26, new Color(0.5f, 0.8f, 1f), TextAnchor.MiddleCenter).GetComponent<Text>();

        // Buttons
        float btnY1 = 0.02f, btnY2 = 0.30f;

        CreateGameButton("Меню", gameUIPanel.transform,
            new Vector2(0.01f, btnY1), new Vector2(0.19f, btnY2),
            BTN_RED, () => gameManager.ShowMenu());

        CreateGameButton("Заново", gameUIPanel.transform,
            new Vector2(0.21f, btnY1), new Vector2(0.39f, btnY2),
            BTN_GREEN, () => gameManager.RestartGame());

        CreateGameButton("Повернуть", gameUIPanel.transform,
            new Vector2(0.41f, btnY1), new Vector2(0.59f, btnY2),
            BTN_BLUE, () => gameManager.boardRenderer.FlipBoard());

        CreateGameButton("Храм", gameUIPanel.transform,
            new Vector2(0.61f, btnY1), new Vector2(0.79f, btnY2),
            BTN_PURPLE, () => gameManager.ActivateTemple());

        CreateGameButton("Сдвиг", gameUIPanel.transform,
            new Vector2(0.81f, btnY1), new Vector2(0.99f, btnY2),
            BTN_ORANGE, () => gameManager.ActivateCastling());

        UpdateGameInfo();
    }

    public void UpdateGameInfo()
    {
        if (turnText == null) return;

        bool playerTurn = gameManager.isPlayerTurn;
        bool isLocal = gameManager.currentMode == GameManager.GameMode.LocalMultiplayer;

        if (isLocal)
        {
            turnText.text = gameManager.engine.whiteTurn ? "Ход белых" : "Ход чёрных";
            turnText.color = gameManager.engine.whiteTurn ?
                new Color(0.9f, 0.9f, 0.8f) : ACCENT_GOLD;
        }
        else
        {
            turnText.text = playerTurn ? "Ваш ход" : "Ход компьютера";
            turnText.color = playerTurn ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.7f, 0.3f);
        }

        moveCountText.text = "Ход: " + (gameManager.moveCount / 2 + 1);

        string pCap = FormatCaptures(gameManager.playerCapturedPieces);
        string aCap = FormatCaptures(gameManager.aiCapturedPieces);
        captureText.text = "Вы: " + (pCap.Length > 0 ? pCap : "\u2014") +
                           "  |  ИИ: " + (aCap.Length > 0 ? aCap : "\u2014");

        bool whiteCheck = gameManager.engine.IsInCheck(true);
        bool blackCheck = gameManager.engine.IsInCheck(false);
        if (whiteCheck || blackCheck)
            statusText.text = "КАЛ!";
        else
            statusText.text = "";
    }

    public void ShowThinking(bool show)
    {
        if (thinkingText != null)
            thinkingText.text = show ? "Думаю..." : "";
    }

    private string FormatCaptures(List<int> pieces)
    {
        if (pieces.Count == 0) return "";
        string result = "";
        foreach (int p in pieces)
        {
            int type = CalessEngine.PieceType(p);
            if (CalessEngine.PieceNames.ContainsKey(type))
            {
                string name = CalessEngine.PieceNames[type];
                if (name.Length > 2) name = name.Substring(0, 2);
                result += name + " ";
            }
        }
        return result.Trim();
    }

    // ==================== GAME OVER ====================

    public void ShowGameOver(string title, string message)
    {
        if (gameOverPanel != null) Destroy(gameOverPanel);

        gameOverPanel = CreatePanel("GameOver", canvas.transform);
        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.28f);
        rt.anchorMax = new Vector2(0.95f, 0.72f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        gameOverPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.98f);

        Outline panelOutline = gameOverPanel.AddComponent<Outline>();
        panelOutline.effectColor = BORDER_GOLD;
        panelOutline.effectDistance = new Vector2(3, 3);

        CreateText(title, gameOverPanel.transform,
            new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.92f),
            48, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText(message, gameOverPanel.transform,
            new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.68f),
            30, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreatePremiumButton("Заново", "\u21BA", gameOverPanel.transform,
            new Vector2(0.08f, 0.10f), new Vector2(0.48f, 0.32f),
            BTN_GREEN, () => {
                Destroy(gameOverPanel);
                gameManager.RestartGame();
            });

        CreatePremiumButton("Меню", "\u2302", gameOverPanel.transform,
            new Vector2(0.52f, 0.10f), new Vector2(0.92f, 0.32f),
            BTN_GRAY, () => {
                Destroy(gameOverPanel);
                gameManager.ShowMenu();
            });
    }

    // ==================== REVIVE DIALOG ====================

    public void ShowReviveDialog(string pieceName, Action onRevive, Action onSkip)
    {
        ShowTwoButtonDialog("Оживление",
            "Козёл может оживить: " + pieceName + "\nИспользовать способность?",
            "Оживить", onRevive,
            "Пропустить", onSkip);
    }

    // ==================== TRAINING ====================

    public void ShowTrainingUI(Action<int> onPieceSelected, Action onBack, Action onClear,
                                Action onRandomEnemy, Action onNextPuzzle, Action onPrevPuzzle,
                                int puzzleIndex, int totalPuzzles, string puzzleDescription)
    {
        HideAll();

        trainingUIPanel = CreatePanel("TrainingUI", canvas.transform);
        RectTransform rt = trainingUIPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.20f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        trainingUIPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.96f);

        // Top border
        GameObject topBorder = new GameObject("TrainingBorder");
        topBorder.transform.SetParent(trainingUIPanel.transform, false);
        RectTransform borderRT = topBorder.AddComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0, 1);
        borderRT.anchorMax = new Vector2(1, 1);
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;
        borderRT.sizeDelta = new Vector2(0, 2);
        borderRT.pivot = new Vector2(0.5f, 1);
        Image borderImg = topBorder.AddComponent<Image>();
        borderImg.color = BORDER_GOLD;

        // Description
        CreateText(puzzleDescription, trainingUIPanel.transform,
            new Vector2(0.02f, 0.68f), new Vector2(0.98f, 0.96f),
            24, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        // Puzzle number
        CreateText("Задача " + (puzzleIndex + 1) + " / " + totalPuzzles, trainingUIPanel.transform,
            new Vector2(0.02f, 0.52f), new Vector2(0.98f, 0.68f),
            22, ACCENT_GOLD, TextAnchor.MiddleCenter);

        // Buttons
        float btnY1 = 0.04f, btnY2 = 0.48f;

        CreateGameButton("Назад", trainingUIPanel.transform,
            new Vector2(0.01f, btnY1), new Vector2(0.19f, btnY2),
            BTN_RED, onBack);

        CreateGameButton("< Пред", trainingUIPanel.transform,
            new Vector2(0.21f, btnY1), new Vector2(0.39f, btnY2),
            BTN_GRAY, onPrevPuzzle);

        CreateGameButton("След >", trainingUIPanel.transform,
            new Vector2(0.41f, btnY1), new Vector2(0.59f, btnY2),
            BTN_GRAY, onNextPuzzle);

        CreateGameButton("Враги", trainingUIPanel.transform,
            new Vector2(0.61f, btnY1), new Vector2(0.79f, btnY2),
            BTN_ORANGE, onRandomEnemy);

        CreateGameButton("Сброс", trainingUIPanel.transform,
            new Vector2(0.81f, btnY1), new Vector2(0.99f, btnY2),
            BTN_BLUE, onClear);
    }

    // ==================== BLUETOOTH ====================

    public void ShowBluetoothUI(Action onHost, Action onJoin, Action onBack)
    {
        HideAll();

        menuPanel = CreateFullPanel("BluetoothMenu");
        menuPanel.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", menuPanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  BLUETOOTH  \u2666", menuPanel.transform,
            new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.88f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Создать игру (хост)", "\uD83D\uDCE1", menuPanel.transform,
            new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.69f),
            BTN_GREEN, onHost);

        CreatePremiumButton("Присоединиться", "\uD83D\uDD17", menuPanel.transform,
            new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.52f),
            BTN_BLUE, onJoin);

        CreatePremiumButton("Назад", "\u2190", menuPanel.transform,
            new Vector2(0.20f, 0.18f), new Vector2(0.80f, 0.30f),
            BTN_GRAY, onBack);
    }

    // ==================== DIALOGS ====================

    public void ShowDialog(string title, string message, string btnText, Action onConfirm)
    {
        if (dialogPanel != null) Destroy(dialogPanel);

        dialogPanel = CreatePanel("Dialog", canvas.transform);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.28f);
        rt.anchorMax = new Vector2(0.95f, 0.72f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        dialogPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.98f);

        Outline dlgOutline = dialogPanel.AddComponent<Outline>();
        dlgOutline.effectColor = BORDER_GOLD;
        dlgOutline.effectDistance = new Vector2(3, 3);

        CreateText(title, dialogPanel.transform,
            new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.92f),
            38, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText(message, dialogPanel.transform,
            new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.70f),
            26, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreatePremiumButton(btnText, "", dialogPanel.transform,
            new Vector2(0.15f, 0.06f), new Vector2(0.85f, 0.24f),
            BTN_GREEN, () => {
                Destroy(dialogPanel);
                onConfirm?.Invoke();
            });
    }

    public void ShowTwoButtonDialog(string title, string message,
        string btn1Text, Action onBtn1, string btn2Text, Action onBtn2)
    {
        if (dialogPanel != null) Destroy(dialogPanel);

        dialogPanel = CreatePanel("Dialog2", canvas.transform);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.28f);
        rt.anchorMax = new Vector2(0.95f, 0.72f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        dialogPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.98f);

        Outline dlgOutline = dialogPanel.AddComponent<Outline>();
        dlgOutline.effectColor = BORDER_GOLD;
        dlgOutline.effectDistance = new Vector2(3, 3);

        CreateText(title, dialogPanel.transform,
            new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.92f),
            38, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText(message, dialogPanel.transform,
            new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.70f),
            26, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreatePremiumButton(btn1Text, "", dialogPanel.transform,
            new Vector2(0.05f, 0.06f), new Vector2(0.47f, 0.24f),
            BTN_GREEN, () => {
                Destroy(dialogPanel);
                onBtn1?.Invoke();
            });

        CreatePremiumButton(btn2Text, "", dialogPanel.transform,
            new Vector2(0.53f, 0.06f), new Vector2(0.95f, 0.24f),
            BTN_GRAY, () => {
                Destroy(dialogPanel);
                onBtn2?.Invoke();
            });
    }

    // ==================== UTILITIES ====================

    public void HideAll()
    {
        if (menuPanel != null) { Destroy(menuPanel); menuPanel = null; }
        if (playSubMenu != null) { Destroy(playSubMenu); playSubMenu = null; }
        if (settingsPanel != null) { Destroy(settingsPanel); settingsPanel = null; }
        if (gameUIPanel != null) { Destroy(gameUIPanel); gameUIPanel = null; }
        if (gameOverPanel != null) { Destroy(gameOverPanel); gameOverPanel = null; }
        if (trainingUIPanel != null) { Destroy(trainingUIPanel); trainingUIPanel = null; }
        if (difficultyPanel != null) { Destroy(difficultyPanel); difficultyPanel = null; }
        if (sidePanel != null) { Destroy(sidePanel); sidePanel = null; }
        if (dialogPanel != null) { Destroy(dialogPanel); dialogPanel = null; }
        if (tabBar != null) { tabBar = null; }
        if (tabContentArea != null) { tabContentArea = null; }
    }

    private GameObject CreateFullPanel(string name)
    {
        GameObject panel = CreatePanel(name, canvas.transform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        Image img = panel.AddComponent<Image>();
        img.color = BG_PANEL;
        img.raycastTarget = true;

        return panel;
    }

    private GameObject CreateText(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = cachedFont;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;

        return obj;
    }

    private void CreatePremiumButton(string text, string icon, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Action onClick)
    {
        GameObject btnObj = new GameObject("PremBtn_" + text);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = BORDER_GOLD;
        outline.effectDistance = new Vector2(2, 2);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        // Icon + Text
        if (!string.IsNullOrEmpty(icon))
        {
            CreateText(icon, btnObj.transform,
                new Vector2(0.02f, 0.10f), new Vector2(0.15f, 0.90f),
                28, ACCENT_GOLD, TextAnchor.MiddleCenter);

            CreateText(text, btnObj.transform,
                new Vector2(0.15f, 0.10f), new Vector2(0.98f, 0.90f),
                32, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        }
        else
        {
            CreateText(text, btnObj.transform,
                new Vector2(0.02f, 0.10f), new Vector2(0.98f, 0.90f),
                32, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        }
    }

    private void CreateGameButton(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Action onClick)
    {
        GameObject btnObj = new GameObject("GameBtn_" + text);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(BORDER_GOLD.r, BORDER_GOLD.g, BORDER_GOLD.b, 0.4f);
        outline.effectDistance = new Vector2(1, 1);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(4, 2);
        trt.offsetMax = new Vector2(-4, -2);

        Text t = txtObj.AddComponent<Text>();
        t.text = text;
        t.font = cachedFont;
        t.fontSize = 22;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
    }

    private void CreateSmallButton(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Color textColor, Action onClick)
    {
        GameObject btnObj = new GameObject("SmallBtn_" + text);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        CreateText(text, btnObj.transform,
            new Vector2(0, 0), new Vector2(1, 1),
            18, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
    }

    private void CreateCircleButton(string icon, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color borderColor, Color iconColor, Action onClick)
    {
        GameObject circle = new GameObject("CircleBtn");
        circle.transform.SetParent(parent, false);
        RectTransform rt = circle.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = circle.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.08f, 0.07f, 0.8f);

        Outline outline = circle.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(2, 2);

        if (onClick != null)
        {
            Button btn = circle.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());
        }

        CreateText(icon, circle.transform,
            new Vector2(0, 0), new Vector2(1, 1),
            28, iconColor, TextAnchor.MiddleCenter);
    }

    private void CreateRoundButton(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Color textColor, Action onClick)
    {
        GameObject btnObj = new GameObject("RoundBtn_" + text);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = btnObj.AddComponent<Image>();
        bg.color = bgColor;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = BORDER_GOLD;
        outline.effectDistance = new Vector2(1, 1);

        Button btn = btnObj.AddComponent<Button>();
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        CreateText(text, btnObj.transform,
            new Vector2(0, 0), new Vector2(1, 1),
            28, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
    }

    private void CreateDecorativeLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject line = new GameObject("DecorLine");
        line.transform.SetParent(parent, false);
        RectTransform rt = line.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = line.AddComponent<Image>();
        img.color = ACCENT_GOLD_DIM;
        img.raycastTarget = false;
    }

    private GameObject CreateScrollView(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        // ScrollView container
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(parent, false);
        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = anchorMin;
        scrollRT.anchorMax = anchorMax;
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;

        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 30f;

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = Color.clear;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewRT = viewport.AddComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero;
        viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = Vector2.zero;
        viewRT.offsetMax = Vector2.zero;

        Image viewMask = viewport.AddComponent<Image>();
        viewMask.color = Color.white;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = new Vector2(0, 0);
        contentRT.offsetMax = new Vector2(0, 0);
        contentRT.sizeDelta = new Vector2(0, 0);

        scrollRect.content = contentRT;
        scrollRect.viewport = viewRT;

        return scrollObj;
    }

    private void CreateToggleRow(string label, bool currentValue, Transform parent,
        float y, Action<bool> onToggle)
    {
        CreateText(label, parent,
            new Vector2(0.05f, y), new Vector2(0.55f, y + 0.06f),
            28, ACCENT_LIGHT, TextAnchor.MiddleLeft);

        string btnText = currentValue ? "ВКЛ" : "ВЫКЛ";
        Color btnColor = currentValue ? BTN_GREEN : BTN_RED;

        CreateGameButton(btnText, parent,
            new Vector2(0.60f, y), new Vector2(0.95f, y + 0.06f),
            btnColor, () => onToggle(!currentValue));
    }

    private void CreatePieceIcon(Transform parent, int pieceType, bool isWhite, Vector2 position, int size)
    {
        Sprite sprite = PieceSpriteGenerator.GetPieceSprite(pieceType, isWhite, PieceSpritesHolder.Sprites);
        if (sprite == null) return;

        GameObject iconObj = new GameObject("PieceIcon_" + pieceType);
        iconObj.transform.SetParent(parent, false);

        RectTransform rt = iconObj.AddComponent<RectTransform>();
        rt.anchorMin = position;
        rt.anchorMax = new Vector2(position.x + 0.06f, position.y + 0.08f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = iconObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white;
    }
}
