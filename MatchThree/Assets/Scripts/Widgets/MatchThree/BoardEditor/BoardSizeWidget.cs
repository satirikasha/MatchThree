using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Elements.Game.MatchThree.Editor;

public class BoardSizeWidget : MonoBehaviour {

  public Slider Slider;
  public InputField Input;

	void Start () {
    Slider.onValueChanged.AddListener(_ => {
      Input.text = _.ToString();
      BoardEditorInput.Current.OnEditorModeReset.Invoke();
    });
    Input.onEndEdit.AddListener(_ => {
      int val;
      if(int.TryParse(_, out val)) {
        Slider.value = Mathf.Clamp(val, Slider.minValue, Slider.maxValue);
        Input.text = Slider.value.ToString();
      }
      else {
        Slider.value = Slider.minValue;
        Input.text = Slider.value.ToString();
      }
      BoardEditorInput.Current.OnEditorModeReset.Invoke();
    });
    BoardEditor.Current.OnLevelLoaded += _ => {
      Slider.value = _.Size;
      Input.text = _.Size.ToString();
    };
	}
}
