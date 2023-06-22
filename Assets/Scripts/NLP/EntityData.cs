using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EntityData : ScriptableObject
{
    public SerializableDictionary<string, List<string>> Synset;
    public List<string> Types;

    public List<string> GetGlobalSynset()
    {
        List<string> globalSynset = new List<string>();

        Dictionary<string, List<string>> synsetDict = Synset;

        foreach (string language in synsetDict.Keys)
        {
            List<string> currentLanguageSynset = new List<string>();

            if (synsetDict.TryGetValue(language, out currentLanguageSynset))
            {
                foreach (string syn in currentLanguageSynset)
                {
                    globalSynset.Add(syn);
                }
            }
        }

        return globalSynset;
    }

    public List<string> GetEntityTypes()
    {
        List<string> types = new List<string>();

        foreach (string entityType in Types)
        {
            types.Add(entityType);
        }

        return types;
    }
}
