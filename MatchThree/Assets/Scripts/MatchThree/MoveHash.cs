namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class MoveHash : List<Move> {
    public new void Add(Move item){
      if(this.TrueForAll(_ => _ != item))
        base.Add(item);
    }
  }
}
