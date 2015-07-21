namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System;

  [Serializable]
  public class Item : MonoBehaviour {

    public const string ITEMS_FOLDER_PATH = "Prefabs/Items/";

    [HideInInspector]
    public Cell ParentCell;
    [HideInInspector]
    public ItemType Type;

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

    /// <summary>
    /// Shows the item, requires an assigned parent cell.
    /// </summary>
    public void Show() {
      this.transform.parent = BoardController.Current.ItemsContainer;
      this.transform.localPosition = ParentCell.transform.localPosition;
      this.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the item
    /// </summary>
    public void Hide() {
      ParentCell.ChildItem = null;
      this.transform.parent = ItemFactory.Current.transform;
      this.gameObject.SetActive(false);
    }
  }
}
