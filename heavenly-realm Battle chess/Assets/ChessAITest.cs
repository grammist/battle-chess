using UnityEngine;
using System.Collections.Generic;

public class ChessAI : MonoBehaviour
{
    private static int DEPTH = 4; // AI search depth
    private static int CHECKMATE = 1000;
    private static int STALEMATE = 0;

    private static Dictionary<string, int> pieceScore = new Dictionary<string, int>
    {
        { "K", 0 }, { "Q", 9 }, { "R", 5 }, { "B", 3 }, { "N", 3 }, { "p", 1 }
    };

    // Adjusted piece position scores to avoid repetitive movements
    private static int[,] whiteKnightScores = {
        {-50,-40,-30,-30,-30,-30,-40,-50 },
        {-40,-20,  0,  5,  5,  0,-20,-40 },
        {-30,  5, 10, 15, 15, 10,  5,-30 },
        {-30,  0, 15, 20, 20, 15,  0,-30 },
        {-30,  5, 15, 20, 20, 15,  5,-30 },
        {-30,  0, 10, 15, 15, 10,  0,-30 },
        {-40,-20,  0,  0,  0,  0,-20,-40 },
        {-50,-40,-30,-30,-30,-30,-40,-50 }
    };
    private static int[,] blackKnightScores = {
        {-50,-40,-30,-30,-30,-30,-40,-50 },
        {-40,-20,  0,  5,  5,  0,-20,-40 },
        {-30,  5, 10, 15, 15, 10,  5,-30 },
        {-30,  0, 15, 20, 20, 15,  0,-30 },
        {-30,  5, 15, 20, 20, 15,  5,-30 },
        {-30,  0, 10, 15, 15, 10,  0,-30 },
        {-40,-20,  0,  0,  0,  0,-20,-40 },
        {-50,-40,-30,-30,-30,-30,-40,-50 }
    };

    private static int[,] whiteBishopScores = {
        {-20,-10,-10,-10,-10,-10,-10,-20 },
        {-10,  5,  0,  0,  0,  0,  5,-10 },
        {-10, 10, 10, 10, 10, 10, 10,-10 },
        {-10,  0, 10, 10, 10, 10,  0,-10 },
        {-10,  5,  5, 10, 10,  5,  5,-10 },
        {-10,  0,  5, 10, 10,  5,  0,-10 },
        {-10,  0,  0,  0,  0,  0,  0,-10 },
        {-20,-10,-10,-10,-10,-10,-10,-20 }
    };
    private static int[,] blackBishopScores = {
        {-20,-10,-10,-10,-10,-10,-10,-20 },
        {-10,  0,  0,  0,  0,  0,  0,-10 },
        {-10,  0,  5, 10, 10,  5,  0,-10 },
        {-10,  5,  5, 10, 10,  5,  5,-10 },
        {-10,  0, 10, 10, 10, 10,  0,-10 },
        {-10, 10, 10, 10, 10, 10, 10,-10 },
        {-10,  5,  0,  0,  0,  0,  5,-10 },
        {-20,-10,-10,-10,-10,-10,-10,-20 }
    };

    private static int[,] whiteQueenScores = {
        {-20,-10,-10, -5, -5,-10,-10,-20 },
        {-10,  0,  5,  0,  0,  0,  0,-10 },
        {-10,  5,  5,  5,  5,  5,  0,-10 },
        {  0,  0,  5,  5,  5,  5,  0, -5 },
        { -5,  0,  5,  5,  5,  5,  0, -5 },
        {-10,  0,  5,  5,  5,  5,  0,-10 },
        {-10,  0,  0,  0,  0,  0,  0,-10 },
        {-20,-10,-10, -5, -5,-10,-10,-20 }
    };
    private static int[,] blackQueenScores = {
        {-20,-10,-10, -5, -5,-10,-10,-20 },
        {-10,  0,  0,  0,  0,  0,  0,-10 },
        {-10,  0,  5,  5,  5,  5,  0,-10 },
        { -5,  0,  5,  5,  5,  5,  0, -5 },
        {  0,  0,  5,  5,  5,  5,  0, -5 },
        {-10,  0,  5,  5,  5,  5,  0,-10 },
        {-10,  0,  5,  0,  0,  0,  0,-10 },
        {-20,-10,-10, -5, -5,-10,-10,-20 }
    };

