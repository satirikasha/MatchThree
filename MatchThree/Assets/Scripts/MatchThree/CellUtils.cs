namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;

  public static class GameUtils {

    public static bool IsNotNullOrEmpty(this Cell cell) {
      return cell != null && cell.ChildItem != null;
    }
  }
}
