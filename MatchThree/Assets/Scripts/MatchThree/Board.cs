namespace Elements.Game.MatchThree {
  using Elements.Game.MatchThree.Data;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;

  public class Board {

    public const int MAX_SIZE = 9;

    public int Size = MAX_SIZE;
    public Cell[,] Cells;
    public List<ItemType> ItemTypes;

    public BoardData GetData() {
      var data = new BoardData();
      data.Size = Size;
      data.ItemTypes = ItemTypes.ToArray();
      data.Cells = new CellData[Cells.GetLength(0), Cells.GetLength(1)];
      for(int i = 0; i < Cells.GetLength(0); i++)
        for(int j = 0; j < Cells.GetLength(1); j++)
          data.Cells[i, j] = Cells[i, j].Data;
      return data;
    }

    public void SetData(BoardData data) {
      Size = data.Size;
      ItemTypes = new List<ItemType>(data.ItemTypes);
      for(int i = 0; i < Cells.GetLength(0); i++) {
        for(int j = 0; j < Cells.GetLength(1); j++) {
          Cells[i, j].LoadData(data.Cells[i, j]);
          Cells[i, j].ApplyVisuals();
        }
      }
    }
  }
}