    private static int[,] whiteRookScores = {
        { 0,  0,  0,  5,  5,  0,  0,  0 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        { 5, 10, 10, 10, 10, 10, 10,  5 },
        { 0,  0,  0,  0,  0,  0,  0,  0 }
    };
    private static int[,] blackRookScores = {
        { 0,  0,  0,  0,  0,  0,  0,  0 },
        { 5, 10, 10, 10, 10, 10, 10,  5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        {-5,  0,  0,  0,  0,  0,  0, -5 },
        { 0,  0,  0,  5,  5,  0,  0,  0 }
    };

    private static int[,] whitePawnScores = {
    { 0,  0,  0,  0,  0,  0,  0,  0 },
    { 5, 10, 10,-20,-20, 10, 10,  5 },
    { 5, -5,-10,  0,  0,-10, -5,  5 },
    { 0,  0,  0, 20, 20,  0,  0,  0 },
    { 5,  5, 10, 25, 25, 10,  5,  5 },
    {10, 10, 20, 30, 30, 20, 10, 10 },
    {80, 80, 80, 80, 80, 80, 80, 80 }, // 提高前排兵的移动奖励
    { 0,  0,  0,  0,  0,  0,  0,  0 }
};

private static int[,] blackPawnScores = {
    { 0,  0,  0,  0,  0,  0,  0,  0 },
    {80, 80, 80, 80, 80, 80, 80, 80 }, // 提高前排兵的移动奖励
    {10, 10, 20, 30, 30, 20, 10, 10 },
    { 5,  5, 10, 25, 25, 10,  5,  5 },
    { 0,  0,  0, 20, 20,  0,  0,  0 },
    { 5, -5,-10,  0,  0,-10, -5,  5 },
    { 5, 10, 10,-20,-20, 10, 10,  5 },
    { 0,  0,  0,  0,  0,  0,  0,  0 }
};
private static int[,] whiteKingScores = {
    { 20, 30, 10,  0,  0, 10, 30, 20 },
    { 20, 20,  0,  0,  0,  0, 20, 20 },
    {-10,-20,-20,-20,-20,-20,-20,-10 },
    {-20,-30,-30,-40,-40,-30,-30,-20 },
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-30,-40,-40,-50,-50,-40,-40,-30 }
};
private static int[,] blackKingScores = {
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-30,-40,-40,-50,-50,-40,-40,-30 },
    {-20,-30,-30,-40,-40,-30,-30,-20 },
    {-10,-20,-20,-20,-20,-20,-20,-10 },
    { 20, 20,  0,  0,  0,  0, 20, 20 },
    { 20, 30, 10,  0,  0, 10, 30, 20 }
};

    private static Dictionary<string, int[,]> piecePositionScores = new Dictionary<string, int[,]>
{
    { "wN", whiteKnightScores }, // White Knight
    { "bN", blackKnightScores }, // Black Knight
    { "wB", whiteBishopScores }, // White Bishop
    { "bB", blackBishopScores }, // Black Bishop
    { "wQ", whiteQueenScores },  // White Queen
    { "bQ", blackQueenScores },  // Black Queen
    { "wR", whiteRookScores },   // White Rook
    { "bR", blackRookScores },   // Black Rook
    { "wK", whiteKingScores },   // White King
    { "bK", blackKingScores },   // Black King
    { "wp", whitePawnScores },   // White Pawn
    { "bp", blackPawnScores }    // Black Pawn
};

    void Start()
    {
        // Initialize the board
        string[,] board = new string[8, 8]
        {
            { "bR", "bN", "bB", "bQ", "bK", "bB", "bN", "bR" },
            { "bp", "bp", "bp", "bp", "bp", "bp", "bp", "bp" },
            { "--", "--", "--", "--", "--", "--", "--", "--" },
            { "--", "--", "--", "--", "--", "--", "--", "--" },
            { "--", "--", "--", "--", "--", "--", "--", "--" },
            { "--", "--", "--", "--", "--", "--", "--", "--" },
            { "wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp" },
            { "wR", "wN", "wB", "wQ", "wK", "wB", "wN", "wR" }
        };

        // Print initial board
        Debug.Log("Initial Board:");
        PrintBoard(board);

        // Alternating moves for 50 turns
        bool isWhiteTurn = true;
        for (int i = 0; i < 50; i++)
        {
            Debug.Log($"\nTurn {i + 1}: {(isWhiteTurn ? "White" : "Black")}'s move");
            board = FindBestMove(board, isWhiteTurn);
            PrintBoard(board);
            isWhiteTurn = !isWhiteTurn; // Switch turns
        }
    }

