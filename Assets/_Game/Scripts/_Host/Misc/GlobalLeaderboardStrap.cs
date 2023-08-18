using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitchLib.Client.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaderboardStrap : MonoBehaviour
{
    public Vector3 startPos;
    public PlayerObject containedPlayer;

    public TextMeshProUGUI playerNameMesh;
    public TextMeshProUGUI totalCorrectMesh;
    public RawImage avatarRend;

    public Image borderRend;
    public Image backgroundRend;

    public Color[] borderCols;
    public Color[] backgroundCols;

    private Vector3 targetPosition;
    private float elapsedTime = 0;

    public void SetUpStrap()
    {
        startPos = GetComponent<RectTransform>().localPosition;
        targetPosition = startPos;
        playerNameMesh.text = "";
        totalCorrectMesh.text = "";
        gameObject.SetActive(false);
    }

    public void PopulateStrap(PlayerObject pl, bool isClone)
    {
        //Flicker to life?
        gameObject.SetActive(true);
        containedPlayer = pl;
        playerNameMesh.text = pl.playerName;
        avatarRend.texture = pl.profileImage;
        totalCorrectMesh.text = pl.points.ToString();
        if (!isClone)
            pl.strap = this;
        else
            pl.cloneStrap = this;
    }

    public void SetBackgroundColor(bool hotseat)
    {
        backgroundRend.color = hotseat ? backgroundCols[0] : backgroundCols[1];
        borderRend.color = hotseat ? borderCols[0] : borderCols[1];
        totalCorrectMesh.text = containedPlayer.points.ToString();
    }

    public void SetCorrectOrIncorrectColor(bool correct)
    {
        backgroundRend.color = correct ? backgroundCols[2] : backgroundCols[3];
        borderRend.color = correct ? borderCols[2] : borderCols[3];
        totalCorrectMesh.text = containedPlayer.points.ToString();
    }

    public void SetLockedInColor()
    {
        backgroundRend.color = backgroundCols[4];
        borderRend.color = borderCols[4];
        totalCorrectMesh.text = containedPlayer.points.ToString();
    }

    public void MoveStrap(Vector3 targetPos, int i)
    {
        //playerNameMesh.text = (i + 1).ToString();
        targetPosition = targetPos;
        elapsedTime = 0;
    }

    public void Update()
    {
        LerpStraps();
    }

    private void LerpStraps()
    {
        elapsedTime += Time.deltaTime * 1f;
        float percentageComplete = elapsedTime / LeaderboardManager.Get.reorderDuration;
        this.gameObject.transform.localPosition = Vector3.Lerp(this.gameObject.transform.localPosition, targetPosition, Mathf.SmoothStep(0, 1, percentageComplete));
    }
}
