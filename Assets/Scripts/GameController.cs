using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    public GameObject connectionErrorUI;
    public GameObject logInUI;
    public GameObject signUpUI;
    public GameObject startUI;
    public GameObject pauseUI;
    public GameObject inGameUI;
    public static GameState currentState;
    public static GameState prevState;

    public enum GameState
    {
        GAME_START,
        SERVER_CONNECTION_ERROR,
        SIGN_UP,
        LOG_IN,
        GAME_PAUSE,
        GAME_PLAY
    };

    void Start () {
        currentState = GameState.GAME_PLAY;
        prevState = currentState;
	}
	
	void FixedUpdate () {
        // sets up the current GUI
        switch (currentState)
        {
            case GameState.GAME_START:
                signUpUI.SetActive(false);
                logInUI.SetActive(false);
                pauseUI.SetActive(false);
                connectionErrorUI.SetActive(false);
                inGameUI.SetActive(false);
                startUI.SetActive(true);
                InputController.gamePaused = true;
                break;

            case GameState.SERVER_CONNECTION_ERROR:
                startUI.SetActive(false);
                signUpUI.SetActive(false);
                logInUI.SetActive(false);
                pauseUI.SetActive(false);
                inGameUI.SetActive(false);
                connectionErrorUI.SetActive(true);
                break;

            case GameState.GAME_PAUSE:
                startUI.SetActive(false);
                signUpUI.SetActive(false);
                logInUI.SetActive(false);
                connectionErrorUI.SetActive(false);
                inGameUI.SetActive(false);
                pauseUI.SetActive(true);
                InputController.gamePaused = true;
                break;

            case GameState.LOG_IN:
                startUI.SetActive(false);
                signUpUI.SetActive(false);
                connectionErrorUI.SetActive(false);
                pauseUI.SetActive(false);
                inGameUI.SetActive(false);
                logInUI.SetActive(true);
                InputController.gamePaused = true;
                break;

            case GameState.SIGN_UP:
                startUI.SetActive(false);
                logInUI.SetActive(false);
                connectionErrorUI.SetActive(false);
                pauseUI.SetActive(false);
                inGameUI.SetActive(false);
                signUpUI.SetActive(true);
                InputController.gamePaused = true;
                break;

            case GameState.GAME_PLAY:
                startUI.SetActive(false);
                signUpUI.SetActive(false);
                logInUI.SetActive(false);
                connectionErrorUI.SetActive(false);
                pauseUI.SetActive(false);
                inGameUI.SetActive(true);
                InputController.gamePaused = false;
                break;

            default:
                break;
        }
	}

    public void clickedSignUp()
    {
        currentState = GameState.SIGN_UP;
    }

    public void clickedLogIn()
    {
        currentState = GameState.LOG_IN;
    }

    public void goBackToStartScreen()
    {
        currentState = GameState.GAME_START;
    }

    public void pauseGame()
    {
        InputController.gamePaused = true;
        currentState = GameState.GAME_PAUSE;
    }

    public void continueGame()
    {
        currentState = GameState.GAME_PLAY;
    }

    public void exitGame()
    {
        currentState = GameState.GAME_START;
    }

}
