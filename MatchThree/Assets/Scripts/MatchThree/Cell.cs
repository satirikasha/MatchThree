namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;

  public class Cell : MonoBehaviour {

    public Item ChildItem {
      get {
        return _ChildItem;
      }
      set {
        _ChildItem = value;
        if(value != null)
          _ChildItem.transform.position = this.transform.position;
      }
    }
    private Item _ChildItem;

    public Position BoardPosition { get; set; }

    public Cell Up {
      get {
        if(_Up == null && BoardPosition.y < BoardController.CELLS_COUNT_Y - 1)
          _Up = BoardController.Current.Board[BoardPosition.x, BoardPosition.y + 1];
        return _Up;
      }
    }
    private Cell _Up;

    public Cell Down {
      get {
        if(_Down == null && BoardPosition.y > 0 )
          _Down = BoardController.Current.Board[BoardPosition.x, BoardPosition.y - 1];
        return _Down;
      }
    }
    private Cell _Down;

    public Cell Right {
      get {
        if(_Right == null && BoardPosition.x < BoardController.CELLS_COUNT_X - 1)
          _Right = BoardController.Current.Board[BoardPosition.x + 1, BoardPosition.y];
        return _Right;
      }
    }
    private Cell _Right;

    public Cell Left {
      get {
        if(_Left == null && BoardPosition.x > 0)
          _Left = BoardController.Current.Board[BoardPosition.x - 1, BoardPosition.y];
        return _Left;
      }
    }
    private Cell _Left;
  }
}
