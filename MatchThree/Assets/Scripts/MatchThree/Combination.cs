namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Combinations;
  using UnityEngine;

  public class Combination {

    public ItemType Type;
    public Cell Center;
    public Cell[] Cells;

    public Combination() { }

    public Combination(Combination combination) {
      Type = combination.Type;
      Center = combination.Center;
      Cells = combination.Cells;
    }

    public static bool Detect(out Combination combination, Cell input) {
      combination = new Combination();
      combination.Cells = GetAllNeighboursOfSameType(input).ToArray();
      combination.Center = input;
      combination.Type = input.ChildItem.Type;
      bool result = false;

      if(Three.IsCombination(ref combination)) {
        var three = new Three(combination);
        combination = three;
        if(Four.IsCombination(ref three))
          Debug.Log("Four detected");
        result = true;
      }

      if(!result)
        combination = null;
      return result;
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
