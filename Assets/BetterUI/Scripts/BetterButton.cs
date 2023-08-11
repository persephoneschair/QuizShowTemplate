using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BetterButton : MonoBehaviour
{
    [HideInInspector] public Button button;
    [HideInInspector] public Image border;
    [HideInInspector] public TextMeshProUGUI textMesh;
    public enum ColorScheme { LightRed, LightBlue, LightGreen, LightYellow, DarkRed, DarkBlue, DarkGreen, DarkYellow };
    public enum BorderThickness { Thin, Medium, Thick };
    public enum ButtonStyle { Rounded, Straight, RoundedGradient, StraightGradient };

    #region Definitions

    [Header("Definitions")]
    [HideInInspector] public ColorBlock[] colorBlocks;

    private Sprite[] currentBorderSprites;
    [HideInInspector] public Sprite[] roundedBorderSprites;
    [HideInInspector] public Sprite[] straightBorderSprites;
    [HideInInspector] public Sprite[] buttonStyleSprites;

    #endregion

    #region Public Settings
    [Header("Colors")]
    [OnValueChanged("SetBorderColor")]
    public Color borderColor;

    [OnValueChanged("SetButtonColors")]
    public ColorScheme buttonColorScheme;

    [OnValueChanged("SetButtonTint")]
    public Color buttonTint;

    [OnValueChanged("SetTextColor")]
    public Color textColor;

    [Header("Border Styles")]
    [OnValueChanged("SetBorderThickness")]
    public BorderThickness borderThickness;

    [OnValueChanged("SetStyle")]
    public ButtonStyle buttonStyle;

    [OnValueChanged("SetPixelPerUnitMultiplier")]
    [Range(0, 25)] public float pixelsPerUnitMultiplier = 2f;

    [Header("Transform")]
    [OnValueChanged("SetRectTransform")]
    [Range(0, 1f)] public float xMin = 0.2f;
    [OnValueChanged("SetRectTransform")]
    [Range(0, 1f)] public float xMax = 0.4f, yMin = 0.2f, yMax = 0.4f;

    #endregion

    private void Start()
    {
        SetBorderColor();
        SetButtonColors();
        SetButtonTint();
        SetTextColor();
        SetBorderThickness();
        SetStyle();
        SetPixelPerUnitMultiplier();
        SetRectTransform();
    }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

    public void SetBorderColor()
    {
        border.color = borderColor;
    }

    public void SetButtonColors()
    {
        button.colors = colorBlocks[(int)buttonColorScheme];
    }

    public void SetButtonTint()
    {
        button.GetComponent<Image>().color = buttonTint;
    }

    public void SetTextColor()
    {
        textMesh.color = textColor;
    }

    public void SetBorderThickness()
    {
        currentBorderSprites = (buttonStyle == ButtonStyle.Straight || buttonStyle == ButtonStyle.StraightGradient) ? straightBorderSprites : roundedBorderSprites;
        border.sprite = currentBorderSprites[(int)borderThickness];
    }

    public void SetStyle()
    {
        currentBorderSprites = (buttonStyle == ButtonStyle.Straight || buttonStyle == ButtonStyle.StraightGradient) ? straightBorderSprites : roundedBorderSprites;
        button.GetComponent<Image>().sprite = buttonStyleSprites[(int)buttonStyle];
        SetBorderThickness();
    }

    public void SetPixelPerUnitMultiplier()
    {
        button.GetComponent<Image>().pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
        border.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
    }

    public void SetRectTransform()
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(0,0);
        rt.offsetMax = new Vector2(0,0);
    }

#endif
}
