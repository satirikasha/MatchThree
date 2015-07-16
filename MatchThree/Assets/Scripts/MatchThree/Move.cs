namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class Move {
    public Cell From { get; set; }
    public Cell To { get; set; }

    private bool _Applied = false;

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
      if(!_Applied) {
        var toItem = To.ChildItem;
        To.ChildItem = From.ChildItem;
        From.ChildItem = toItem;
        _Applied = true;
      }
    }

    public void Revert() {
      if(_Applied) {
        var toItem = To.ChildItem;
        To.ChildItem = From.ChildItem;
        From.ChildItem = toItem;
        _Applied = false;
      }
    }
  }
}
