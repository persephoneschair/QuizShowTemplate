using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Podiums : MonoBehaviour
{
    public List<Podium> podia;
    public static Podiums GetPodiums { get; private set; }
    private void Awake()
    {
        if (GetPodiums != null && GetPodiums != this)
            Destroy(this);
        else
            GetPodiums = this;
    }
}
