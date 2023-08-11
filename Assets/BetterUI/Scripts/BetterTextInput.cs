using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BetterTextInput : MonoBehaviour
{
    [HideInInspector] public TMP_InputField inputField;
    [HideInInspector] public TextMeshProUGUI typedMesh;
    [HideInInspector] public TextMeshProUGUI placeholderMesh;
    [HideInInspector] public Image border;

    public enum ColorScheme { LightRed, LightBlue, LightGreen, LightYellow, DarkRed, DarkBlue, DarkGreen, DarkYellow };
    public enum BorderThickness { Thin, Medium, Thick };
    public enum TextFieldStyle { Rounded, Straight, RoundedGradient, StraightGradient };

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

    [OnValueChanged("SetFieldColorScheme")]
    public ColorScheme fieldColorScheme;

    [OnValueChanged("SetFieldTint")]
    public Color fieldTint;

    [OnValueChanged("SetTextColor")]
    public Color typedTextColor;
    [OnValueChanged("SetTextColor")]
    public Color placeholderTextColor;

    [Header("Border Styles")]
    [OnValueChanged("SetBorderThickness")]
    public BorderThickness borderThickness;

    [OnValueChanged("SetStyle")]
    public TextFieldStyle textFieldStyle;

    [OnValueChanged("SetPixelPerUnitMultiplier")]
    [Range(0, 25)] public float pixelsPerUnitMultiplier = 2f;

    [Header("Text")]
    [OnValueChanged("SetText")]
    public string placeholderText;

    [Header("Transform")]
    [OnValueChanged("SetRectTransform")]
    [Range(0, 1f)] public float xMin = 0.2f;
    [OnValueChanged("SetRectTransform")]
    [Range(0, 1f)] public float xMax = 0.4f, yMin = 0.2f, yMax = 0.4f;

    private void Start()
    {
        SetBorderColor();
        SetFieldColorScheme();
        SetFieldTint();
        SetTextColor();
        SetBorderThickness();
        SetStyle();
        SetPixelPerUnitMultiplier();
        SetText();
        SetRectTransform();
    }

    #endregion

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

    public void SetBorderColor()
    {
        border.color = borderColor;
    }

    public void SetFieldColorScheme()
    {
        inputField.colors = colorBlocks[(int)fieldColorScheme];
    }

    public void SetFieldTint()
    {
        inputField.GetComponent<Image>().color = fieldTint;
    }

    public void SetTextColor()
    {
        placeholderMesh.color = placeholderTextColor;
        typedMesh.color = typedTextColor;
    }

    public void SetBorderThickness()
    {
        currentBorderSprites = (textFieldStyle == TextFieldStyle.Straight || textFieldStyle == TextFieldStyle.StraightGradient) ? straightBorderSprites : roundedBorderSprites;
        border.sprite = currentBorderSprites[(int)borderThickness];
    }

    public void SetStyle()
    {
        currentBorderSprites = (textFieldStyle == TextFieldStyle.Straight || textFieldStyle == TextFieldStyle.StraightGradient) ? straightBorderSprites : roundedBorderSprites;
        inputField.gameObject.GetComponent<Image>().sprite = buttonStyleSprites[(int)textFieldStyle];
        SetBorderThickness();
    }

    public void SetPixelPerUnitMultiplier()
    {
        inputField.GetComponent<Image>().pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
        border.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
    }

    public void SetText()
    {
        placeholderMesh.text = placeholderText;
    }

    public void SetRectTransform()
    {
        RectTransform rt = inputField.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);
    }

#endif
}
