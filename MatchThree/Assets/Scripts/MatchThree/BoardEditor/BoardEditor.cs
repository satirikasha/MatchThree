namespace Elements.Game.MatchThree.Editor {
  using System;
  using System.IO;
  using System.Linq;
  using System.Collections;
  using System.Collections.Generic;
  using System.Windows.Forms;
  using System.Runtime.Serialization.Formatters.Binary;
  using UnityEngine;
  using Engine.Utils;
  using Random = UnityEngine.Random;
  using Application = UnityEngine.Application;
  using Elements.Game.MatchThree.Data;



  public class BoardEditor: BoardController {

    public static new BoardEditor Current { get { return BoardController._Current as BoardEditor; } }

    public event Action<Board> OnLevelLoaded;

    public const string FILE_EXT = ".m3l";

    public BoardEditorMode Mode { get; private set; }

    private Cell SelectedCell;

    private string CurrentFilePath;

    void Awake() {
      _Current = this;
      PrepareItemTypes();
      PrepareCells();
    }

    void Start() {
      OnLevelLoaded(Board);
    }

    private void PrepareCells() {
      Board.Cells = new Cell[CELLS_COUNT_X, CELLS_COUNT_Y];
      var offset = new Vector3(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      for(int i = 0; i < CELLS_COUNT_X; i++) {
        for(int j = 0; j < CELLS_COUNT_Y; j++) {
          var cell = Instantiate(CellPrefab).GetComponent<Cell>();
          cell.transform.parent = CellsContainer;
          cell.transform.localPosition = new Vector3(i * CellWidth + CellWidth / 2, j * CellHeight + CellHeight / 2, 0) + offset;
          cell.BoardPosition = new Position() { x = i, y = j };
          cell.Data.IsItemGenerator = j == CELLS_COUNT_Y - 1;
          cell.enabled = false;
          Board.Cells[i, j] = cell;
        }
      }
    }

    void Update() { }

    public Cell GetCell(Vector2 screenPosition) {
      var offset = new Vector2(-CELLS_COUNT_X * CellWidth / 2, -CELLS_COUNT_Y * CellHeight / 2);
      Vector2 pos = Camera.main.ScreenToWorldPoint(screenPosition);
      pos -= this.transform.position.ToVector2() + offset;
      var x = Mathf.RoundToInt((pos.x - CellWidth / 2) / CellWidth);
      var y = Mathf.RoundToInt((pos.y - CellHeight / 2) / CellHeight);
      return x.IsBetween(0, CELLS_COUNT_X - 1) && y.IsBetween(0, CELLS_COUNT_Y - 1) ? Board.Cells[x, y] : null;
    }

    public Cell GetNearestCell(Vector2 screenPosition, Func<Cell, bool> selector) {
      Vector3 pos = Camera.main.ScreenToWorldPoint(screenPosition);
      Cell result = null;
      float minSqrDistance = float.MaxValue;
      float curSqrDistance = 0;
      foreach(var cell in Board.Cells) {
        if(selector(cell)) {
          curSqrDistance = (pos - cell.transform.position).sqrMagnitude;
          if(curSqrDistance < minSqrDistance) {
            result = cell;
            minSqrDistance = curSqrDistance;
          }
        }
      }
      return result;
    }

    public void SetMode(BoardEditorMode mode) {
      if(SelectedCell != null) {
        DeselectCell(SelectedCell);
        SelectedCell = null;
      }
      Mode = mode;
    }

    public void OnClick(Vector2 position) {
      if(Mode == BoardEditorMode.Normal) {
        Cell cell = GetCell(position);
        if(cell != null && !cell.Data.IsVoid) {
          if(SelectedCell != null) {
            if(!ReferenceEquals(SelectedCell, cell)) {
              DeselectCell(SelectedCell);
              SelectedCell = cell;
              SelectCell(SelectedCell);
            }
            else {
              DeselectCell(SelectedCell);
              SelectedCell = null;
            }
          }
          else {
            SelectedCell = cell;
            SelectCell(SelectedCell);
          }
        }
      }
      else {
        if(Mode == BoardEditorMode.Delete)
          Delete();
        if(Mode == BoardEditorMode.Restore)
          Restore();
      }
    }

    public void OnHover(Vector2 position) {
      if(Mode == BoardEditorMode.Delete || Mode == BoardEditorMode.Restore) {
        Cell cell;
        if(Mode == BoardEditorMode.Delete)
          cell = GetNearestCell(position, _ => !_.Data.IsVoid);
        else
          cell = GetNearestCell(position, _ => _.Data.IsVoid);
        if(cell != null) {
          if(!ReferenceEquals(SelectedCell, cell)) {
            if(SelectedCell != null)
              DeselectCell(SelectedCell);
            SelectedCell = cell;
            SelectCell(cell);
          }
        }
      }
    }

    private void SelectCell(Cell cell) {
      switch(Mode) {
        case BoardEditorMode.Delete: cell.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); break;
        case BoardEditorMode.Restore: var renderer = cell.GetComponent<SpriteRenderer>(); renderer.enabled = true; renderer.color = new Color(1, 1, 1, 0.5f); break;
        default: cell.GetComponent<SpriteRenderer>().color = Color.green; break;
      }
    }

    private void DeselectCell(Cell cell) {
      switch(Mode) {
        case BoardEditorMode.Delete: cell.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1); break;
        case BoardEditorMode.Restore: var renderer = cell.GetComponent<SpriteRenderer>(); renderer.color = new Color(1, 1, 1, 1); if(cell.Data.IsVoid) renderer.enabled = false; break;
        default: cell.GetComponent<SpriteRenderer>().color = Color.white; break;
      }
    }

    public void ClearSelection() {
      if(SelectedCell != null) {
        DeselectCell(SelectedCell);
        SelectedCell = null;
      }
    }

    private void SaveToFile(string path) {
      using(var fs = new FileStream(path, FileMode.Create)) {
        new BinaryFormatter().Serialize(fs, Board.GetData());
      }
    }

    private void LoadFromFile(string path) {
      using(var fs = new FileStream(path, FileMode.Open)) {
        Board.SetData(new BinaryFormatter().Deserialize(fs) as BoardData);
        OnLevelLoaded(Board);
      }
    }

    public void Save() {
      if(!String.IsNullOrEmpty(CurrentFilePath))
        SaveToFile(CurrentFilePath);
      else
        SaveAs();
    }

    public void SaveAs() {
      var dialog = new SaveFileDialog();
      dialog.Filter = FILE_EXT + " | *" + FILE_EXT;
      if(PlayerPrefs.HasKey("InitialDir"))
        dialog.InitialDirectory = PlayerPrefs.GetString("InitialDir");
      dialog.AddExtension = true;
      dialog.RestoreDirectory = true;

      if(dialog.ShowDialog() == DialogResult.OK) {
        PlayerPrefs.SetString("InitialDir", dialog.FileName.Remove(dialog.FileName.LastIndexOf('\\') + 1));
        CurrentFilePath = dialog.FileName;
        SaveToFile(dialog.FileName);
      }
    }

    public void Load() {
      var dialog = new OpenFileDialog();
      dialog.Filter = FILE_EXT + " | *" + FILE_EXT;
      if(PlayerPrefs.HasKey("InitialDir"))
        dialog.InitialDirectory = PlayerPrefs.GetString("InitialDir");
      dialog.AddExtension = true;
      dialog.RestoreDirectory = true;

      if(dialog.ShowDialog() == DialogResult.OK){
        CurrentFilePath = dialog.FileName;
        LoadFromFile(dialog.FileName);
      }
    }

    public void New() {
      if(MessageBox.Show("Your current progress will be lost. \n Do you want to proceed?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        Application.LoadLevelAsync(Application.loadedLevel);
    }

    public void HotKeys() {
      var form = new Form();
      var keyLayout = new FlowLayoutPanel();
      var valLayout = new FlowLayoutPanel();
      keyLayout.Padding = new Padding(5);
      valLayout.Padding = new Padding(5);
      keyLayout.Dock = DockStyle.Left;
      valLayout.Dock = DockStyle.Right;
      keyLayout.FlowDirection = FlowDirection.TopDown;
      valLayout.FlowDirection = FlowDirection.TopDown;
      keyLayout.AutoSize = true;
      valLayout.AutoSize = true;
      form.Controls.Add(keyLayout);
      form.Controls.Add(valLayout);
      foreach(var desc in BoardEditorInput.HotkeysDesc) {
        var keyLabel = new Label();
        var valLabel = new Label();
        keyLabel.Text = desc.Key;
        valLabel.Text = desc.Value;
        keyLabel.AutoSize = true;
        valLabel.AutoSize = true;
        keyLayout.Controls.Add(keyLabel);
        valLayout.Controls.Add(valLabel);
      }
      form.AutoSize = true;
      form.Width = keyLayout.Width + valLayout.Width;
      form.KeyPreview = true;
      form.KeyDown += (s, e) => { if(e.KeyCode == Keys.Escape) form.Close(); };
      form.Show();
    }

    public void MoveSelectionUp() {
      if(SelectedCell != null && SelectedCell.Up != null) {
        DeselectCell(SelectedCell);
        SelectedCell = SelectedCell.Up;
        SelectCell(SelectedCell);
      }
    }

    public void MoveSelectionDown() {
      if(SelectedCell != null && SelectedCell.Down != null) {
        DeselectCell(SelectedCell);
        SelectedCell = SelectedCell.Down;
        SelectCell(SelectedCell);
      }
    }

    public void MoveSelectionLeft() {
      if(SelectedCell != null && SelectedCell.Left != null) {
        DeselectCell(SelectedCell);
        SelectedCell = SelectedCell.Left;
        SelectCell(SelectedCell);
      }
    }

    public void MoveSelectionRight() {
      if(SelectedCell != null && SelectedCell.Right != null) {
        DeselectCell(SelectedCell);
        SelectedCell = SelectedCell.Right;
        SelectCell(SelectedCell);
      }
    }

    public void Delete() {
      if(SelectedCell != null) {
        SelectedCell.Data.IsVoid = true;
        SelectedCell.ApplyVisuals();
        ClearSelection();
      }
    }

    public void Restore() {
      if(SelectedCell != null) {
        SelectedCell.Data.IsVoid = false;
        SelectedCell.ApplyVisuals();
        ClearSelection();
      }
    }

    public void Block() {
      if(SelectedCell != null) {
        SelectedCell.Data.IsBlocked = !SelectedCell.Data.IsBlocked;
        SelectedCell.ApplyVisuals();
      }
    }

    public void Clay() {
      if(SelectedCell != null) {
        SelectedCell.Data.IsClayed = !SelectedCell.Data.IsClayed;
        SelectedCell.ApplyVisuals();
      }
    }
  }
}
