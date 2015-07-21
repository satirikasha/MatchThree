namespace Elements.Game.MatchThree.Combinations {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;

  public class Five: Four {

    public Five(Four four) 
      : base(four) { }

    public Five(Five five) 
      : base(five) { }

    public static bool IsCombination(ref Four four) {
      var result = false;
      var five = new Five(four);
      foreach(var variant in five.FourVariants) {
        if(variant.Orientation == Orientation.Horizontal && variant.Cells[3].Right.IsNotNullOrEmpty() && variant.Cells[3].Right.ChildItem.Type == five.Type) {
          five.Cells = new Cell[5];
          Array.Copy(variant.Cells, five.Cells, variant.Cells.Length);
          five.Cells[4] = variant.Cells[3].Right;
          result = true;
        }
        if(variant.Orientation == Orientation.Vertical && variant.Cells[3].Up.IsNotNullOrEmpty() && variant.Cells[3].Up.ChildItem.Type == five.Type) {
          five.Cells = new Cell[5];
          Array.Copy(variant.Cells, five.Cells, variant.Cells.Length);
          five.Cells[4] = variant.Cells[3].Up;
          result = true;
        }
      }
      four = result ? five : null;
      return result;
    }
  }
}
