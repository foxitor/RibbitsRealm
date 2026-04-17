using System.Collections; using UnityEngine; using System.Collections.Generic;

public class ItemLib : MonoBehaviour {
    public Item[] Items;

    public GameObject DisplayItem(string id) {
        if (id != null) {
            foreach (var item in Items) {
                if (item.id == id)
                    return item.prefab;
            }
        }
        return null;
    }
    public Item ShareRawItem(string id) {
        if (id != null) {
            foreach (var item in Items) {
                if (item.id == id)
                    return item;
            }
        }
        return null;
    }
}
