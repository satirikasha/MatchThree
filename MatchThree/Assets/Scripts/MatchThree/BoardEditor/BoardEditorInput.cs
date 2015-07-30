namespace Elements.Game.MatchThree.Editor {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;
  using UnityEngine.EventSystems;
  using Engine.Utils;

  public class BoardEditorInput: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler {

    public static BoardEditorInput Current {
      get { return _Current; }
    }
    private static BoardEditorInput _Current;

    void Awake() {
      _Current = this;
    }

    void Update() {
      if(Input.GetKeyDown(KeyCode.Delete))
        BoardEditor.Current.OnDelete();
    }

    public void OnBeginDrag(PointerEventData eventData) {
    }

    public void OnDrag(PointerEventData eventData) {
    }

    public void OnEndDrag(PointerEventData eventData) { 
    }

    public void OnPointerDown(PointerEventData eventData) {
      BoardEditor.Current.OnClick(eventData.pressPosition);
    }
  }
}
