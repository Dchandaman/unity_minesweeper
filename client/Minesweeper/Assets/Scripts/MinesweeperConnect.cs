using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class NewGameResponse
{
	public string game_id;
	public string error;
}

[Serializable]
public class GridValues
{
	public int x;
	public int y;
	public int value;
}

[Serializable]
public class SelectResponse
{
	public List<GridValues> results;
	public string error;
}

public class MinesweeperConnect : MonoBehaviour
{
	[SerializeField] private string url = "http://localhost:3000";
	[SerializeField] private Transform _loadingIndicator = null;

	[Serializable]
	private class NewGamePostData
	{
		[Serializable]
		public class GridSize
		{
			public int x = 0;
			public int y = 0;
		}

		public GridSize grid_size = new GridSize();
		public int bomb_quantity;
	}

	[Serializable]
	private class SelectCellPostData
	{
		[Serializable]
		public class GridPosition
		{
			public int x = 0;
			public int y = 0;
		}

		public GridPosition grid_position = new GridPosition();
		public string game_id = "invalid";
	}

	private string _gameId = "invalid";
	private int _width = 0;
	private int _height = 0;
	private int _mines = 0;

	public delegate void PostResponseHandler(object sender, System.EventArgs args);
	public event PostResponseHandler OnPostResponse;

	private void Start()
	{
		_loadingIndicator.gameObject.SetActive(false);

		OnPostResponse += OnNewGame;
	}

	private void OnDestroy()
	{
		OnPostResponse -= OnNewGame;	
	}

	public void SelectCell(int x, int y)
	{
		var postData = new SelectCellPostData();
		postData.grid_position.x = x;
		postData.grid_position.y = y;
		postData.game_id = _gameId;

		StartCoroutine(SendPost<SelectResponse>("select", JsonUtility.ToJson(postData)));
	}

	public void NewGame(int width, int height, int mines)
	{
		_width = width;
		_height = height;
		_mines = mines;

		var postData = new NewGamePostData();
		postData.grid_size.x = _width;
		postData.grid_size.y = _height;
		postData.bomb_quantity = _mines;

		StartCoroutine(SendPost<NewGameResponse>("start", JsonUtility.ToJson(postData)));
	}

	IEnumerator SendPost<T>(string method, string jsonStr)
	{
		_loadingIndicator.gameObject.SetActive(true);

		yield return null;

		using(UnityWebRequest request = UnityWebRequest.Post($"{url}/{method}", jsonStr))
		{
			//Debug.Log($"Sent Data: {jsonStr}");
			byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonStr);

			request.SetRequestHeader("content-type", "application/json");
			request.uploadHandler = new UploadHandlerRaw(jsonBytes);
			request.uploadHandler.contentType = "application/json";

			yield return request.SendWebRequest();

			if ( request.result == UnityWebRequest.Result.ConnectionError || 
				 request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
                Debug.Log(request.url);
            }
			else
			{
				Debug.Log("Sent data!");
			}

			var resultStr = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
			var result = JsonUtility.FromJson<T>(resultStr);

			//Debug.Log($"Received result: {resultStr}");

			OnPostResponse?.Invoke(result, null);

			yield return null;
		}

		_loadingIndicator.gameObject.SetActive(false);

		yield return null;
	}

	private void OnNewGame(object sender, System.EventArgs args) => OnNewGame(sender as NewGameResponse);
	private void OnNewGame(NewGameResponse response)
	{
		if(response == null)
			return;

		_gameId = response.game_id;
	}
}
