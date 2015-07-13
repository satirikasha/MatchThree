namespace Elements.Game.MatchThree.Combinations {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class Three: Combination {

    public static bool IsCombination(out Combination result, Cell input) {
      result = new Three();
      result.Type = input.ChildItem.Type;
      if(input.Up != null && input.Up.ChildItem.Type == result.Type && input.Down != null && input.Down.ChildItem.Type == result.Type) {
        result.Cells = new Cell[] { input.Up, input, input.Down };
        return true;
      }
      if(input.Left != null && input.Left.ChildItem.Type == result.Type && input.Right != null && input.Right.ChildItem.Type == result.Type) {
        result.Cells = new Cell[] { input.Left, input, input.Right };
        return true;
      }
      result = null;
      return false;
    }
  }
}
