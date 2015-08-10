namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using Engine.Utils;
  using System;
  using Elements.Game.MatchThree.Data;

  public class Cell: MonoBehaviour {

    public CellData Data {
      get {
        if(_Data == null) {
          _Data = new CellData();
        }
        return _Data;
      }
    }
    private CellData _Data;

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
    private Item _ChildItem;

    public Position BoardPosition { 
      get {
        return Data.BoardPosition;
      } 
      set {
        Data.BoardPosition = value;
      } 
    }

    public bool CanGenerateItems { get { return Data.IsItemGenerator; } }
    public bool CanMove { get { return !(Data.IsBlocked || Data.IsVoid); } }

    #region Navigation properties
    public Cell Up {
      get {
        if(_Up == null && BoardPosition.y < BoardController.Current.Board.Size - 1)
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
        if(_Right == null && BoardPosition.x < BoardController.Current.Board.Size - 1)
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
      var fallDownCell = GetFallDownCell();
      if(fallDownCell != null) {
        fallDownCell.ChildItem = ChildItem;
        ChildItem = null;
      }
      if(CanGenerateItems && ChildItem == null) {
        ChildItem = ItemFactory.Current.GetItem(BoardController.Current.Board.ItemTypes.GetRandomElement());
        ChildItem.Show();
      }
    }

    public Cell GetFallDownCell() {
      Cell result = null;
      Cell nextCell = Down;
      while(nextCell != null && nextCell.Data.IsVoid) {
        nextCell = nextCell.Down;
      }
      if(nextCell != null) {
        if(nextCell.ChildItem == null) {
          result = nextCell;
        }
        //else {
        //  if(nextCell.Left != null && !nextCell.Left.Data.IsVoid && nextCell.Left.ChildItem == null) {
        //    result = nextCell.Left;
        //    return result;
        //  }
        //  if(nextCell.Right != null && !nextCell.Right.Data.IsVoid && nextCell.Right.ChildItem == null) {
        //    result = nextCell.Right;
        //    return result;
        //  }
        //}
      }
      return result;
    }

    public void LoadData(CellData data) {
      _Data = data;
    }

    public void SwapItems(Cell cell) {
      var item = cell.ChildItem;
      cell.ChildItem = this.ChildItem;
      this.ChildItem = item;
    }

    public void RemoveItem() {
      RemoveObstacle();
      if(ChildItem != null) {
        ChildItem.Hide();
      }
    }

    public void RemoveObstacle() {
      if(Data.IsClayed) {
        Data.IsClayed = false;
        ApplyVisuals();
        return;
      }
      if(Data.IsBlocked) {
        Data.IsBlocked = false;
        ApplyVisuals();
        return;
      }
    }

    public void ApplyVisuals() {
      // Disable all modificators if void
      if(Data.IsVoid) {
        Data.IsClayed = false;
        Data.IsBlocked = false;
      }
      // Apply visuals
      this.transform.GetChild(0).gameObject.SetActive(Data.IsClayed);
      this.transform.GetChild(1).gameObject.SetActive(Data.IsBlocked);
      this.GetComponent<SpriteRenderer>().enabled = !Data.IsVoid;
    }
  }
}
