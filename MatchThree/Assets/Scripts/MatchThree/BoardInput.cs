namespace Elements.Game.MatchThree {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;
  using UnityEngine.EventSystems;
  using Engine.Utils;

  public class BoardInput: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public const float SWIPE_LENGTH_RATIO = 0.05f;

    public static BoardInput Current {
      get { return _Current; }
    }
    private static BoardInput _Current;

    /// <summary>
    /// Start position, Snap direction ({0,1}, {0,-1}, {1,0}, {-1,0})
    /// </summary>
    public event Action<Vector2,Vector2> OnSwipe;
    private bool ReceivingSwipe;

    void Awake() {
      _Current = this;
    }

    public void OnBeginDrag(PointerEventData eventData) {
      ReceivingSwipe = true;
    }

    public void OnDrag(PointerEventData eventData) {
      if(ReceivingSwipe) {
        var delta = eventData.position - eventData.pressPosition;
        if((SWIPE_LENGTH_RATIO * Screen.width).deg2() < delta.sqrMagnitude) {
          if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
            OnSwipe(eventData.pressPosition, delta.x > 0 ? Vector2.right : Vector2.left);
          }
          else {
            OnSwipe(eventData.pressPosition, delta.y > 0 ? Vector2.up : Vector2.down);
          }
          ReceivingSwipe = false;
        }
      }
    }

    public void OnEndDrag(PointerEventData eventData) { }
  }
}
