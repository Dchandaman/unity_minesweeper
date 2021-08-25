const express = require('express');
const bodyParser = require('body-parser');
const shortId = require('shortid');
const Minesweeper = require("./minesweeper.js");

const server = express();

server.use(bodyParser.urlencoded({extended: false}));
server.use(bodyParser.json());

server.listen(3000, () => console.log("Minesweeper server up and running..."));

let games = {};

function CreateNewGame(size_x, size_y, quantity)
{
	if(size_x < 1 || size_y < 1 || quantity < 1 || quantity > ((size_y * size_x) - 1))
		return undefined;

	let gameId = shortId.generate();
	let game = new Minesweeper(size_x, size_y, quantity, gameId);
	games[gameId] = game;

	return game;
}

server.post('/start', (req, res) => {
	let data = req.body;

	if(data.grid_size == undefined)
		res.send({ error: "grid_size not defined"});
	else if(data.grid_size.x == undefined)
		res.send({ error: "grid_size.x not defined"});
	else if(data.grid_size.y == undefined)
		res.send({ error: "grid_size.y not defined"});
	else if(data.bomb_quantity == undefined)
		res.send({error: "bomb_quantity not defined"})
	else
	{
		let game = CreateNewGame(data.grid_size.x, data.grid_size.y, data.bomb_quantity);

		if(game == undefined)
			res.send({error: "bad params: " + JSON.stringify(data)});
		else
			res.send({ game_id: game.gameId });
	}
});

server.post('/select', (req, res) => {
	let data = req.body;
	let response = {results: []};

	if(data.grid_position == undefined)
		response.error = "grid_position is undefined";
	else if(data.grid_position.x == undefined)
		response.error = "grid_position.x is undefined";
	else if(data.grid_position.y == undefined)
		response.error = "grid_position.y is undefined";
	else if(games[data.game_id] == undefined)
		response.error = "cannot find game id: " + data.game_id;
	else
	{
		let game = games[data.game_id];
		response.results = game.SelectCell(data.grid_position.x, data.grid_position.y);
	}

	res.send(response);
});