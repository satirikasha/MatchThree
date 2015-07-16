namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Combinations;
  using UnityEngine;

  public abstract class Combination {

    public ItemType Type;

    public Cell Center;

    public Cell[] Cells;

    public static bool Detect(out Combination result, Cell input) {
      result = new Three();
      result.Cells = GetAllNeighboursOfSameType(input).ToArray();
      return true;
      //result = null;
      //if(Three.IsCombination(out result, input)) {
      //  return true;
      //}
      //else {
      //  result = null;
      //  return false;
      //}
    }

    private static HashSet<Cell> GetAllNeighboursOfSameType(Cell input) {
      HashSet<Cell> result = new HashSet<Cell>();
      HashSet<Cell> cellsToCheck = new HashSet<Cell>();
      result.Add(input);
      cellsToCheck.Add(input);
      while(cellsToCheck.Count != 0) {
        HashSet<Cell> newCellsToCheck = new HashSet<Cell>();
        result.UnionWith(cellsToCheck);
        foreach(var cell in cellsToCheck) {
           newCellsToCheck.UnionWith(GetNeighboursOfSameType(cell, result));
        }
        cellsToCheck = newCellsToCheck;
      }
      return result;
    }

    private static HashSet<Cell> GetNeighboursOfSameType(Cell input, IEnumerable<Cell> exclude = null) {
      HashSet<Cell> result = new HashSet<Cell>();
      if(exclude == null)
        exclude = new List<Cell>();
      if(!input.IsNotNullOrEmpty())
        input.GetComponent<SpriteRenderer>().color = Color.gray;
      var type = input.ChildItem.Type;
      Cell currentCell;
      for(int i = 0; i < 4; i++) {
        switch(i) {
          case 0: currentCell = input.Left; break;
          case 1: currentCell = input.Down; break;
          case 2: currentCell = input.Right; break;
          case 3: currentCell = input.Up; break;
          default: throw new Exception("Unknown cell side index");
        }
        if(currentCell.IsNotNullOrEmpty() && currentCell.ChildItem.Type == type && !exclude.Contains(currentCell))
          result.Add(currentCell);
      }
      return result;
    }

    public void Remove() {
      foreach(var cell in Cells)
        if(cell.IsNotNullOrEmpty())
          cell.ChildItem.Hide();
    }
  }
}
