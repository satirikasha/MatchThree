namespace Elements.Game.MatchThree.Combinations {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;

  public class Four: Three {

    //|3|
    //|2| or |0||1||2||3|
    //|1|
    //|0|

    public List<Variant> FourVariants = new List<Variant>();

    public Four(Three three) 
      : base(three) { }

    public Four(Four four) 
      : base(four) {
        FourVariants = four.FourVariants;
    }

    public static bool IsCombination(ref Three three) {
      var result = false;
      var four = new Four(three);
      foreach(var variant in four.ThreeVariants) {
        if(variant.Orientation == Orientation.Horizontal && variant.Cells[2].Right.IsNotNullOrEmpty() && variant.Cells[2].Right.ChildItem.Type == four.Type) {
          four.FourVariants.Add(new Variant() { Cells = new Cell[] { variant.Cells[0], variant.Cells[1], variant.Cells[2], variant.Cells[2].Right }, Orientation = Orientation.Horizontal });
          result = true;
        }
        if(variant.Orientation == Orientation.Vertical && variant.Cells[2].Up.IsNotNullOrEmpty() && variant.Cells[2].Up.ChildItem.Type == four.Type) {
          four.FourVariants.Add(new Variant() { Cells = new Cell[] { variant.Cells[0], variant.Cells[1], variant.Cells[2], variant.Cells[2].Up }, Orientation = Orientation.Vertical });
          result = true;
        }
      }
      if(result) {
        four.Cells = four.FourVariants.FirstOrDefault().Cells;
        three = four;
      }
      return result;
    }

    public new class Variant {
      public Cell[] Cells;
      public Orientation Orientation;
    }
  }
}
