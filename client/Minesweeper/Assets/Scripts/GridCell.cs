using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;


public class GridCell : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private Animator _animator = null;
	[SerializeField] private TMPro.TextMeshProUGUI _numberText = null;
	[SerializeField] private List<Color> _numberColors = null;
	[SerializeField] private Image _highlight = null;

	public int x { get => _x; }
	public int y { get => _y; } 
	public int index { get => _index; }
	public bool cleared { get => _cleared; }

	private int _x = 0;
	private int _y = 0;
	private int _index = 0;
	private bool _cleared = false;
	private bool _flagged = false;
	private MinesweeperController _controller = null;

	private void Start()
	{
		_animator.SetBool("cleared", false);
		_animator.SetBool("flagged", false);
		var bg = x + (y % 2 == 0 ? 1 : 0);
		_animator.SetInteger("background", (bg % 2) + 1);
	}

	private void OnMouseOver()
    {
    	if(!_cleared)
    		_highlight.gameObject.SetActive(true);
    	else
    		_highlight.gameObject.SetActive(false);
    }

    private void OnMouseExit()
    {
    	_highlight.gameObject.SetActive(false);
    }

	public void OnPointerClick(PointerEventData eventData)
	{
		if(_cleared == true)
			return;
			
		if (eventData.button == PointerEventData.InputButton.Left)
		    _controller.SelectCell(this);
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			if(_flagged)
				PullFlag();
			else if(_controller.flags > 0)
				PlantFlag();
		}
	}

	public void Setup(int x, int y, int index, MinesweeperController controller)
	{
		_x = x;
		_y = y;
		_index = index;
		_cleared = false;
		_controller = controller;
	}

	public void ShowValue(int cellValue)
	{
		_animator.SetBool("cleared", true);
		
		RevealValue(cellValue);

		_cleared = true;
	}

	public void RevealValue(int cellValue)
	{
		_animator.SetInteger("value", cellValue);

		if(cellValue > 0)
		{
			_numberText.text = $"{cellValue}";
			if(cellValue < _numberColors.Count)
				_numberText.color = _numberColors[cellValue - 1];
			else
				_numberText.color = _numberColors.Last();
		}
	}

	public void PlantFlag()
	{
		_flagged = true;
		_animator.SetBool("flagged", _flagged);
		_controller.PlantFlag();
	}

	public void PullFlag()
	{
		_flagged = false;
		_animator.SetBool("flagged", _flagged);	
		_controller.PullFlag();
	}
}
