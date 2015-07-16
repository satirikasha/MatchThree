namespace Elements.Game.MatchThree.Combinations {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;

  public class Three: Combination {

    public List<Variant> Variants = new List<Variant>();

    public Three(Combination combination) : base(combination) { }

    public static bool IsCombination(ref Combination combination) {
      var result = false;
      var three = new Three(combination);
      var center = three.Center;
      if(center.Up.IsNotNullOrEmpty() && center.Up.ChildItem.Type == three.Type && center.Down.IsNotNullOrEmpty() && center.Down.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center.Up, center, center.Down }, Orientation = Orientation.Vertical });
        result = true;
      }
      if(center.Left.IsNotNullOrEmpty() && center.Left.ChildItem.Type == three.Type && center.Right.IsNotNullOrEmpty() && center.Right.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center.Left, center, center.Right }, Orientation = Orientation.Horizontal });
        result = true;
      }

      three.Variants.ForEach(_ => _.Cells.ToList().ForEach(a => a.GetComponent<SpriteRenderer>().color = Color.black));

      if(result)
        combination = three;
      return result;
    }

    public class Variant {
      public Cell[] Cells;
      public Orientation Orientation;
    }
  }
}
