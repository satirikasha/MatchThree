namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System;

  [Serializable]
  public class Item : MonoBehaviour {

    public const string ITEMS_FOLDER_PATH = "Prefabs/Items/";

    [HideInInspector]
    public ItemType Type;

    [HideInInspector]
    public Cell ParentCell;

    public static Item GetInstance(ItemType type) {
      Item result;
      if(!ItemCache.TryGetValue(type, out result)) {
        result = Resources.Load<Item>(ITEMS_FOLDER_PATH + type.ToString());
        result.Type = type;
        ItemCache.Add(type, result);
      }
      return result;
    }
    private static Dictionary<ItemType, Item> ItemCache = new Dictionary<ItemType, Item>();

    public void Show() {
      this.gameObject.SetActive(true);
    }

    public void Hide() {
      this.transform.parent = ItemFactory.Current.transform;
      ParentCell.ChildItem = null;
      ParentCell = null;
      this.gameObject.SetActive(false);
    }
  }
}
