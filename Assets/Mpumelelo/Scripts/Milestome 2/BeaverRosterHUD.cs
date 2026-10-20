using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeaverRosterHUD : MonoBehaviour
{
    private const float PanelWidth = 370f;
    private const float PanelHeight = 280f;
    private const float RowHeight = 30f;
    private const float RowsTop = 70f;

    private static readonly Color PanelColor = new Color(0.08f, 0.12f, 0.16f, 0.9f);
    private static readonly Color RowColor = new Color(1f, 1f, 1f, 0.045f);
    private static readonly Color ActiveRowColor = new Color(0.78f, 0.87f, 0.91f, 0.98f);
    private static readonly Color PrimaryTeamColor = new Color(0.37f, 0.68f, 0.88f, 1f);
    private static readonly Color SecondaryTeamColor = new Color(0.96f, 0.61f, 0.34f, 1f);
    private static readonly Color MainTextColor = new Color(0.94f, 0.96f, 0.97f, 1f);
    private static readonly Color ActiveTextColor = new Color(0.09f, 0.16f, 0.19f, 1f);
    private static readonly Color DeadTextColor = new Color(1f, 0.53f, 0.48f, 1f);
    private static readonly Color AliveTextColor = new Color(0.72f, 0.86f, 0.76f, 1f);

    private readonly List<BeaverRow> rows = new List<BeaverRow>();
    private RectTransform panel;
    private TMP_FontAsset font;

    public void Initialize(
        List<BeaverController> player1Beavers,
        List<BeaverController> player2Beavers,
        TMP_FontAsset sourceFont)
    {
        if (sourceFont == null)
        {
            Debug.LogError("BeaverRosterHUD requires a TextMeshPro font from the gameplay HUD.");
            return;
        }

        font = sourceFont;
        rows.Clear();

        if (panel != null)
        {
            Destroy(panel.gameObject);
        }

        BuildPanel(player1Beavers, player2Beavers);
    }

    public void Refresh(
        List<BeaverController> player1Beavers,
        List<BeaverController> player2Beavers,
        BeaverController activeBeaver)
    {
        if (panel == null || rows.Count == 0) return;

        int rowIndex = 0;
        RefreshPlayerRows(player1Beavers, 1, activeBeaver, ref rowIndex);
        RefreshPlayerRows(player2Beavers, 2, activeBeaver, ref rowIndex);
    }

    private void BuildPanel(
        List<BeaverController> player1Beavers,
        List<BeaverController> player2Beavers)
    {
        panel = CreateRectTransform("BeaverRoster", transform, new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f),
            new Vector2(PanelWidth, PanelHeight));
        panel.SetAsLastSibling();

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColor;
        panelImage.raycastTarget = false;

        AddText("Title", "BEAVER STATUS", 14f, 10f, 342f, 28f, 24f,
            MainTextColor, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddText("UnitHeader", "UNIT", 14f, 43f, 140f, 20f, 15f,
            new Color(0.68f, 0.75f, 0.79f, 1f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddText("HealthHeader", "HP", 170f, 43f, 54f, 20f, 15f,
            new Color(0.68f, 0.75f, 0.79f, 1f), TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        AddText("StatusHeader", "STATUS", 245f, 43f, 108f, 20f, 15f,
            new Color(0.68f, 0.75f, 0.79f, 1f), TextAlignmentOptions.MidlineRight, FontStyles.Bold);

        int rowIndex = 0;
        AddPlayerRows(player1Beavers, 1, ref rowIndex);
        AddPlayerRows(player2Beavers, 2, ref rowIndex);
    }

    private void AddPlayerRows(List<BeaverController> beavers, int playerNumber, ref int rowIndex)
    {
        if (beavers == null) return;

        Color teamColor = playerNumber == 1 ? PrimaryTeamColor : SecondaryTeamColor;
        for (int beaverIndex = 0; beaverIndex < beavers.Count; beaverIndex++)
        {
            float top = RowsTop + rowIndex * RowHeight;
            RectTransform rowRect = CreateRectTransform($"Player{playerNumber}Beaver{beaverIndex + 1}",
                panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(10f, -top), new Vector2(350f, 26f));

            Image rowBackground = rowRect.gameObject.AddComponent<Image>();
            rowBackground.color = RowColor;
            rowBackground.raycastTarget = false;

            RectTransform teamAccent = CreateRectTransform("TeamAccent", rowRect,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(4f, 0f));
            Image accentImage = teamAccent.gameObject.AddComponent<Image>();
            accentImage.color = teamColor;
            accentImage.raycastTarget = false;

            TextMeshProUGUI unitText = AddText($"Unit{playerNumber}-{beaverIndex + 1}",
                $"P{playerNumber}  /  BEAVER {beaverIndex + 1}", 12f, top + 2f, 138f, 22f, 17f,
                MainTextColor, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            TextMeshProUGUI healthText = AddText($"Health{playerNumber}-{beaverIndex + 1}",
                "100", 170f, top + 2f, 54f, 22f, 17f,
                MainTextColor, TextAlignmentOptions.MidlineRight, FontStyles.Normal);
            TextMeshProUGUI statusText = AddText($"Status{playerNumber}-{beaverIndex + 1}",
                "ALIVE", 245f, top + 2f, 108f, 22f, 15f,
                AliveTextColor, TextAlignmentOptions.MidlineRight, FontStyles.Bold);

            rows.Add(new BeaverRow
            {
                Background = rowBackground,
                UnitText = unitText,
                HealthText = healthText,
                StatusText = statusText
            });

            rowIndex++;
        }
    }

    private void RefreshPlayerRows(
        List<BeaverController> beavers,
        int playerNumber,
        BeaverController activeBeaver,
        ref int rowIndex)
    {
        if (beavers == null) return;

        for (int beaverIndex = 0; beaverIndex < beavers.Count && rowIndex < rows.Count; beaverIndex++)
        {
            BeaverRow row = rows[rowIndex++];
            BeaverController beaver = beavers[beaverIndex];
            BeaverHealth health = beaver != null ? beaver.GetComponent<BeaverHealth>() : null;
            bool isDead = beaver == null || (health != null && health.IsDead);
            bool isActive = !isDead && beaver == activeBeaver;
            int state = isDead ? 2 : (isActive ? 1 : 0);
            if (row.CachedState != state)
            {
                row.Background.color = isActive ? ActiveRowColor : RowColor;
                row.UnitText.color = isActive ? ActiveTextColor : MainTextColor;
                row.HealthText.color = isActive ? ActiveTextColor : MainTextColor;
                row.StatusText.color = isActive ? ActiveTextColor : (isDead ? DeadTextColor : AliveTextColor);
                row.StatusText.text = isDead ? "DEAD" : (isActive ? "ACTIVE" : "ALIVE");
                row.CachedState = state;
            }

            int healthValue = beaver == null ? 0 : (health != null ? health.CurrentHealth : -1);
            if (healthValue < 0 && !row.MissingHealthWarningLogged)
            {
                Debug.LogWarning($"Beaver {playerNumber}-{beaverIndex + 1} has no BeaverHealth component.");
                row.MissingHealthWarningLogged = true;
            }

            if (row.CachedHealth != healthValue)
            {
                row.HealthText.text = healthValue < 0 ? "N/A" : healthValue.ToString();
                row.CachedHealth = healthValue;
            }
        }
    }

    private TextMeshProUGUI AddText(
        string objectName,
        string value,
        float x,
        float top,
        float width,
        float height,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        FontStyles fontStyle)
    {
        RectTransform textRect = CreateRectTransform(objectName, panel,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(x, -top), new Vector2(width, height));
        TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRectTransform(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private sealed class BeaverRow
    {
        public Image Background;
        public TextMeshProUGUI UnitText;
        public TextMeshProUGUI HealthText;
        public TextMeshProUGUI StatusText;
        public bool MissingHealthWarningLogged;
        public int CachedHealth = int.MinValue;
        public int CachedState = int.MinValue;
    }
}
