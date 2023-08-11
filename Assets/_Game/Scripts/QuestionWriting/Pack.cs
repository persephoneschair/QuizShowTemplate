using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Pack
{
    public string author;
    public List<Question> r1Questions = new List<Question>();
    public List<Question> r2Questions = new List<Question>();
    public List<Question> r3Questions = new List<Question>();
    public List<Question> r4Questions = new List<Question>();
}
