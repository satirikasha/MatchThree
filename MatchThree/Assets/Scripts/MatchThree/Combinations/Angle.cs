namespace Elements.Game.MatchThree.Combinations {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;

  public class Angle: Three {

    public Angle(Three three) 
      : base(three) { }

    public Angle(Angle angle) 
      : base(angle) { }

    public static bool IsCombination(ref Three three) {
      var angle = new Angle(three);
      foreach(var variant1 in angle.ThreeVariants) {
        foreach(var variant2 in angle.ThreeVariants) {
          if(variant1.Orientation != variant2.Orientation) {
            HashSet<Cell> cells = new HashSet<Cell>();
            cells.UnionWith(variant1.Cells);
            cells.UnionWith(variant2.Cells);
            angle.Cells = cells.ToArray();
            three = angle;
            return true;
          }
        }
      }
      three = null;
      return false;
    }
  }
}
