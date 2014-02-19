using UnityEngine;
using System.Collections;


#region Enumerations and Structures
public enum GameStates { Start, TurnStart, PlayerTurn, Shooting, BallMove, BallPocket, TurnEnd, SwapPlayer, Reposition, Continue, End };
public enum BallTypes {White, Eight, Stripes, Solids, None };

public struct PlayerDetails 
{
	private readonly int playerID; 
	public int PlayerID { get { return playerID; } }
	private readonly string displayName; 
	public string DisplayName { get { return displayName; } }
	public BallTypes BallType;
	public int BallsLeft;

	public PlayerDetails(int id, string name)
	{
		playerID = id;
		displayName = name;
		BallsLeft = 7;
		BallType = BallTypes.None;
	}
}
#endregion


#region Events
public class  BallPocketEvent : GameEvent
{
	private int ballID;
	public int BallID { get { return ballID; } }

	private BallTypes ballType;
	public BallTypes BallType { get { return ballType; } }

	public BallPocketEvent(int id, BallTypes type)
	{
		ballID = id;
		ballType = type;
	}
}

public class InGameStart : GameEvent {}
public class StateChanged : GameEvent {}
public class TurnReady : GameEvent {}
public class ShotTaken : GameEvent {}
public class BallStopped : GameEvent {}
public class PowerSelect : GameEvent {}
public class WhiteReposition : GameEvent {}
public class RepositionDone : GameEvent {}
#endregion

// class controlling the gameplay.
public class GameManager : MonoBehaviour
{
	protected GameManager() {}

	#region Properties
	//--------------------------
	//Declare shared properties.
	//--------------------------
	public GameStates CurrentState {get; private set;}						//The active state.
	public void SetCurrentState (GameStates state)
	{
		CurrentState = state;
		EventManager.instance.Raise (new StateChanged());
	}

	public PlayerDetails CurrentPlayer;				//The active player's details.
	private PlayerDetails[] players;
	private int playerFlag;
	private bool isBreak;
	private bool isPocketed;

	//The static instance of the game state.
	private static GameManager instance = null;
	public static GameManager Instance
	{
		get
		{
			//create the instance as a gameobject if it doesnt exist.
			if(GameManager.instance == null)
			{
				instance = new GameObject("GameManager").AddComponent<GameManager>();
				//GameManager.instance = new GameManager();
			}

			return GameManager.instance;
		}
	}

	#endregion
	
	#region Public Methods
	//Start a new state.
	public void StartState()
	{
		Debug.Log ("[GameManager]: Create new State");

		#region Add Listeners
		// Add all listeners
		EventManager.instance.AddListener<BallPocketEvent> (OnBallPocketed);
		EventManager.instance.AddListener<InGameStart> (OnInGameStart);
		EventManager.instance.AddListener<TurnReady> (OnTurnReady);
		EventManager.instance.AddListener<ShotTaken> (OnShotTaken);
		EventManager.instance.AddListener<PowerSelect> (OnPowerSelect);
		EventManager.instance.AddListener<BallStopped> (OnBallStopped);
		EventManager.instance.AddListener<WhiteReposition> (OnWhiteReposition);
		EventManager.instance.AddListener<RepositionDone> (OnRepositionDone);
		#endregion

		players = new PlayerDetails[2];
		players[0] = new PlayerDetails(0, "Player1");
		players[1] = new PlayerDetails(1, "Player2");

		//load the first level.
		Application.LoadLevel ("game");
	}
	#endregion
	
	#region Event Listeners

	#region Unity Events
	//Remove the instance on application quit.
	public void OnApplicationQuit()
	{
		EventManager.instance.RemoveListener<BallPocketEvent> (OnBallPocketed);
		instance = null;
	}
	#endregion


	#region Custom Events
	// GameStarted
	private void OnInGameStart (InGameStart e)
	{
		Debug.Log ("[GameManager]: event (InGameStart)");
		SetCurrentState (GameStates.Start);

		StartCoroutine (ResetAll ());
	}

	// When all preliminary work is done.
	private void OnTurnReady (TurnReady e)
	{
		Debug.Log ("[GameManager]: event (TurnReady)");
		SetCurrentState (GameStates.PlayerTurn);
	}

	// Shot has been taken.
	private void OnShotTaken (ShotTaken e)
	{
		Debug.Log ("[GameManager]: event (ShotTaken)");
		SetCurrentState (GameStates.BallMove);
	}

	// player is selecting the power.
	private void OnPowerSelect (PowerSelect e)
	{
		Debug.Log ("[GameManager]: event (PowerSelect)");
		SetCurrentState (GameStates.Shooting);
	}

