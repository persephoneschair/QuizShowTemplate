using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsManager : SingletonMonoBehaviour<CreditsManager>
{

    public GameObject endCard;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    [Button]
    public void RollCredits()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(Credits());
    }

    IEnumerator Credits()
    {
        yield return new WaitForSeconds(0f);
        endCard.SetActive(true);
    }
}
