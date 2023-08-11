using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaderboardStrap : MonoBehaviour
{
    private int _position;
    public int Position
    {
        get { return _position; }
        set
        {
            _position = value;
            SetUpStrap();
        }
    }

    public TextMeshProUGUI posMesh;
    public TextMeshProUGUI playerNameMesh;
    public TextMeshProUGUI totalCorrectMesh;
    public TextMeshProUGUI maxPointsMesh;

    public Image borderRend;
    public Image backgroundRend;

    public Color[] borderCols;
    public Color[] backgroundCols;


    public void SetUpStrap()
    {
        int index = Position > 2 ? 2 : Position;
        borderRend.color = borderCols[index];
        backgroundRend.color = backgroundCols[index];

        posMesh.text = Extensions.AddOrdinal(Position + 1);
    }

    public void PopulateStrap(PlayerObject pl)
    {
        playerNameMesh.text = pl.playerName;
        totalCorrectMesh.text = pl.totalCorrect.ToString();
    }

    public void KillStrap()
    {
        posMesh.text = "";
    }
}
