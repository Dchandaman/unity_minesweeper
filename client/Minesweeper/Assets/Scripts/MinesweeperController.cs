using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinesweeperController : MonoBehaviour
{
	[Header("Default Values")]
	[SerializeField] private int _defaultWidth = 10;
	[SerializeField] private int _defaultHeight = 10;
	[SerializeField] private int _defaultMines = 10;

	[Header("Game Flow")]
	[SerializeField] private Animator _gameFlow = null;
	[SerializeField] private MinesweeperConnect _connector = null;

	[Header("Game Area")]
	[SerializeField] private GridLayoutGroup _grid = null;
	[SerializeField] private GridCell _gridCellPrefab = null;
	[SerializeField] private TMPro.TextMeshProUGUI _flagText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _timeText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _gameIdText = null;

	[Header("New Game Settings")]
	[SerializeField] private TMPro.TMP_InputField _widthInputField = null;
	[SerializeField] private TMPro.TMP_InputField _heightInputField = null;
	[SerializeField] private TMPro.TMP_InputField _minesInputField = null;

	public int width  { get; private set; } = 0;
	public int height { get; private set; } = 0;
	public int mines  { get; private set; } = 0;
	public int flags  { get => _flags; }

	private bool _active = false;
	private int _flags = 0;
	private List<GridCell> _gridCells = new List<GridCell>();

	private float _gameTime = 0.0f;
	private float _prevDisplaySeconds = 0.0f;

	private void Awake()
	{
		width = _defaultWidth;
		height = _defaultHeight;
		mines = _defaultMines;
	}

	private void OnDestroy()
	{
		_connector.OnPostResponse -= OnNewGame;
		_connector.OnPostResponse -= OnSelectCell;
	}

	private void Start()
	{
		_timeText.text = $"0";
		RefreshInputTextFields();

		_connector.OnPostResponse += OnNewGame;
		_connector.OnPostResponse += OnSelectCell;

		Next();
	}

	private void Update()
	{
		if(!_active)
			return;

		_gameTime += Time.deltaTime;

		if(Mathf.Floor(_gameTime) != _prevDisplaySeconds)
		{
			_prevDisplaySeconds = Mathf.Floor(_gameTime);
			_timeText.text = $"{_prevDisplaySeconds.ToString("0")}";
		}
	}

	private void UpdateFlagText()
	{
		_flagText.text = $"{_flags}";
	}

	private GridCell GetGridCell(int x, int y)
	{
		var index = x + y * width;

		if(index >= 0 && index < _gridCells.Count)
			return _gridCells[index];
		else
			return null;
	}

	private void RefreshInputTextFields()
	{
		_widthInputField.text = $"{width}";
		_heightInputField.text = $"{height}";
		_minesInputField.text = $"{mines}";
	}

	private void CheckWin()
	{
		int unclearedCells = 0;
		foreach(var cell in _gridCells)
		{
			if(!cell.cleared)
			{
				unclearedCells++;
				if(unclearedCells > mines)
					return;
			}
		}

		Win();
	}
	
	public void ShowNewGame()
	{
		RefreshInputTextFields();

		_gameFlow.Play("NewGame");
	}

	public void CloseNewGame()
	{
		RefreshInputTextFields();
		Back();
	}

	public void Back() { _gameFlow.SetTrigger("back"); }
	public void Next() { _gameFlow.SetTrigger("next"); }
	public void Error() { _gameFlow.SetTrigger("error"); _active = false; }
	public void Win() { _gameFlow.SetTrigger("win"); _active = false; }
	public void Lose() { _gameFlow.SetTrigger("lose"); _active = false; }

	public void NewGame()
	{
		//get latest values
		width = Int32.Parse(_widthInputField.text);
		height = Int32.Parse(_heightInputField.text);
		mines = Int32.Parse(_minesInputField.text);

		_gridCells.Clear();

		_flags = 0;
		UpdateFlagText();

		_gameIdText.text = "";
		
		_gameTime = 0.0f;
		//UpdateTime();

		// remove existing grid
		foreach(Transform child in _grid.gameObject.transform)
		{
			if(child != _grid.gameObject)
				GameObject.Destroy(child.gameObject);
		}
		
		_connector.NewGame(width, height, mines);
	}

	public void SelectCell(GridCell cell) 
	{
		_connector.SelectCell(cell.x, cell.y);
	}

	public void PlantFlag()
	{
		if(_flags <= 0)
			return;

		_flags--;
		UpdateFlagText();
	}

	public void PullFlag()
	{
		_flags++;
		UpdateFlagText();
	}

	private void OnNewGame(object sender, System.EventArgs args) => OnNewGame(sender as NewGameResponse);
	private void OnNewGame(NewGameResponse response)
	{
		if(response == null)
			return;

		if(string.IsNullOrEmpty(response.game_id))
		{
			Error();
			return;
		}

		_gameFlow.SetTrigger("new_game");

		_flags = mines;
		UpdateFlagText();

		_grid.constraintCount = width;

		// fill in grid
		int index = 0;
		for(int y = 0; y < height; ++y)
		{
			for(int x = 0; x < width; ++x)	
			{
				var cell = Instantiate(_gridCellPrefab, _grid.transform);
				cell.Setup(x, y, index, this);
				_gridCells.Add(cell);
				++index;
			}
		}

		_gameIdText.text = response.game_id;
		_active = true;

		Next();
	}

	private void OnSelectCell(object sender, System.EventArgs args) => OnSelectCell(sender as SelectResponse);
	private void OnSelectCell(SelectResponse response)
	{
		if(response == null)
			return;

		if(response.results != null)
		{
			foreach(var cell in response.results)
			{
				if(cell.value == -1)
					Lose();

				GetGridCell(cell.x, cell.y).ShowValue(cell.value);
			}
		}

		CheckWin();
	}

}
