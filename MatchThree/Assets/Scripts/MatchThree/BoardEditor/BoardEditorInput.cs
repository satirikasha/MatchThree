namespace Elements.Game.MatchThree.Editor {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;
  using UnityEngine.EventSystems;
  using Engine.Utils;
  using UnityEngine.UI;
using UnityEngine.Events;

  public class BoardEditorInput: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

    public static BoardEditorInput Current {
      get { return _Current; }
    }
    private static BoardEditorInput _Current;

    public static Dictionary<string, string> HotkeysDesc = new Dictionary<string, string>() {
        {"1",                "Enable/disable water"       },
        {"2",                "Enable/disable fire"        },
        {"3",                "Enable/disable wind"        },
        {"4",                "Enable/disable earth"       },
        {"5",                "Enable/disable electricity" },
        {"c",                "Add/remove clay"            },
        {"b",                "Add/remove block"           },
        {"↑",                "Move selection up"          },
        {"↓",                "Move selection down"        },
        {"→",                "Move selection right"       },
        {"←",                "Move selection left"        },
        {"esc",              "Switch to normal mode"      },
        {"ctrl + s",         "Save"                       },
        {"ctrl + shift + s", "Save as"                    },
        {"ctrl + shift + n", "New file"                   }
    };

    public UnityEvent OnEditorModeReset;

    public bool IsHovered { get; private set; }

    void Awake() {
      _Current = this;
    }

    void Start() {
      var rectTransform = (RectTransform)this.transform;
      rectTransform.position = Camera.main.WorldToScreenPoint(BoardEditor.Current.transform.position);
      var deltaPos = Camera.main.WorldToScreenPoint(BoardEditor.Current.transform.position - new Vector3(BoardEditor.Current.CellWidth * BoardEditor.CELLS_COUNT_X / 2, 0));
      var size = (rectTransform.position.x - deltaPos.x) * 2;
      rectTransform.sizeDelta = Vector2.one * size;
    }

    void Update() {
      if(IsHovered)
        BoardEditor.Current.OnHover(Input.mousePosition);
      if(Input.GetKeyDown(KeyCode.Escape))
        OnEditorModeReset.Invoke();
      if(Input.GetKeyDown(KeyCode.UpArrow))
        BoardEditor.Current.MoveSelectionUp();
      if(Input.GetKeyDown(KeyCode.DownArrow))
        BoardEditor.Current.MoveSelectionDown();
      if(Input.GetKeyDown(KeyCode.LeftArrow))
        BoardEditor.Current.MoveSelectionLeft();
      if(Input.GetKeyDown(KeyCode.RightArrow))
        BoardEditor.Current.MoveSelectionRight();
      if(Input.GetKeyDown(KeyCode.F1))
        BoardEditor.Current.HotKeys();
      if(Input.GetKeyDown(KeyCode.B))
        BoardEditor.Current.Block();
      if(Input.GetKeyDown(KeyCode.C))
        BoardEditor.Current.Clay();
      if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
          if(Input.GetKeyDown(KeyCode.N))
            BoardEditor.Current.New();
          if(Input.GetKeyDown(KeyCode.S))
            BoardEditor.Current.SaveAs();
        }
        else {
          if(Input.GetKeyDown(KeyCode.S))
            BoardEditor.Current.Save();
        }
      }
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

    public void OnPointerEnter(PointerEventData eventData) {
      IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      IsHovered = false;
      if(BoardEditor.Current.Mode == BoardEditorMode.Restore || BoardEditor.Current.Mode == BoardEditorMode.Delete)
      BoardEditor.Current.ClearSelection();
    }

    public void OnRestoreModeChanged(bool value) {
      if(value) {
        BoardEditor.Current.SetMode(BoardEditorMode.Restore);
      }
      else {
        if(BoardEditor.Current.Mode == BoardEditorMode.Restore) {
          BoardEditor.Current.SetMode(BoardEditorMode.Normal);
        }
      }
    }

    public void OnDeleteModeChanged(bool value) {
      if(value) {
        BoardEditor.Current.SetMode(BoardEditorMode.Delete);
      }
      else {
        if(BoardEditor.Current.Mode == BoardEditorMode.Delete) {
          BoardEditor.Current.SetMode(BoardEditorMode.Normal);
        }
      }
    }
  }
}
