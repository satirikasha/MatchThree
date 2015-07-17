namespace Elements.Game.MatchThree.Combinations {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;

  public class Three: Combination {

    public List<Variant> Variants = new List<Variant>();

    public Three(Combination combination) : base(combination) { }

    public static bool IsCombination(ref Combination combination) {// |2|
      var result = false;                                          // |1| or |0||1||2|
      var three = new Three(combination);                          // |0|
      var center = three.Center;
      #region Detection
      if(center.Up.IsNotNullOrEmpty() && center.Up.ChildItem.Type == three.Type && center.Down.IsNotNullOrEmpty() && center.Down.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center.Down, center, center.Up }, Orientation = Orientation.Vertical });
        result = true;
      }
      if(center.Left.IsNotNullOrEmpty() && center.Left.ChildItem.Type == three.Type && center.Right.IsNotNullOrEmpty() && center.Right.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center.Left, center, center.Right }, Orientation = Orientation.Horizontal });
        result = true;
      }
      if(center.Up.IsNotNullOrEmpty() && center.Up.ChildItem.Type == three.Type && center.Up.Up.IsNotNullOrEmpty() && center.Up.Up.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center, center.Up, center.Up.Up }, Orientation = Orientation.Vertical });
        result = true;
      }
      if(center.Down.IsNotNullOrEmpty() && center.Down.ChildItem.Type == three.Type && center.Down.Down.IsNotNullOrEmpty() && center.Down.Down.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center.Down.Down, center.Down, center }, Orientation = Orientation.Vertical });
        result = true;
      }
      if(center.Left.IsNotNullOrEmpty() && center.Left.ChildItem.Type == three.Type && center.Left.Left.IsNotNullOrEmpty() && center.Left.Left.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center.Left.Left, center.Left, center }, Orientation = Orientation.Vertical });
        result = true;
      }
      if(center.Right.IsNotNullOrEmpty() && center.Right.ChildItem.Type == three.Type && center.Right.Right.IsNotNullOrEmpty() && center.Right.Right.ChildItem.Type == three.Type) {
        three.Variants.Add(new Variant() { Cells = new Cell[] { center, center.Right, center.Right.Right }, Orientation = Orientation.Vertical });
        result = true;
      }
      #endregion
      Debug.Log("Fonund " + three.Variants.Count + " variants");
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
