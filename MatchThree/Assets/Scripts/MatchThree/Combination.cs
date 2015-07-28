namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Combinations;
  using UnityEngine;
  using Engine.Utils;

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
      if(!input.IsNotNullOrEmpty())
        return false;
      combination.Cells = new Cell[0];
      combination.Center = input;
      combination.Type = input.ChildItem.Type;

      if(Three.IsCombination(ref combination)) {
        var three = combination as Three;
        combination = three;
        if(Four.IsCombination(ref three)) {
          var four = three as Four;
          combination = four;
          if(Five.IsCombination(ref four)) {
            var five = four as Five;
            combination = five;
          }
        }
        three = combination as Three;
        if(combination.GetType() != typeof(Five) && Angle.IsCombination(ref three)) {
          var angle = three as Angle;
          combination = angle;
        }
      }

      var resultName = combination.GetType().Name;
      if(resultName != "Combination")
        Debug.Log(resultName);

      var result = combination.GetType() != typeof(Combination);
      if(!result)
        combination = null;
      return result;
    }

    public static HashSet<Cell> GetAllNeighboursOfSameType(Cell input) {
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
      var color = Utils.GetRandomColor();//!!
      foreach(var cell in Cells)
        if(cell.IsNotNullOrEmpty()) {
          cell.RemoveItem();
          cell.GetComponent<SpriteRenderer>().color = color;//!!
        }
    }
  }
}
