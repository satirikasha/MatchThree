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

    public static bool operator ==(Move x, Move y) {
      return ReferenceEquals(x.From, y.From) && ReferenceEquals(x.To, y.To) || ReferenceEquals(x.From, y.To) && ReferenceEquals(x.To, y.From);
    }

    public static bool operator !=(Move x, Move y) {
      return !(x == y);
    }

    public void Apply() {
      var toItem = To.ChildItem;
      To.ChildItem = From.ChildItem;
      From.ChildItem = toItem;
    }
  }
}
