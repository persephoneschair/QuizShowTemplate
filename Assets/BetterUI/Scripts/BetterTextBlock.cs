using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BetterTextBlock : MonoBehaviour
{
    [HideInInspector] public Image background;
    [HideInInspector] public Image border;
    [HideInInspector] public TextMeshProUGUI textMesh;
    public enum ColorScheme { LightRed, LightBlue, LightGreen, LightYellow, DarkRed, DarkBlue, DarkGreen, DarkYellow };
    public enum BorderThickness { Thin, Medium, Thick };
    public enum BlockStyle { Rounded, Straight, RoundedGradient, StraightGradient };

    #region Definitions

    [Header("Definitions")]

    private Sprite[] currentBorderSprites;
    [HideInInspector] public Sprite[] roundedBorderSprites;
    [HideInInspector] public Sprite[] straightBorderSprites;
    [HideInInspector] public Sprite[] buttonStyleSprites;

    #endregion

    #region Public Settings
    [Header("Colors")]
    [OnValueChanged("SetBorderColor")]
    public Color borderColor;

    [OnValueChanged("SetBlockColor")]
    public Color blockColorScheme;

    [OnValueChanged("SetTextColor")]
    public Color textColor;

    [Header("Border Styles")]
    [OnValueChanged("SetBorderThickness")]
    public BorderThickness borderThickness;

    [OnValueChanged("SetStyle")]
    public BlockStyle blockStyle;

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
        SetBlockColor();
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

    public void SetBlockColor()
    {
        background.color = blockColorScheme;
    }

    public void SetTextColor()
    {
        textMesh.color = textColor;
    }

    public void SetBorderThickness()
    {
        currentBorderSprites = (blockStyle == BlockStyle.Straight || blockStyle == BlockStyle.StraightGradient) ? straightBorderSprites : roundedBorderSprites;
        border.sprite = currentBorderSprites[(int)borderThickness];
    }

    public void SetStyle()
    {
        currentBorderSprites = (blockStyle == BlockStyle.Straight || blockStyle == BlockStyle.StraightGradient) ? straightBorderSprites : roundedBorderSprites;
        background.sprite = buttonStyleSprites[(int)blockStyle];
        SetBorderThickness();
    }

    public void SetPixelPerUnitMultiplier()
    {
        background.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
        border.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
    }

    public void SetRectTransform()
    {
        RectTransform rt = background.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);
    }

#endif
}
