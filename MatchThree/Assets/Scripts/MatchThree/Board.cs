namespace Elements.Game.MatchThree{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

  [Serializable]
  public class Board {
    public Cell[,] Cells;
    public List<ItemType> ItemTypes;
  }
}
