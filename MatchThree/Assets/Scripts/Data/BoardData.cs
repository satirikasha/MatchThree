namespace Elements.Game.MatchThree.Data {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  [Serializable]
  public class BoardData {
    public CellData[,] Cells;
    public ItemType[] ItemTypes;
  }
}