	// the cueball has stopped rolling.
	private void OnBallStopped (BallStopped e)
	{
		Debug.Log ("[GameManager]: event (BallStopped)");
		SetCurrentState (GameStates.TurnEnd);
		EndTurn ();
	}

	//A normal ball has been pocketed.				
	private void OnBallPocketed (BallPocketEvent e)
	{
		Debug.Log ("[GameManager]: Ball Pocketed caught with ballID:" + e.BallID.ToString ());
		isPocketed = true;
		if(isBreak)
		{
			isBreak = false;
		}
		else
		{
			if(CurrentPlayer.BallType == BallTypes.None)
				CurrentPlayer.BallType = e.BallType;

			if(players[0].BallType == e.BallType) 
			{
				players[0].BallsLeft--;
			}
			else if(players[1].BallType == e.BallType)
			{
				players[1].BallsLeft--;
			}
			else
			{
				Debug.Log ("[GameManager]: Foul pocketed wrong ball");
			}

			if(players[0].BallsLeft == 0)
			{
				players[0].BallType = BallTypes.Eight;
				players[0].BallsLeft = 1;
			}
			if(players[1].BallsLeft == 0)
			{
				players[1].BallType = BallTypes.Eight;
				players[1].BallsLeft = 1;
			}
		}
	}

	// white ball is being repositioned.
	private void OnWhiteReposition (WhiteReposition e)
	{
		Debug.Log ("[GameManager]: event (WhiteReposition)");
		SwapActivePlayer ();
		GameObject.Find ("GUI_activePlayer").guiText.text = "[ActivePlayer]:" + CurrentPlayer.DisplayName;
		SetCurrentState (GameStates.Reposition);
	}


	private void OnRepositionDone (RepositionDone e)
	{
		Debug.Log ("[GameManager]: event (RepositionDone)");
		isPocketed = false;
		SetCurrentState (GameStates.TurnStart);
	}

	#endregion

	#endregion

	#region Private Methods
	private void SwapActivePlayer ()
	{
		if (playerFlag == 0)
			playerFlag = 1;
		else 
			playerFlag = 0;

		CurrentPlayer = players[playerFlag];
	}

	private IEnumerator ResetAll ()
	{
		playerFlag = 0;
		CurrentPlayer = players[playerFlag];
		GameObject.Find ("GUI_activePlayer").guiText.text = CurrentPlayer.DisplayName;
		
		//Reset all normal balls.
		GameObject.Find("1").transform.position = new Vector3(7.1f, 1.8f, -9.2f);
		GameObject.Find("2").transform.position = new Vector3(7.8f, 1.8f, -9.6f);
		GameObject.Find("3").transform.position = new Vector3(7.8f,1.8f, -8.8f);
		GameObject.Find("4").transform.position = new Vector3(8.5f,1.8f,-10.0f);
		GameObject.Find("5").transform.position = new Vector3(8.5f,1.8f,-9.2f);
		GameObject.Find("6").transform.position = new Vector3(8.5f,1.8f,-8.4f);
		GameObject.Find("7").transform.position = new Vector3(9.2f,1.8f,-10.4f);
		GameObject.Find("8").transform.position = new Vector3(9.2f,1.8f,-9.6f);
		GameObject.Find("9").transform.position = new Vector3(9.2f,1.8f,-8.8f);
		GameObject.Find("10").transform.position = new Vector3(9.2f,1.8f,-8.0f);
		GameObject.Find("11").transform.position = new Vector3(9.9f,1.8f,-10.8f);
		GameObject.Find("12").transform.position = new Vector3(9.9f,1.8f,-10.0f);
		GameObject.Find("13").transform.position = new Vector3(9.9f,1.8f,-9.2f);
		GameObject.Find("14").transform.position = new Vector3(9.9f,1.8f,-8.4f);
		GameObject.Find("15").transform.position = new Vector3(9.9f,1.8f,-7.6f);
		
		float xpos = 0.1f;
		for(int i=1; i<=15; i++)
		{
			xpos += 0.03f;
			GameObject go = GameObject.Find (i+"_icon");
			go.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
			go.transform.position = new Vector3(xpos, go.transform.position.y, go.transform.position.z); 
			go.guiTexture.enabled = true;

			yield return new WaitForSeconds(0.1f);
		}

		yield return new WaitForSeconds(1.0f);

		isBreak = true;
		SetCurrentState (GameStates.TurnStart);
	}

	private void EndTurn ()
	{
		if(!isPocketed)
		{
			SwapActivePlayer ();
			GameObject.Find ("GUI_activePlayer").guiText.text = "[ActivePlayer]:" + CurrentPlayer.DisplayName;
		}

		isPocketed = false;
		SetCurrentState(GameStates.TurnStart);
	}
	#endregion
}
