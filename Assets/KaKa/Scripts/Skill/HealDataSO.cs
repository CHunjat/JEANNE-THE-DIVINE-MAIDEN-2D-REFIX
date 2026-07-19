using UnityEngine;

[CreateAssetMenu(fileName = "HealDataSO", menuName = "Scriptable Objects/HealDataSO")]
public class HealDataSO : ScriptableObject
{
    public string healName;
    public float healAmount = 50f;
    public float healMpCost = 100f;
}
