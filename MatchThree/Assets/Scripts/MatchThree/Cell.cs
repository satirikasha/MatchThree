namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using Engine.Utils;
  using System;

  [Serializable]
  public class Cell: MonoBehaviour {

    public event Action<Cell> OnChildItemChanged;

    public Item ChildItem {
      get {
        return _ChildItem;
      }
      set {
        if(_ChildItem != null && ReferenceEquals(_ChildItem.ParentCell, this)) {
          _ChildItem.ParentCell = null;
        }
        _ChildItem = value;
        if(_ChildItem != null) {
          OnChildItemChanged(this);
          _ChildItem.ParentCell = this;
          _ChildItem.transform.position = this.transform.position;
        }
      }
    }
    [NonSerialized]
    private Item _ChildItem;

    public Position BoardPosition { get; set; }

    public bool CanGenerateItems { get { return IsItemGenerator; } }
    public bool CanMove { get { return !(IsBlocked || IsVoid); } }

    public bool IsItemGenerator;
    public bool IsVoid;
    public bool IsBlocked;

    #region Navigation properties
    public Cell Up {
      get {
        if(_Up == null && BoardPosition.y < BoardController.CELLS_COUNT_Y - 1)
          _Up = BoardController.Current.Board.Cells[BoardPosition.x, BoardPosition.y + 1];
        return _Up;
      }
    }
    private Cell _Up;

    public Cell Down {
      get {
        if(_Down == null && BoardPosition.y > 0)
          _Down = BoardController.Current.Board.Cells[BoardPosition.x, BoardPosition.y - 1];
        return _Down;
      }
    }
    private Cell _Down;

    public Cell Right {
      get {
        if(_Right == null && BoardPosition.x < BoardController.CELLS_COUNT_X - 1)
          _Right = BoardController.Current.Board.Cells[BoardPosition.x + 1, BoardPosition.y];
        return _Right;
      }
    }
    private Cell _Right;

    public Cell Left {
      get {
        if(_Left == null && BoardPosition.x > 0)
          _Left = BoardController.Current.Board.Cells[BoardPosition.x - 1, BoardPosition.y];
        return _Left;
      }
    }
    private Cell _Left;
    #endregion

    void Update() {
      if(Down != null && Down.ChildItem == null) {
        Down.ChildItem = ChildItem;
        ChildItem = null;
      }
      if(CanGenerateItems && ChildItem == null) {
        ChildItem = ItemFactory.Current.GetItem(BoardController.Current.Board.ItemTypes.GetRandomElement());
        ChildItem.Show();
      }
    }

    public void SwapItems(Cell cell) {
      var item = cell.ChildItem;
      cell.ChildItem = this.ChildItem;
      this.ChildItem = item;
    }

    public void RemoveItem() {
      if(ChildItem != null) {
        ChildItem.Hide();
      }
    }

    public void ApplyVisuals() {
      if(IsVoid)
        IsBlocked = false;
      this.transform.GetChild(0).gameObject.SetActive(IsBlocked);
      this.GetComponent<SpriteRenderer>().enabled = IsVoid;
    }
  }
}
