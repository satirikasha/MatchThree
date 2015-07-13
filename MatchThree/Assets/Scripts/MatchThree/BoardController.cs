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
          item.ParentCell = cell;
          item.transform.parent = ItemsContainer;
          item.transform.localPosition = cell.transform.localPosition;
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
        if(GetPossibleMoves().Contains(move))
          move.Apply();
        Combination combination;
        foreach(var cell in Board) {
          Debug.Log("Detect attempt");
          if(Combination.Detect(out combination, cell)) {
            Debug.Log("Detected!");
            combination.Remove();
            break;
          }
        }
      };
    }

    private Cell GetCell(Vector2 position) {
      var offset = new Vector2(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      Vector2 pos = Camera.main.ScreenToWorldPoint(position);
      pos -= this.transform.position.ToVector2() + offset;
      return Board[Mathf.RoundToInt((pos.x - CellWidth / 2) / CellWidth), Mathf.RoundToInt((pos.y - CellHeight / 2) / CellHeight)];
    }

    private void RemoveItems() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board[i, j];
          cell.ChildItem.Hide();
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

          if(cell.Right != null && cell.Right.ChildItem.Type == cellType) {
            if(cell.Left != null) {
              var targetCell = cell.Left;
              if(targetCell.Up != null && targetCell.Up.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Up));
              if(targetCell.Down != null && targetCell.Down.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Down));
              if(targetCell.Left != null && targetCell.Left.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Left));
            }
            if(cell.Right.Right != null) {
              var targetCell = cell.Right.Right;
              if(targetCell.Up != null && targetCell.Up.ChildItem.Type == cellType) 
                result.Add(new Move(targetCell, targetCell.Up));
              if(targetCell.Down != null && targetCell.Down.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Down));
              if(targetCell.Right != null && targetCell.Right.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Right));
            }
          }

          if(cell.Right != null && cell.Right.Right != null && cell.Right.Right.ChildItem.Type == cellType) {
            var targetCell = cell.Right;
            if(targetCell.Up != null && targetCell.Up.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Up));
            if(targetCell.Down != null && targetCell.Down.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Down));
          }

          if(cell.Up != null && cell.Up.ChildItem.Type == cellType) {
            if(cell.Down != null) {
              var targetCell = cell.Down;
              if(targetCell.Right != null && targetCell.Right.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Right));
              if(targetCell.Down != null && targetCell.Down.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Down));
              if(targetCell.Left != null && targetCell.Left.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Left));
            }
            if(cell.Up.Up != null) {
              var targetCell = cell.Up.Up;
              if(targetCell.Up != null && targetCell.Up.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Up));
              if(targetCell.Left != null && targetCell.Left.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Left));
              if(targetCell.Right != null && targetCell.Right.ChildItem.Type == cellType)
                result.Add(new Move(targetCell, targetCell.Right));
            }
          }

          if(cell.Up != null && cell.Up.Up != null && cell.Up.Up.ChildItem.Type == cellType) {
            var targetCell = cell.Up;
            if(targetCell.Left != null && targetCell.Left.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Left));
            if(targetCell.Right != null && targetCell.Right.ChildItem.Type == cellType)
              result.Add(new Move(targetCell, targetCell.Right));
          }

          #endregion
        }
      }
      return result;
    }

    public void NormalizeBoard() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {

        }
      }
    }

    public void Shuffle() {
      RemoveItems();
      PrepareItems();
    }

    public void RandomMove() {
      GetPossibleMoves().GetRandomElement().Apply();
    }
  }
}
