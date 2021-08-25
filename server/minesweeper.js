'use strict';

const _ = require('lodash');

module.exports = class Minesweeper
{
	#width = 0;
	#height = 0;
	#mines = 0;
	#gameId = "invalid";
	#gameGrid = [];
	#mineLocations = [];

	constructor(width, height, numMines, gameId)
	{
		this.width = width;
		this.height = height;
		this.mines = numMines;
		this.gameId = gameId;
		this.gameGrid = [];
		this.mineLocations = [];

		this.InitializeField();
	}

	GetIndex = (x, y) =>
	{
		if(x < 0 || x >= this.width || y < 0 || y >= this.height)
			return -1;

		return y * this.width + x;
	}

	GetCoordinates = (index) =>
	{
		var y = Math.floor( index / this.width );
		var x = index % this.width;
		return {x, y};
	}

	GetValue = (x, y) =>
	{
		let index = this.GetIndex(x, y);
		if(index == -1)
			return undefined;
		else
			return this.gameGrid[index];
	}

	CallOnSurroundingCells = (x, y, func) =>
	{
		for(let i = -1; i <= 1; ++i)
		{
			for(let j = -1; j <= 1; ++j)
			{
				let xoff = i + x;
				let yoff = j + y;

				if(i == 0 && j == 0)
					continue;

				if(xoff < 0 || xoff >= this.width || yoff < 0 || yoff >= this.height)
					continue;
				else
					func(xoff, yoff);
			}
		}
	}

	MarkCells = (index) =>
	{
		let coord = this.GetCoordinates(index);

		this.CallOnSurroundingCells(coord.x, coord.y, (x, y) =>{
			let adjIndex = this.GetIndex(x, y);
			let val = this.gameGrid[adjIndex];
			if(val != -1)
				this.gameGrid[adjIndex]++;
		});
	}

	InitializeField = () =>
	{
		this.gameGrid = Array(this.width * this.height);
		_.fill(this.gameGrid, 0);
		_.fill(this.gameGrid, -1, 0, this.mines);
		this.gameGrid = _.shuffle(this.gameGrid);
		for(let i = 0; i < this.gameGrid.length; ++i)
		{
			let val = this.gameGrid[i];
			if(val == -1)
				this.mineLocations.push(i);
		}
		//console.log("mine locations: " + JSON.stringify(this.mineLocations));

		//walk through mines and determine value in each cell
		this.mineLocations.forEach( index => this.MarkCells(index) );

		// var grid = _.chunk(this.gameGrid, this.width);
		// grid.forEach(value => console.log(JSON.stringify(value)));
	}

	SelectCell = (x, y) =>
	{
		//console.log("Select Cell(" + x + "," + y + ")");
		let index = this.GetIndex(x,y);
		if(index == -1)
			return [];

		let value = this.gameGrid[index];
		let retVal = { visited: [], results: []};

		if(value == 0)
		{
			this.SelectCellRecursive(x, y, retVal);
			return retVal.results;
		}
		else
			return [{x, y, value}];
	}

	SelectCellRecursive = (x, y, retVal) =>
	{
		let index = this.GetIndex(x,y);
		if(index == -1)
			return;

		if(retVal.visited.indexOf(index) != -1)
			return;

		retVal.visited.push(index);

		let value = this.gameGrid[index];
		//console.log("SelectCellRecursive: (" + x + "," + y + ") = " + value);
		
		if(value == undefined)
			return;

		retVal.results.push({x, y, value});
		if(value == 0)
		{
			this.CallOnSurroundingCells(x, y, (offx, offy) => {
				this.SelectCellRecursive(offx, offy, retVal);
			});
		}
	}
};