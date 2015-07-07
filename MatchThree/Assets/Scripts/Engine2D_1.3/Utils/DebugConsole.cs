namespace Engine.Utils {
  using System;
  using System.Collections.Generic;
  using UnityEngine;
  using System.Linq;

  public class DebugConsole: MonoBehaviour {

    private static DebugConsole _Current;

    private Rect _WindowRect;
    private Queue<LogRecord> RecordQueue;
    private float _Capacity;

    public static void Instantiate(int width, int capacity) {
      if(_Current == null && Debug.isDebugBuild) {
        var obj = new GameObject("DebugConsole", typeof(DebugConsole)).GetComponent<DebugConsole>();
        _Current = obj;
        obj.RecordQueue = new Queue<LogRecord>();
        obj._WindowRect = new Rect(Screen.width - 5 - width, 5, width, 0);
        obj._Capacity = capacity;
        Application.logMessageReceived += (c, s, t) => {
          AddLog(new LogRecord() {
            Message = c,
            Type = t,
          });
        };
        DontDestroyOnLoad(obj.gameObject);
      }
    }

    private static void AddLog(LogRecord record) {
      if(_Current != null) {
        _Current.RecordQueue.Enqueue(record);
        if(_Current.RecordQueue.Count > _Current._Capacity)
          _Current.RecordQueue.Dequeue();
      }
    }

    void OnGUI() {
      _WindowRect = GUILayout.Window(0, _WindowRect, UpdateWindow, "Console");
    }

    private void UpdateWindow(int id) {
      GUIStyle style = new GUIStyle();
      style.richText = true;
      GUI.UnfocusWindow();
      if(RecordQueue.Count != 0)
      for(int i = 0; i < RecordQueue.Count; i++) {
        var element = RecordQueue.ElementAt(i);
        GUILayout.Label("<color=" + element.Color + ">" + element.Message + "</color>", style);
      }
      else
        GUILayout.Label("");
    }
  }

  public struct LogRecord {
    public string Message;
    public LogType Type {
      get {
        return _Type;
      }
      set {
        _Type = value;
        switch(value) {
          case LogType.Log:       Color = "white";  break;
          case LogType.Warning:   Color = "yellow"; break;
          case LogType.Error:     Color = "red";    break;
          case LogType.Assert:    Color = "red"; break;
          case LogType.Exception: Color = "red"; break;
        }
      }
    }
    private LogType _Type;
    public string Color { get; private set; }
  }
}
