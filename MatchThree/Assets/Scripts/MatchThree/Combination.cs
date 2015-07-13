namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Combinations;

  public abstract class Combination: Cell {

    public ItemType Type;

    public Cell[] Cells;

    public static bool Detect(out Combination result, Cell input) {
      result = null;
      if(Three.IsCombination(out result, input)) {
        return true;
      }
      else {
        result = null;
        return false;
      }
    }

    public void Remove() {
      foreach(var cell in Cells)
        cell.ChildItem.Hide();
    }
  }
}
