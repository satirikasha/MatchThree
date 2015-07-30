namespace Elements.UI {
  using UnityEngine;
  using System.Collections;
  using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

  public class ElementsToggleWidget: MonoBehaviour {

    public int MinActiveElements = 3;

    private List<Toggle> ChildToggles;
    private Toggle LastDisabledToggle;

    void Start() {
      ChildToggles = new List<Toggle>(this.GetComponentsInChildren<Toggle>());
      foreach(var toggle in ChildToggles) {
        var toggleRef = toggle;
        toggle.onValueChanged.AddListener(_ => {
          if(!_) {
            if(ChildToggles.Count(a => a.isOn) < MinActiveElements)
              LastDisabledToggle.isOn = true;
            LastDisabledToggle = toggleRef;
          }
        });
      }
    }
  }
}
