namespace Elements.Game.MatchThree {
  using UnityEngine;
  using System.Collections;
  using Engine.Utils;

  public class GameManager: MonoBehaviour {

    public static GameManager Current {
      get {
        return _Current;
      }
    }
    private static GameManager _Current;

    void Awake() {
      _Current = this;
      FPSCounter.Instantiate();
      DebugConsole.Instantiate((int)(Screen.width / 3), 10);
    }
  }
}
