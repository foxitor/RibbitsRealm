using System; using UnityEngine;

[System.Serializable]
public class ItemStack {
    public string id;
    public int count;
    public string components;
}
[System.Serializable]
public class Item {
    public string id;
    public GameObject prefab;
    public Sprite icon;
}