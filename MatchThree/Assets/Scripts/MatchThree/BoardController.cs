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

    // Use this for initialization
    void Awake() {
      _Current = this;
      PrepareItemTypes();
      PrepareFactory();
      PrepareCells();
      PrepareItems();
    }

    // Update is called once per frame
    void Update() {

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

    private void RemoveItems() {
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board[i, j];
          cell.ChildItem.Hide();
        }
      }
    }

    public List<Move> GetPossibleMoves() {
      var time = DateTime.Now;
      HashSet<Move> result = new HashSet<Move>();
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Board[i, j];
          var cellType = cell.ChildItem.Type;
          if(cell.Right != null && cell.Right.ChildItem.Type == cellType) {
            if(cell.Left != null) {
              var targetCell = cell.Left;
              if(targetCell.Up != null && targetCell.Up.ChildItem.Type == cellType) {
                result.Add(new Move(targetCell, targetCell.Up)); // !! Сделать свой хешсет чтобы ходы считались одинаковыми
              }
              if(targetCell.Down != null && targetCell.Down.ChildItem.Type == cellType) {
                result.Add(new Move(targetCell, targetCell.Down));
              }
              if(targetCell.Left != null && targetCell.Left.ChildItem.Type == cellType) {
                result.Add(new Move(targetCell, targetCell.Left));
              }
            }
            if(cell.Right.Right != null) {
              var targetCell = cell.Right.Right;
              if(targetCell.Up != null && targetCell.Up.ChildItem.Type == cellType) {
                result.Add(new Move(targetCell, targetCell.Up));
              }
              if(targetCell.Down != null && targetCell.Down.ChildItem.Type == cellType) {
                result.Add(new Move(targetCell, targetCell.Down));
              }
              if(targetCell.Right != null && targetCell.Right.ChildItem.Type == cellType) {
                result.Add(new Move(targetCell, targetCell.Right));
              }
            }
          }
        }
      }
      var deltaTime = DateTime.Now - time;
      Debug.Log("Found " + result.Count + " possible moves");
      Debug.Log("Detecting moves took " + deltaTime.Milliseconds + " milliseconds");
      result.ToList().ForEach(_ => _.To.ChildItem.GetComponent<SpriteRenderer>().color = Color.black);
      return result.ToList();
    }

    public void Shuffle() {
      RemoveItems();
      PrepareItems();
      GetPossibleMoves();
    }
  }
}
