using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState State;
    public static event Action<GameState> GameStateChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
        UpdateGameState(GameState.MapGenerator);
    }
    public void UpdateGameState(GameState newState)
    {
        GameStateChanged?.Invoke(newState);

        State = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.MapGenerator:
                HandleMapGeneration();
                break;
            case GameState.Game:
                HandleGame();
                break;
            case GameState.Victory:
                HandleVictory();
                break;
            case GameState.Death:
                HandleDeath();
                break;
        }
    }

    private void HandleMapGeneration()
    {
        /* During the generator, we need to:
         * 1. Generate the map
         * 2. Place rooms
         * 3. Place enemies
         * 4. Place power ups
         */
    }

    private void HandleDeath()
    {
        // During the death screen, you are able to play again, spectate or quit
    }

    private void HandleVictory()
    {
        // During victory, you can play again or quit
    }

    private void HandleGame()
    {
       //.....
    }

    private void HandleMainMenu()
    {
        /* The main menu constitutes 4 buttons:
         * Play
         * Join
         * Host
         * Quit
        */
    }

    public enum GameState
    {
        MainMenu,
        MapGenerator,
        Game,
        Victory,
        Death
    }
}
