using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private Button[] tileList = new Button[9];

    [SerializeField] private TextMeshProUGUI scoreText;

    public static GameController Instance { get; private set; }

    private int[] boardState;
    private bool isGameOver;

    private int player1Score = 0;
    private int player2Score = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        boardState = new int[9];
        isGameOver = false;
    }

    public void OnBoardButtonClicked(int index)
    {
        if (!isGameOver && boardState[index] == 0)
        {
            // Asume que el jugador siempre es X y el oponente siempre es O
            boardState[index] = 1;
            tileList[index].GetComponentInChildren<TextMeshProUGUI>().text = "X";
            CheckGameOver();

            if (isGameOver)
            {
                // Comprueba quién ganó y actualiza el contador correspondiente
                int winner = CheckWinner();
                UpdateScore(winner);

                // Reinicia el tablero
                ResetBoard();
            }

            if (!isGameOver)
            {
                // Aquí puedes enviar el movimiento al servidor
                JSONNode json = new JSONObject();
                json["type"] = "move";
                json["message"] = index;
                SocketManager.gameWS.Send(json.ToString());
            }
        }
    }

    private int CheckWinner()
    {
        // Winning conditions
        int[][] winConditions = new int[][]
        {
        new int[] {0, 1, 2}, // First row
        new int[] {3, 4, 5}, // Second row
        new int[] {6, 7, 8}, // Third row
        new int[] {0, 3, 6}, // First column
        new int[] {1, 4, 7}, // Second column
        new int[] {2, 5, 8}, // Third column
        new int[] {0, 4, 8}, // Diagonal from top left to bottom right
        new int[] {2, 4, 6}  // Diagonal from top right to bottom left
        };

        // Check each winning condition
        foreach (int[] condition in winConditions)
        {
            if (boardState[condition[0]] != 0 && boardState[condition[0]] == boardState[condition[1]] && boardState[condition[0]] == boardState[condition[2]])
            {
                return boardState[condition[0]];
            }
        }

        // If no winner, return 0
        return 0;
    }

    public void HandleOpponentMove(int index)
    {
        if (!isGameOver && boardState[index] == 0)
        {
            boardState[index] = 2;
            tileList[index].GetComponentInChildren<TextMeshProUGUI>().text = "O";
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        // Comprueba las filas
        for (int i = 0; i < 3; i++)
        {
            if (boardState[i * 3] != 0 && boardState[i * 3] == boardState[i * 3 + 1] && boardState[i * 3] == boardState[i * 3 + 2])
            {
                isGameOver = true;
                UpdateScore(boardState[i * 3]);
                ResetBoard();
                return;
            }
        }

        // Comprueba las columnas
        for (int i = 0; i < 3; i++)
        {
            if (boardState[i] != 0 && boardState[i] == boardState[i + 3] && boardState[i] == boardState[i + 6])
            {
                isGameOver = true;
                UpdateScore(boardState[i]);
                ResetBoard();
                return;
            }
        }

        // Comprueba las diagonales
        if (boardState[0] != 0 && boardState[0] == boardState[4] && boardState[0] == boardState[8])
        {
            isGameOver = true;
            UpdateScore(boardState[0]);
            ResetBoard();
            return;
        }
        if (boardState[2] != 0 && boardState[2] == boardState[4] && boardState[2] == boardState[6])
        {
            isGameOver = true;
            UpdateScore(boardState[2]);
            ResetBoard();
            return;
        }

        // Comprueba si el tablero está lleno
        if (!Array.Exists(boardState, element => element == 0))
        {
            isGameOver = true;
            // No hay ganador, por lo que no se actualiza la puntuación
            ResetBoard();
        }
    }


    private void UpdateScore(int winner)
    {
        if (winner == 1)
        {
            player1Score++;
        }
        else if (winner == 2)
        {
            player2Score++;
        }
        scoreText.text = "Player 1: " + player1Score + "  -  Player 2: " + player2Score;
    }

    private void ResetBoard()
    {
        boardState = new int[9];
        isGameOver = false;

        foreach (Button button in tileList)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }
}
