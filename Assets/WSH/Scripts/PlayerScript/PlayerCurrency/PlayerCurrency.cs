using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public const int MAX_CURRENCY = 999_999_999;

    [Header("��ȭ ������")]
    public int anima = 0;
    public int gold = 0;

    public void AddAnima(int amount)
    {
        if (amount <= 0)
            return;

        anima = Mathf.Clamp(anima + amount,0,MAX_CURRENCY);
        Debug.Log(
            $"�ƴϸ� ȹ��! (+{amount}) ���� �ƴϸ�: {anima:N0}"
        );
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        gold = Mathf.Clamp(gold + amount, 0, MAX_CURRENCY);

        Debug.Log(
            $"��� ȹ��! (+{amount}) ���� ���: {gold:N0}"
        );
    }

    public bool TryConsumeGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (gold >= amount)
        {
            gold -= amount;
            return true;
        }

        return false;
    }

    public bool TryUpgradeSkill(SkillData targetSkill)
    {
        if (targetSkill == null || targetSkill.growthData == null)
            return false;

        if (targetSkill.currentLevel >=targetSkill.growthData.growthTable.Length)
        {
            Debug.Log($"{targetSkill.skillName} ��ų�� �̹� �ְ� �����Դϴ�.");

            return false;
        }

        int cost =targetSkill.growthData .growthTable[targetSkill.currentLevel - 1].requiredAnima;

        if (anima >= cost)
        {
            anima -= cost;
            targetSkill.currentLevel++;

            Debug.Log( $"{targetSkill.skillName} ������ ����! " + $"(���� Lv.{targetSkill.currentLevel}/" +$"���� �ƴϸ�: {anima:N0})");

            return true;
        }

        Debug.Log( $"�ƴϸ��� �����մϴ�! " +$"(�ʿ�: {cost:N0} / ����: {anima:N0})");
        return false;
    }

#if UNITY_EDITOR //�ν����Ϳ��� �ִ�������� ���� ������ �ִ�������� ����
    private void OnValidate()
    {
        anima = Mathf.Clamp(
            anima,
            0,
            MAX_CURRENCY
        );

        gold = Mathf.Clamp(
            gold,
            0,
            MAX_CURRENCY
        );
    }
#endif
}