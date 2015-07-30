namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System;
  using Engine.Utils;
  using Random = UnityEngine.Random;

  public class BoardController: MonoBehaviour {

    public static BoardController Current {
      get { return _Current; }
    }
    private static BoardController _Current;

    public const int CELLS_COUNT_X = 9;
    public const int CELLS_COUNT_Y = 9;

    public int ItemsCount = 5;
    public float CellWidth = 2;
    public float CellHeight = 2;

    public Transform CellsContainer;
    public Transform ItemsContainer;

    public GameObject CellPrefab;

    public bool IsInitialized { get; private set; }

    public Board Board { get; private set; }

    public HashSet<Cell> RecentlyChangedCells { get; private set; }
    public int RecentlyChangedCellsCount;

    void Awake() {
      _Current = this;
      PrepareItemTypes();
      PrepareFactory();
      PrepareCells();
      PrepareItems();
      PrepareInput();
      IsInitialized = true;
    }

    void Update() {
      if(RecentlyChangedCells.Count != RecentlyChangedCellsCount) {
        RecentlyChangedCellsCount = RecentlyChangedCells.Count;
      }
      else {
        var combination = new Combination();
        foreach(var cell in RecentlyChangedCells) {
          if(Combination.Detect(out combination, cell))
            combination.Remove();
        }
        RecentlyChangedCells = new HashSet<Cell>();
        RecentlyChangedCellsCount = 0;
      }
    }

    private void PrepareItemTypes() {
      Board = new Board();
      Board.ItemTypes = Enum.GetValues(typeof(ItemType)).Cast<ItemType>().ToList().GetRange(0, ItemsCount);
    }

    private void PrepareFactory() {
      ItemFactory.Instantiate();
      Board.ItemTypes.ForEach(_ => ItemFactory.Current.AddItems(_, 50));
    }

    private void PrepareCells() {
      Board.Cells = new Cell[CELLS_COUNT_X, CELLS_COUNT_Y];
      RecentlyChangedCells = new HashSet<Cell>();
      var offset = new Vector3(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Instantiate(CellPrefab).GetComponent<Cell>();
          cell.transform.parent = CellsContainer;
          cell.transform.localPosition = new Vector3(i * CellWidth + CellWidth / 2, j * CellHeight + CellHeight / 2, 0) + offset;
          cell.BoardPosition = new Position() { x = i, y = j };
          cell.IsItemGenerator = j == CELLS_COUNT_Y - 1;
          cell.OnChildItemChanged += _ => { if(IsInitialized) RecentlyChangedCells.Add(_); };
          Board.Cells[i, j] = cell;
        }
      }
    }

    private void PrepareItems() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board.Cells[i, j];
          var itemTypesAvailable = new List<ItemType>(Board.ItemTypes);
          var itemType = itemTypesAvailable.GetRandomElement();
          while((cell.Left != null && cell.Left.ChildItem.Type == itemType && cell.Left.Left != null && cell.Left.Left.ChildItem.Type == itemType)
             || (cell.Down != null && cell.Down.ChildItem.Type == itemType && cell.Down.Down != null && cell.Down.Down.ChildItem.Type == itemType)) {
            itemTypesAvailable.Remove(itemType);
            itemType = itemTypesAvailable.GetRandomElement();
          }
          cell.ChildItem = ItemFactory.Current.GetItem(itemType);
          cell.ChildItem.Show();
        }
      }
    }

    private void PrepareInput() {
      BoardInput.Current.OnSwipe += (pos, dir) => {
        Cell fromCell;
        Cell toCell;
        fromCell = GetCell(pos);
        if(fromCell == null)
          return;
        if(dir == Vector2.left)
          toCell = fromCell.Left;
        else if(dir == Vector2.right)
          toCell = fromCell.Right;
        else if(dir == Vector2.down)
          toCell = fromCell.Down;
        else if(dir == Vector2.up)
          toCell = fromCell.Up;
        else throw new Exception("Bad snap direction vector");
        if(toCell == null)
          return;
        Move move = new Move(fromCell, toCell);
        move.Apply();
        Combination combination;
        bool isValid = false;
        //List<Cell> affectedCells = new List<Cell>();
        if(Combination.Detect(out combination, move.To)) {
          combination.Remove();
          isValid = true;
          // affectedCells.AddRange(combination.Cells);
        }
        if(Combination.Detect(out combination, move.From)) {
          combination.Remove();
          isValid = true;
          //affectedCells.AddRange(combination.Cells);
        }
        if(!isValid/*affectedCells.Count == 0*/) {
          move.Revert();
        }
        else {
          //StartCoroutine(NormalizeBoard(affectedCells));
        }
      };
    }

    private Cell GetCell(Vector2 screenPosition) {
      var offset = new Vector2(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      Vector2 pos = Camera.main.ScreenToWorldPoint(screenPosition);
      pos -= this.transform.position.ToVector2() + offset;
      var x = Mathf.RoundToInt((pos.x - CellWidth / 2) / CellWidth);
      var y = Mathf.RoundToInt((pos.y - CellHeight / 2) / CellHeight);
      return x.IsBetween(0, CELLS_COUNT_X - 1) && y.IsBetween(0, CELLS_COUNT_Y - 1) ? Board.Cells[x, y] : null;
    }

    public void RemoveItems() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board.Cells[i, j];
          if(cell.ChildItem != null)
            cell.RemoveItem();
        }
      }
    }

    public MoveHash GetPossibleMoves() {
      MoveHash result = new MoveHash();
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          #region Moves detecting
          var cell = Board.Cells[i, j];
          var cellType = cell.ChildItem.Type;
          if(cell.Right.IsNotNullOrEmpty() && cell.Right.ChildItem.Type == cellType) {
            if(cell.Left.IsNotNullOrEmpty()) {
              var targetCell = cell.Left;
              if(targetCell.Up.IsNotNullOrEmpty() && targetCell.Up.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Up));
              if(targetCell.Down.IsNotNullOrEmpty() && targetCell.Down.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Down));
              if(targetCell.Left.IsNotNullOrEmpty() && targetCell.Left.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Left));
            }
            if(cell.Right.Right.IsNotNullOrEmpty()) {
              var targetCell = cell.Right.Right;
              if(targetCell.Up.IsNotNullOrEmpty() && targetCell.Up.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Up));
              if(targetCell.Down.IsNotNullOrEmpty() && targetCell.Down.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Down));
              if(targetCell.Right.IsNotNullOrEmpty() && targetCell.Right.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Right));
            }
          }

          if(cell.Right.IsNotNullOrEmpty() && cell.Right.Right.IsNotNullOrEmpty() && cell.Right.Right.ChildItem.Type == cellType) {
            var targetCell = cell.Right;
            if(targetCell.Up.IsNotNullOrEmpty() && targetCell.Up.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Up));
            if(targetCell.Down.IsNotNullOrEmpty() && targetCell.Down.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Down));
          }

          if(cell.Up.IsNotNullOrEmpty() && cell.Up.ChildItem.Type == cellType) {
            if(cell.Down.IsNotNullOrEmpty()) {
              var targetCell = cell.Down;
              if(targetCell.Right.IsNotNullOrEmpty() && targetCell.Right.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Right));
              if(targetCell.Down.IsNotNullOrEmpty() && targetCell.Down.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Down));
              if(targetCell.Left.IsNotNullOrEmpty() && targetCell.Left.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Left));
            }
            if(cell.Up.Up.IsNotNullOrEmpty()) {
              var targetCell = cell.Up.Up;
              if(targetCell.Up.IsNotNullOrEmpty() && targetCell.Up.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Up));
              if(targetCell.Left.IsNotNullOrEmpty() && targetCell.Left.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Left));
              if(targetCell.Right.IsNotNullOrEmpty() && targetCell.Right.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Right));
            }
          }

          if(cell.Up.IsNotNullOrEmpty() && cell.Up.Up.IsNotNullOrEmpty() && cell.Up.Up.ChildItem.Type == cellType) {
            var targetCell = cell.Up;
            if(targetCell.Left.IsNotNullOrEmpty() && targetCell.Left.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Left));
            if(targetCell.Right.IsNotNullOrEmpty() && targetCell.Right.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Right));
          }

          #endregion
        }
      }
      return result;
    }

    public void Shuffle() {
      //!!
      foreach(var cell in Board.Cells)
        cell.GetComponent<SpriteRenderer>().color = Color.white;
      //!!
      IsInitialized = false;
      RemoveItems();
      PrepareItems();
      IsInitialized = true;
    }

    public void RandomMove() {
      GetPossibleMoves().GetRandomElement().Apply();
    }
  }
}