    public static string[,] FindBestMove(string[,] board, bool isWhiteTurn)
    {
        // Get all valid moves for the current player
        var validMoves = GetValidMoves(board, isWhiteTurn);

        // Find the best move using Alpha-Beta pruning
        var result = FindMoveNegaMaxAlphaBeta(board, validMoves, DEPTH, -CHECKMATE, CHECKMATE, isWhiteTurn ? 1 : -1, isWhiteTurn);

        // Extract the best move from the result
        var bestMove = result.move;

        // Apply the best move and return the updated board
        return ApplyMove(board, bestMove);
    }

    private static (int score, (int, int, int, int) move) FindMoveNegaMaxAlphaBeta(string[,] board, List<(int, int, int, int)> validMoves, int depth, int alpha, int beta, int turnMultiplier, bool isWhiteTurn)
    {
        if (depth == 0 || validMoves.Count == 0)
        {
            int score = ScoreBoard(board, isWhiteTurn) * turnMultiplier;
            return (score, (0, 0, 0, 0));
        }

        (int, int, int, int) bestMove = (0, 0, 0, 0);
        int maxScore = -CHECKMATE;

        foreach (var move in validMoves)
        {
            var newBoard = ApplyMove(board, move);
            var nextMoves = GetValidMoves(newBoard, !isWhiteTurn);
            int score = -FindMoveNegaMaxAlphaBeta(newBoard, nextMoves, depth - 1, -beta, -alpha, -turnMultiplier, !isWhiteTurn).score;

            if (score > maxScore)
            {
                maxScore = score;
                bestMove = move;
            }

            if (maxScore > alpha)
            {
                alpha = maxScore;
            }

            if (alpha >= beta)
            {
                break;
            }
        }

        return (maxScore, bestMove);
    }

    private static string[,] ApplyMove(string[,] board, (int, int, int, int) move)
    {
        var newBoard = (string[,])board.Clone();
        newBoard[move.Item3, move.Item4] = newBoard[move.Item1, move.Item2];
        newBoard[move.Item1, move.Item2] = "--";

        // Check if a pawn reached the last row and promote it to a queen
        if (newBoard[move.Item3, move.Item4][1] == 'p' && (move.Item3 == 0 || move.Item3 == 7))
        {
            newBoard[move.Item3, move.Item4] = newBoard[move.Item3, move.Item4][0] + "Q";
        }

        return newBoard;
    }

