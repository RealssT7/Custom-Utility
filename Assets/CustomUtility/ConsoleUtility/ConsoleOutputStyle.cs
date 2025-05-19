using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "COS_", menuName = "CustomUtility/Console/OutputStyle")]
public class ConsoleOutputStyle : ScriptableObject
{
    public string StyleName;
    public List<string> Keywords = new();
    public Color TextColor = Color.white;
}