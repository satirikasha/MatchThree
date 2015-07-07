namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System;
  using Engine.Utils;
  using Random = UnityEngine.Random;

  public class ItemFactory: MonoBehaviour {

    public static ItemFactory Current {
      get { return _Current; }
    }
    private static ItemFactory _Current;

    private Dictionary<ItemType,Queue<Item>> Items;

    // Use this for initialization
    public static void Instantiate() {
      var factory = new GameObject("ItemFactory", typeof(ItemFactory)).GetComponent<ItemFactory>();
      _Current = factory;
      factory.transform.parent = BoardController.Current.transform;
      factory.Items = new Dictionary<ItemType, Queue<Item>>();
    }

    public void AddItems(ItemType type, int count) {
      count--;
      while(count != 0) {
        var item = Instantiate(Item.GetInstance(type));
        item.transform.parent = this.transform;
        if(!Items.ContainsKey(type))
          Items.Add(type, new Queue<Item>());
        Items[type].Enqueue(item);
        count--;
      }
    }

    public Item GetItem(ItemType type) {
      var item = Items[type].Dequeue();
      Items[type].Enqueue(item);
      return item;
    }
  }
}
