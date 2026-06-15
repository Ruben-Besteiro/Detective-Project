using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mystery/Hypothesis")]
public class HypothesisData : ScriptableObject
{
    public List<HypothesisVersion> versions;

    public HypothesisVersion GetCurrentVersion()
    {
        HypothesisVersion current = versions[0];

        foreach (var version in versions)
        {
            bool valid = true;

            foreach (var flag in version.requiredFlags)
            {
                if (!FlagManager.Instance.HasFlag(flag))
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                current = version;
        }

        return current;
    }
}