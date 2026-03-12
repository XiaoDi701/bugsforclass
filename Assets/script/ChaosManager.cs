using System.Collections.Generic;
using UnityEngine;

public class ChaosManager : MonoBehaviour
{
    [Header("Watched Objects")]
    public List<GameObject> chaosObjects;

    [Header("Rules")]
    public List<ChaosRule> rules;

    private int lastCount = -1;

    void Update()
    {
        int count = GetActiveCount();
        if (count == lastCount) return;

        lastCount = count;
        ApplyRules(count);
    }

    int GetActiveCount()
    {
        int n = 0;
        foreach (var obj in chaosObjects)
            if (obj != null && obj.activeSelf) n++;
        return n;
    }

    void ApplyRules(int count)
    {
        foreach (var rule in rules)
        {
            if (rule.activeCount != count) continue;

            foreach (var obj in rule.objectsToEnable)
                if (obj != null) obj.SetActive(true);

            foreach (var obj in rule.objectsToDisable)
                if (obj != null) obj.SetActive(false);
        }
    }
}
