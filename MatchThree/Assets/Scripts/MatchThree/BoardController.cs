﻿namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System;
  using Engine.Utils;
  using Random = UnityEngine.Random;
  using System.IO;
  using System.Runtime.Serialization.Formatters.Binary;
  using Elements.Game.MatchThree.Data;
  using Elements.Storage;
using UnityEngine.UI;

  public class BoardController: MonoBehaviour {

    public static BoardController Current {
      get { return _Current; }
    }
    protected static BoardController _Current;

    public float CellWidth = 2;
    public float CellHeight = 2;

    public Transform CellsContainer;
    public Transform ItemsContainer;

    public GameObject CellPrefab;

    public Camera Camera;
    public float CameraSizeCoeff = 0.25f;

    public bool IsInitialized { get; private set; }

    public Board Board { get; protected set; }

    public HashSet<Cell> RecentlyChangedCells { get; private set; }
    public int RecentlyChangedCellsCount;

    void Awake() {
      _Current = this;
      PrepareCells();
      PrepareBoard();
      PrepareCamera();
      PrepareFactory();
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

    private void PrepareBoard() {
      BoardData data;
      using(var ms = new MemoryStream(Resources.Load<TextAsset>("Levels/" + SessionStorage.LevelToLoad).bytes)) {
        data = new BinaryFormatter().Deserialize(ms) as BoardData;
      }
      Board = new Board();
      Board.Size = data.Size;
      PrepareCells();
      Board.SetData(data);
    }

    /// <summary>
    /// Board size shoud be set before preparing cells
    /// </summary>
    protected virtual void PrepareCells() {
      Board.Cells = new Cell[Board.Size, Board.Size];
      RecentlyChangedCells = new HashSet<Cell>();
      var offset = new Vector3(-Board.Size * CellWidth / 2, -Board.Size * CellHeight / 2);
      for(int i = 0; i < Board.Size; i++) {
        for(int j = 0; j < Board.Size; j++) {
          var cell = Instantiate(CellPrefab).GetComponent<Cell>();
          cell.transform.parent = CellsContainer;
          cell.transform.localPosition = new Vector3(i * CellWidth + CellWidth / 2, j * CellHeight + CellHeight / 2, 0) + offset;
          cell.BoardPosition = new Position() { x = i, y = j };
          cell.Data.IsItemGenerator = j == Board.Size - 1;
          cell.OnChildItemChanged += _ => { if(IsInitialized) RecentlyChangedCells.Add(_); };
          Board.Cells[i, j] = cell;
        }
      }
    }

    protected virtual void PrepareCamera() {
      Camera.orthographicSize -= (Board.MAX_SIZE - Board.Size) * CameraSizeCoeff;
    }

    private void PrepareFactory() {
      ItemFactory.Instantiate();
      Board.ItemTypes.ForEach(_ => ItemFactory.Current.AddItems(_, 50));
    }

    private void PrepareItems() {
      for(int i = 0; i < Board.Size; i++) {
        for(int j = 0; j < Board.Size; j++) {
          var cell = Board.Cells[i, j];
          if(!cell.Data.IsVoid) {
            var itemTypesAvailable = new List<ItemType>(Board.ItemTypes);
            var itemType = itemTypesAvailable.GetRandomElement();
            while((cell.Left.IsNotNullOrEmpty() && cell.Left.ChildItem.Type == itemType && cell.Left.Left.IsNotNullOrEmpty() && cell.Left.Left.ChildItem.Type == itemType)
               || (cell.Down.IsNotNullOrEmpty() && cell.Down.ChildItem.Type == itemType && cell.Down.Down.IsNotNullOrEmpty() && cell.Down.Down.ChildItem.Type == itemType)) {
              itemTypesAvailable.Remove(itemType);
              itemType = itemTypesAvailable.GetRandomElement();
            }
            cell.ChildItem = ItemFactory.Current.GetItem(itemType);
            cell.ChildItem.Show();
          }
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
        if(move.CanApply()) {
          move.Apply();
          Combination combination;
          bool isValid = false;
          if(Combination.Detect(out combination, move.To)) {
            combination.Remove();
            isValid = true;
          }
          if(Combination.Detect(out combination, move.From)) {
            combination.Remove();
            isValid = true;
          }
          if(!isValid) {
            move.Revert();
          }
        }
      };
    }

    public Cell GetCell(Vector2 screenPosition) {
      var offset = new Vector2(-Board.Size * CellWidth / 2, -Board.Size * CellHeight / 2);
      Vector2 pos = Camera.main.ScreenToWorldPoint(screenPosition);
      pos -= this.transform.position.ToVector2() + offset;
      var x = Mathf.RoundToInt((pos.x - CellWidth / 2) / CellWidth);
      var y = Mathf.RoundToInt((pos.y - CellHeight / 2) / CellHeight);
      return x.IsBetween(0, Board.Size - 1) && y.IsBetween(0, Board.Size - 1) ? Board.Cells[x, y] : null;
    }

    public void RemoveItems() {
      for(int i = 0; i < Board.Size; i++) {
        for(int j = 0; j < Board.Size; j++) {
          var cell = Board.Cells[i, j];
          if(cell.ChildItem != null)
            cell.ChildItem.Hide();
        }
      }
    }

    public MoveHash GetPossibleMoves() {
      MoveHash result = new MoveHash();
      for(int i = 0; i < Board.Size; i++) {
        for(int j = 0; j < Board.Size; j++) {
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
      IsInitialized = false;
      RemoveItems();
      PrepareItems();
      IsInitialized = true;
    }

    public void RandomMove() {
      GetPossibleMoves().GetRandomElement().Apply();
    }

    public void Load(InputField input) {
      SessionStorage.LevelToLoad = input.text;
      Application.LoadLevel(Application.loadedLevel);
    }
  }
}