    private static int ScoreBoard(string[,] board, bool isWhiteTurn)
    {
        int score = 0;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                string square = board[row, col];
                if (square != "--")
                {
                    int piecePositionScore = 0;
                    if (square[1] != 'K') // Ignore King's position score
                    {
                        string key = square; // Use the full piece code (e.g., "wN", "bB")
                        piecePositionScore = piecePositionScores[key][row, col];
                    }

                    // Base score: piece value + position score
                    int baseScore = pieceScore[square.Substring(1)] + piecePositionScore;

                    // Check if the piece is under attack (potential capture)
                    bool isUnderAttack = IsSquareUnderAttack(board, row, col, !isWhiteTurn);
                    if (isUnderAttack)
                    {
                        // Penalize if the piece is under attack
                        baseScore -= pieceScore[square.Substring(1)] / 2; // Reduce value by half
                    }

                    // Add to total score
                    if (square[0] == 'w') // White piece
                    {
                        score += baseScore;
                    }
                    else if (square[0] == 'b') // Black piece
                    {
                        score -= baseScore;
                    }
                }
            }
        }

        return isWhiteTurn ? score : -score;
    }

    private static bool IsSquareUnderAttack(string[,] board, int row, int col, bool isWhiteAttacker)
    {
        // Check for attacks by pawns
        int pawnDirection = isWhiteAttacker ? -1 : 1;
        if (row + pawnDirection >= 0 && row + pawnDirection < 8)
        {
            if (col - 1 >= 0 && board[row + pawnDirection, col - 1] == (isWhiteAttacker ? "wp" : "bp"))
                return true;
            if (col + 1 < 8 && board[row + pawnDirection, col + 1] == (isWhiteAttacker ? "wp" : "bp"))
                return true;
        }

        // Check for attacks by knights
        int[] knightDx = { 2, 1, -1, -2, -2, -1, 1, 2 };
        int[] knightDy = { 1, 2, 2, 1, -1, -2, -2, -1 };
        for (int i = 0; i < 8; i++)
        {
            int newRow = row + knightDx[i];
            int newCol = col + knightDy[i];
            if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string piece = board[newRow, newCol];
                if (piece == (isWhiteAttacker ? "wN" : "bN"))
                    return true;
            }
        }

        // Check for attacks by bishops, rooks, and queens
        int[] bishopDx = { 1, 1, -1, -1 };
        int[] bishopDy = { 1, -1, 1, -1 };
        int[] rookDx = { 1, -1, 0, 0 };
        int[] rookDy = { 0, 0, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            // Check bishop and queen attacks
            int newRow = row + bishopDx[i];
            int newCol = col + bishopDy[i];
            while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string piece = board[newRow, newCol];
                if (piece != "--")
                {
                    if (piece == (isWhiteAttacker ? "wB" : "bB") || piece == (isWhiteAttacker ? "wQ" : "bQ"))
                        return true;
                    break;
                }
                newRow += bishopDx[i];
                newCol += bishopDy[i];
            }

            // Check rook and queen attacks
            newRow = row + rookDx[i];
            newCol = col + rookDy[i];
            while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string piece = board[newRow, newCol];
                if (piece != "--")
                {
                    if (piece == (isWhiteAttacker ? "wR" : "bR") || piece == (isWhiteAttacker ? "wQ" : "bQ"))
                        return true;
                    break;
                }
                newRow += rookDx[i];
                newCol += rookDy[i];
            }
        }

        (int score, (int, int, int, int) move) FindMoveNegaMaxAlphaBeta(string[,] board, List<(int, int, int, int)> validMoves, int depth, int alpha, int beta, int turnMultiplier, bool isWhiteTurn)
        {
            if (depth == 0 || validMoves.Count == 0)
            {
                int score = ScoreBoard(board, isWhiteTurn) * turnMultiplier;
                return (score, (0, 0, 0, 0));
            }

            (int, int, int, int) bestMove = (0, 0, 0, 0);
            int maxScore = -CHECKMATE;

            foreach (var move in validMoves)
            {
                var newBoard = ApplyMove(board, move);
                var nextMoves = GetValidMoves(newBoard, !isWhiteTurn);

                // Check if the move captures a piece
                string capturedPiece = board[move.Item3, move.Item4];
                int captureBonus = capturedPiece != "--" ? pieceScore[capturedPiece.Substring(1)] : 0;

                int score = -FindMoveNegaMaxAlphaBeta(newBoard, nextMoves, depth - 1, -beta, -alpha, -turnMultiplier, !isWhiteTurn).score;
                score += captureBonus; // Add capture bonus

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }

                if (maxScore > alpha)
                {
                    alpha = maxScore;
                }

                if (alpha >= beta)
                {
                    break;
                }
            }

            return (maxScore, bestMove);
        }

        // Check for attacks by king
        int[] kingDx = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] kingDy = { 1, 0, -1, 1, -1, 1, 0, -1 };
        for (int i = 0; i < 8; i++)
        {
            int newRow = row + kingDx[i];
            int newCol = col + kingDy[i];
            if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string piece = board[newRow, newCol];
                if (piece == (isWhiteAttacker ? "wK" : "bK"))
                    return true;
            }
        }

        return false;
    }

    private static List<(int, int, int, int)> GetValidMoves(string[,] board, bool isWhiteTurn)
    {
        List<(int, int, int, int)> validMoves = new List<(int, int, int, int)>();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                string piece = board[row, col];
                if (piece != "--" && (piece[0] == 'w' && isWhiteTurn || piece[0] == 'b' && !isWhiteTurn))
                {
                    switch (piece[1])
                    {
                        case 'p': // Pawn
                            validMoves.AddRange(GetPawnMoves(board, row, col, isWhiteTurn));
                            break;
                        case 'N': // Knight
                            validMoves.AddRange(GetKnightMoves(board, row, col, isWhiteTurn));
                            break;
                        case 'B': // Bishop
                            validMoves.AddRange(GetBishopMoves(board, row, col, isWhiteTurn));
                            break;
                        case 'R': // Rook
                            validMoves.AddRange(GetRookMoves(board, row, col, isWhiteTurn));
                            break;
                        case 'Q': // Queen
                            validMoves.AddRange(GetQueenMoves(board, row, col, isWhiteTurn));
                            break;
                        case 'K': // King
                            validMoves.AddRange(GetKingMoves(board, row, col, isWhiteTurn));
                            break;
                    }
                }
            }
        }

        return validMoves;
    }

    private static List<(int, int, int, int)> GetPawnMoves(string[,] board, int row, int col, bool isWhiteTurn)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        int direction = isWhiteTurn ? -1 : 1; // White pawns move up, black pawns move down

        // Force pawn to move two steps on its first move
        if ((isWhiteTurn && row == 6) || (!isWhiteTurn && row == 1))
        {
            if (board[row + 2 * direction, col] == "--" && board[row + direction, col] == "--")
            {
                moves.Add((row, col, row + 2 * direction, col));
                return moves; // Only allow the double move on the first move
            }
        }

        // Move forward
        if (row + direction >= 0 && row + direction < 8 && board[row + direction, col] == "--")
        {
            moves.Add((row, col, row + direction, col));
        }

        // Capture diagonally
        if (row + direction >= 0 && row + direction < 8 && col - 1 >= 0 && board[row + direction, col - 1][0] == (isWhiteTurn ? 'b' : 'w'))
        {
            moves.Add((row, col, row + direction, col - 1));
        }
        if (row + direction >= 0 && row + direction < 8 && col + 1 < 8 && board[row + direction, col + 1][0] == (isWhiteTurn ? 'b' : 'w'))
        {
            moves.Add((row, col, row + direction, col + 1));
        }

        return moves;
    }

    private static List<(int, int, int, int)> GetKnightMoves(string[,] board, int row, int col, bool isWhiteTurn)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        int[] dx = { 2, 1, -1, -2, -2, -1, 1, 2 };
        int[] dy = { 1, 2, 2, 1, -1, -2, -2, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newRow = row + dx[i];
            int newCol = col + dy[i];
            if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string targetPiece = board[newRow, newCol];
                if (targetPiece == "--" || targetPiece[0] == (isWhiteTurn ? 'b' : 'w'))
                {
                    moves.Add((row, col, newRow, newCol));
                }
            }
        }

        return moves;
    }

    private static List<(int, int, int, int)> GetBishopMoves(string[,] board, int row, int col, bool isWhiteTurn)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        int[] dx = { 1, 1, -1, -1 };
        int[] dy = { 1, -1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = row + dx[i];
            int newCol = col + dy[i];
            while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string targetPiece = board[newRow, newCol];
                if (targetPiece == "--")
                {
                    moves.Add((row, col, newRow, newCol));
                }
                else
                {
                    if (targetPiece[0] == (isWhiteTurn ? 'b' : 'w'))
                    {
                        moves.Add((row, col, newRow, newCol));
                    }
                    break;
                }
                newRow += dx[i];
                newCol += dy[i];
            }
        }

        return moves;
    }

    private static List<(int, int, int, int)> GetRookMoves(string[,] board, int row, int col, bool isWhiteTurn)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = row + dx[i];
            int newCol = col + dy[i];
            while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string targetPiece = board[newRow, newCol];
                if (targetPiece == "--")
                {
                    moves.Add((row, col, newRow, newCol));
                }
                else
                {
                    if (targetPiece[0] == (isWhiteTurn ? 'b' : 'w'))
                    {
                        moves.Add((row, col, newRow, newCol));
                    }
                    break;
                }
                newRow += dx[i];
                newCol += dy[i];
            }
        }

        return moves;
    }

    private static List<(int, int, int, int)> GetQueenMoves(string[,] board, int row, int col, bool isWhiteTurn)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        moves.AddRange(GetBishopMoves(board, row, col, isWhiteTurn));
        moves.AddRange(GetRookMoves(board, row, col, isWhiteTurn));
        return moves;
    }

    private static List<(int, int, int, int)> GetKingMoves(string[,] board, int row, int col, bool isWhiteTurn)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        int[] dx = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] dy = { 1, 0, -1, 1, -1, 1, 0, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newRow = row + dx[i];
            int newCol = col + dy[i];
            if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                string targetPiece = board[newRow, newCol];
                if (targetPiece == "--" || targetPiece[0] == (isWhiteTurn ? 'b' : 'w'))
                {
                    moves.Add((row, col, newRow, newCol));
                }
            }
        }

        return moves;
    }

    private static void PrintBoard(string[,] board)
    {
        string boardString = "";
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                boardString += board[row, col] + " ";
            }
            boardString += "\n";
        }
        Debug.Log(boardString);
    }
}