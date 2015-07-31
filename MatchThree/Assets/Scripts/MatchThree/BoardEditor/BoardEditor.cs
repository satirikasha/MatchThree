namespace Elements.Game.MatchThree.Editor {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System;
  using Engine.Utils;
  using Random = UnityEngine.Random;

  public class BoardEditor: MonoBehaviour {

    public static BoardEditor Current {
      get { return _Current; }
    }
    private static BoardEditor _Current;

    public const int CELLS_COUNT_X = 9;
    public const int CELLS_COUNT_Y = 9;

    public int ItemsCount = 5;
    public float CellWidth = 2;
    public float CellHeight = 2;

    public Transform CellsContainer;

    public GameObject CellPrefab;

    public Board Board { get; private set; }

    private Cell SelectedCell;

    void Awake() {
      _Current = this;
      PrepareCells();
    }

    private void PrepareCells() {
      Board = new Board();
      Board.Cells = new Cell[CELLS_COUNT_X, CELLS_COUNT_Y];
      var offset = new Vector3(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Instantiate(CellPrefab).GetComponent<Cell>();
          cell.transform.parent = CellsContainer;
          cell.transform.localPosition = new Vector3(i * CellWidth + CellWidth / 2, j * CellHeight + CellHeight / 2, 0) + offset;
          cell.BoardPosition = new Position() { x = i, y = j };
          cell.IsItemGenerator = j == CELLS_COUNT_Y - 1;
          cell.enabled = false;
          Board.Cells[i, j] = cell;
        }
      }
    }

    public Cell GetCell(Vector2 screenPosition) {
      var offset = new Vector2(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      Vector2 pos = Camera.main.ScreenToWorldPoint(screenPosition);
      pos -= this.transform.position.ToVector2() + offset;
      var x = Mathf.RoundToInt((pos.x - CellWidth / 2) / CellWidth);
      var y = Mathf.RoundToInt((pos.y - CellHeight / 2) / CellHeight);
      return x.IsBetween(0, CELLS_COUNT_X - 1) && y.IsBetween(0, CELLS_COUNT_Y - 1) ? Board.Cells[x, y] : null;
    }

    public void OnClick(Vector2 position) {
      Cell cell = GetCell(position);
      if(cell != null) {
        if(SelectedCell != null) {
          if(!ReferenceEquals(SelectedCell, cell)) {
            DeselectCell(SelectedCell);
            SelectedCell = cell;
            SelectCell(SelectedCell);
          }
          else {
            DeselectCell(SelectedCell);
            SelectedCell = null;
          }
        }
        else {
          SelectedCell = cell;
          SelectCell(SelectedCell);
        }
      }
    }

    private void SelectCell(Cell cell) {
      cell.GetComponent<SpriteRenderer>().color = Color.green;
    }

    private void DeselectCell(Cell cell) {
      cell.GetComponent<SpriteRenderer>().color = Color.white;
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

    public void New() {
      Application.LoadLevelAsync(Application.loadedLevel);
    }

    public void Delete() {
      if(SelectedCell != null) {
        SelectedCell.IsVoid = true;
        SelectedCell.ApplyVisuals();
        SelectedCell = null;
      }
    }

    public void Block() {
      if(SelectedCell != null) {
        SelectedCell.IsBlocked = !SelectedCell.IsBlocked;
        SelectedCell.ApplyVisuals();
      }
    }
  }
}
