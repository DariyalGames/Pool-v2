using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour 
{
	void OnGUI()
	{
		//Show the start screen UI elements
		if(GUI.Button (new Rect(Screen.width/2 - 50, Screen.height/2 - 30, 100, 60), "Start Game"))
		{
			StartGame();
		}
	}

	void StartGame()
	{
		Debug.Log ("[GameStart]: Starting the game");

		DontDestroyOnLoad (GameManager.Instance);
		GameManager.Instance.StartState ();
	}
}
