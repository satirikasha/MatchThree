namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class MoveHash : List<Move> {
    public new void Add(Move item) {
      if(!this.Contains(item))
        base.Add(item);
    }

    public new bool Contains(Move item) {
      return this.Any(_ => _ == item);
    }
  }
}
