namespace Elements.Game.MatchThree.Data {
  using UnityEngine;
  using System.Collections;
  using Engine.Utils;
  using System;

  [Serializable]
  public class CellData {

    public Position BoardPosition;

    public bool IsItemGenerator;
    public bool IsVoid;
    public bool IsBlocked;
    public bool IsClayed;
  }
}
