namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class Move {
    public Cell From { get; set; }
    public Cell To { get; set; }

    public Move(Cell from, Cell to) {
      From = from;
      To = to;
    }
  }
}
