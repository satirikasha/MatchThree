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

    public Cell[,] Board { get; private set; }

    private List<ItemType> itemTypes;

    void Awake() {
      _Current = this;
      PrepareItemTypes();
      PrepareFactory();
      PrepareCells();
      PrepareItems();
      PrepareInput();
    }

    private void PrepareItemTypes() {
      itemTypes = Enum.GetValues(typeof(ItemType)).Cast<ItemType>().ToList().GetRange(0, ItemsCount);
    }

    private void PrepareFactory() {
      ItemFactory.Instantiate();
      itemTypes.ForEach(_ => ItemFactory.Current.AddItems(_, 50));
    }

    private void PrepareCells() {
      Board = new Cell[CELLS_COUNT_X, CELLS_COUNT_Y];
      var offset = new Vector3(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Instantiate(CellPrefab).GetComponent<Cell>();
          cell.transform.parent = CellsContainer;
          cell.transform.localPosition = new Vector3(i * CellWidth + CellWidth / 2, j * CellHeight + CellHeight / 2, 0) + offset;
          cell.BoardPosition = new Position() { x = i, y = j };
          Board[i, j] = cell;
        }
      }
    }

    private void PrepareItems() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board[i, j];
          var itemTypesAvailable = new List<ItemType>(itemTypes);
          var itemType = itemTypesAvailable.GetRandomElement();
          while((cell.Left != null && cell.Left.ChildItem.Type == itemType && cell.Left.Left != null && cell.Left.Left.ChildItem.Type == itemType)
             || (cell.Down != null && cell.Down.ChildItem.Type == itemType && cell.Down.Down != null && cell.Down.Down.ChildItem.Type == itemType)) {
            itemTypesAvailable.Remove(itemType);
            itemType = itemTypesAvailable.GetRandomElement();
          }
          var item = ItemFactory.Current.GetItem(itemType);
          cell.ChildItem = item;
          item.Show();
        }
      }
    }

    private void PrepareInput() {
      BoardInput.Current.OnSwipe += (pos, dir) => {
        Cell fromCell;
        Cell toCell;
        fromCell = GetCell(pos);
        if(dir == Vector2.left)
          toCell = fromCell.Left;
        else if(dir == Vector2.right)
          toCell = fromCell.Right;
        else if(dir == Vector2.down)
          toCell = fromCell.Down;
        else if(dir == Vector2.up)
          toCell = fromCell.Up;
        else throw new Exception("Bad snap direction vector");
        Move move = new Move(fromCell, toCell);
        move.Apply();
        Combination combination;
        List<Cell> affectedCells = new List<Cell>();
        if(Combination.Detect(out combination, move.To)) {
          combination.Remove();
          affectedCells.AddRange(combination.Cells);
        }
        if(Combination.Detect(out combination, move.From)) {
          combination.Remove();
          affectedCells.AddRange(combination.Cells);
        }
        if(affectedCells.Count == 0) {
          move.Revert();
        }
        else {
          NormalizeBoard(affectedCells);
        }
      };
    }

    private Cell GetCell(Vector2 screenPosition) {
      var offset = new Vector2(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      Vector2 pos = Camera.main.ScreenToWorldPoint(screenPosition);
      pos -= this.transform.position.ToVector2() + offset;
      return Board[Mathf.RoundToInt((pos.x - CellWidth / 2) / CellWidth), Mathf.RoundToInt((pos.y - CellHeight / 2) / CellHeight)];
    }

    public void RemoveItems() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board[i, j];
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
          var cell = Board[i, j];
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

    public void NormalizeBoard(List<Cell> affectedCells) {
      Cell currentCell;
      HashSet<Cell> cellsToCheck = new HashSet<Cell>();
      int posX;
      foreach(var cell in affectedCells) {
        posX = cell.BoardPosition.x;
        int loopCount = CELLS_COUNT_Y;
        while(cell.ChildItem == null) {
          loopCount--;
          for(int j = cell.BoardPosition.y; j < CELLS_COUNT_Y - 1; j++) {
            currentCell = Board[posX, j];
            currentCell.SwapItems(currentCell.Up);
            cellsToCheck.Add(currentCell);
            if(j == CELLS_COUNT_Y - 2)
              cellsToCheck.Add(currentCell.Up);
          }
          for(int i = 0; i < CELLS_COUNT_X; i++) {
            var productionCell = Board[i, CELLS_COUNT_Y - 1];
            if(productionCell.ChildItem == null) {
              productionCell.ChildItem = ItemFactory.Current.GetItem(itemTypes.GetRandomElement());
              productionCell.ChildItem.Show();
            }
          }
          if(loopCount == 0)
            break;
        }
      }
      Combination combination;
      List<Cell> newCellsAffected = new List<Cell>();
      foreach(var cell in cellsToCheck) {
        if(Combination.Detect(out combination, cell)) {
          combination.Remove();
          newCellsAffected.AddRange(combination.Cells);
        }
      }
      if(newCellsAffected.Count != 0)
        NormalizeBoard(newCellsAffected);
    }

    public void Shuffle() {
      //!!
      foreach(var cell in Board)
        cell.GetComponent<SpriteRenderer>().color = Color.white;
      //!!
      RemoveItems();
      PrepareItems();
    }

    public void RandomMove() {
      GetPossibleMoves().GetRandomElement().Apply();
    }
  }
}
