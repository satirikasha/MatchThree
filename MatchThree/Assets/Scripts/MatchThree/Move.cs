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

    public bool CanApply() {
      return From.CanMove && To.CanMove;
    }

    public void Apply() {
      if(!_Applied) {
        To.SwapItems(From);
        _Applied = true;
      }
    }

    public void Revert() {
      if(_Applied) {
        To.SwapItems(From);
        _Applied = false;
      }
    }
  }
}
