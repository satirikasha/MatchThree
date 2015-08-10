namespace Elements.UI {
  using UnityEngine;
  using System.Collections;
  using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
  using Elements.Game.MatchThree.Editor;
  using System;
  using Elements.Game.MatchThree;

  public class ElementsToggleWidget: MonoBehaviour {

    public int MinActiveElements = 3;

    private List<Toggle> ChildToggles;
    private Toggle LastDisabledToggle;

    void Awake() {
      ChildToggles = new List<Toggle>(this.GetComponentsInChildren<Toggle>());
      foreach(var toggle in ChildToggles) {
        var toggleRef = toggle;
        toggle.onValueChanged.AddListener(_ => {
          if(!_) {
            if(ChildToggles.Count(a => a.isOn) < MinActiveElements)
              LastDisabledToggle.isOn = true;
            LastDisabledToggle = toggleRef;
          }

          if(_)
            BoardEditor.Current.Board.ItemTypes.Add((ItemType)Enum.Parse(typeof(ItemType), toggleRef.name));
          else
            BoardEditor.Current.Board.ItemTypes.Remove((ItemType)Enum.Parse(typeof(ItemType), toggleRef.name));
        });

        BoardEditor.Current.OnLevelLoaded += _ => {
          ChildToggles.ForEach(t => t.interactable = false);
          ChildToggles.ForEach(t => t.isOn = _.ItemTypes.Contains((ItemType)Enum.Parse(typeof(ItemType), t.name)));
          ChildToggles.ForEach(t => t.interactable = true);
        };
      }
    }

    void Update() {
      if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
        if(Input.GetKeyDown(KeyCode.Alpha1))
          ChildToggles[0].isOn = !ChildToggles[0].isOn;
        if(Input.GetKeyDown(KeyCode.Alpha2))
          ChildToggles[1].isOn = !ChildToggles[1].isOn;
        if(Input.GetKeyDown(KeyCode.Alpha3))
          ChildToggles[2].isOn = !ChildToggles[2].isOn;
        if(Input.GetKeyDown(KeyCode.Alpha4))
          ChildToggles[3].isOn = !ChildToggles[3].isOn;
        if(Input.GetKeyDown(KeyCode.Alpha5))
          ChildToggles[4].isOn = !ChildToggles[4].isOn;
      }
    }
  }
}
