using System;
using TMPro;
using UnityEngine;

[Serializable]
public class ReactionResultEntry
{
    [Header("Data")]
    public string key;

    [Header("UI")]
    public TMP_Text label;
    public TMP_Text value;
}