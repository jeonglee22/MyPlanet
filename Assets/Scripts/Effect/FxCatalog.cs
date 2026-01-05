using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FX/FxCatalog")]
public class FxCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public FxId id;
        public GameObject prefab;

        [Header("Type")]
        public bool isUI = false;

        [Header("Pool Settings")]
        public int defaultPoolCapacity = 30;
        public int maxPoolSize = 200;
        public bool collectionCheck = true;
    }

    public List<Entry> entries = new List<Entry>();
}
