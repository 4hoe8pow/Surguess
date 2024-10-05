using UnityEngine;
using HoudiniEngineUnity;

public class HDAAttributeReader : MonoBehaviour
{
    public int GetLargestTotalIndex()
    {
        var attrStore = GetComponentInChildren<HEU_OutputAttributesStore>();
        if (attrStore == null)
        {
            Debug.LogError("HEU_OutputAttributesStore not found!");
            return 0;
        }

        var totals = new float[4];
        for (int i = 1; i <= 4; i++)
        {
            totals[i - 1] = GetAttributeValue(attrStore, $"total_{i}");
        }

        return System.Array.IndexOf(totals, Mathf.Max(totals)) + 1;
    }

    private float GetAttributeValue(HEU_OutputAttributesStore attrStore, string attributeName)
    {
        var attribute = attrStore.GetAttribute(attributeName);
        return attribute?._floatValues?.Length > 0 ? attribute._floatValues[0] : 0f;
    }
}
