using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager boardManager;
    public UIManager uiManager;

    public Player currentPlayer = Player.Black; // Black starts

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        UpdateUI();
        HighlightLegalMoves();
    }

    public void OnBoardInitialized()
    {
        currentPlayer = Player.Black;
        UpdateUI();
        HighlightLegalMoves();
    }

    public void OnTileClicked(Tile tile)
    {
        if (boardManager == null) return;
        if (!boardManager.IsLegalMove(tile.x, tile.y, currentPlayer)) return;

        bool placed = boardManager.PlacePiece(tile.x, tile.y, currentPlayer);
        if (placed)
        {
            // wait for flip coroutine to call OnMoveCompleted
            // optionally disable input while animating (not shown)
        }
    }

    public void OnMoveCompleted()
    {
        // switch player
        currentPlayer = boardManager.Opponent(currentPlayer);
        UpdateUI();

        // check if opponent has legal moves
        var legal = boardManager.GetLegalMoves(currentPlayer);
        if (legal.Count == 0)
        {
            // pass back
            currentPlayer = boardManager.Opponent(currentPlayer);
            legal = boardManager.GetLegalMoves(currentPlayer);
            if (legal.Count == 0)
            {
                // game over
                OnGameOver();
                return;
            }
            else
            {
                // inform player of pass (UI)
                uiManager.ShowPass();
            }
        }
        HighlightLegalMoves();

        // if next player is AI, call AI here (optional)
        if (uiManager != null) uiManager.UpdateScores(boardManager.CountPieces());
    }

    void HighlightLegalMoves()
    {
        // clear all
        for (int x=0;x<BoardManager.SIZE;x++)
            for (int y=0;y<BoardManager.SIZE;y++)
                boardManager.tiles[x,y].Highlight(false);

        var moves = boardManager.GetLegalMoves(currentPlayer);
        foreach (var m in moves) boardManager.tiles[m.x, m.y].Highlight(true);
    }

    void UpdateUI()
    {
        var (b,w) = boardManager.CountPieces();
        uiManager.UpdateTurn(currentPlayer);
        uiManager.UpdateScores((b,w));
    }

    void OnGameOver()
    {
        var (b,w) = boardManager.CountPieces();
        uiManager.ShowGameOver(b, w);
    }

    // public method for restart
    public void Restart()
    {
        boardManager.InitBoard();
        currentPlayer = Player.Black;
        UpdateUI();
        HighlightLegalMoves();
    }
}
