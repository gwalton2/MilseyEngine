using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilseyEngine
{
    class AI
    {
        const int INFTY = int.MaxValue;
        const int WINSCORE = int.MaxValue - 1;   

        ChessBoard chessboard;
        Board board;
        Game game;
        Game.PieceColor mycolor;
        private int depth;
        private int[] currentmove;
        private bool GameOver;

        public AI(ChessBoard chessboard, Board board, Game game, int depth, Game.PieceColor mycolor)
        {
            this.chessboard = chessboard;
            this.board = board;
            this.game = game;
            this.depth = depth;
            this.mycolor = mycolor;
            GameOver = false;
        }

        public int[] GetMove()
        {
            FindMove(depth, -INFTY, INFTY);
            return currentmove;
        }

        private int FindMove(int depth, int alpha, int beta)
        {
            if (depth == 0 || GameOver)
            {
                return StaticScore();
            }

            int[] best = null;
            int score = 0;
            List<int[]> mymoves = GetMoves(ConvertBoard());

            int bestsofar = int.MinValue;
            foreach (int[] move in mymoves)
            {
                board.MoveBitBoard(move[0], move[1]);
                int response = FindMin(depth - 1, alpha, beta);
                if (response >= bestsofar)
                {
                    best = move;
                    score = StaticScore();
                    bestsofar = response;
                    alpha = Math.Max(alpha, bestsofar);
                    if (beta <= alpha)
                    {
                        board.UndoLastMoveBitBoard();
                        break;
                    }
                }
                board.UndoLastMoveBitBoard();
            }
            currentmove = best;
            return score;
        }

        private int FindMin(int depth, int alpha, int beta)
        {
            if (depth == 0 || GameOver)
            {
                return StaticScore();
            }

            int[] best = null;
            int score = 0;
            List<int[]> mymoves = GetMoves(ConvertBoard());

            int bestsofar = int.MaxValue;
            foreach (int[] move in mymoves)
            {
                board.MoveBitBoard(move[0], move[1]);
                int response = FindMove(depth - 1, alpha, beta);
                if (response <= bestsofar)
                {
                    best = move;
                    score = StaticScore();
                    bestsofar = response;
                    beta = Math.Min(beta, bestsofar);
                    if (beta <= alpha)
                    {
                        board.UndoLastMoveBitBoard();
                        break;
                    }
                }
                board.UndoLastMoveBitBoard();
            }
            return score;
        }

        private int StaticScore()
        {
            if (Win(mycolor))
            {
                GameOver = true;
                return WINSCORE;
            }
            if (Win(game.OppositeColor(mycolor)))
            {
                GameOver = true;
                return -WINSCORE;
            }

            int score = 0;
            List<List<int>> boardList = ConvertBoard();
            List<int[,]> scorecharts = new List<int[,]> {whitepawnscore, whiteknightscore, whitebishopscore, whiterookscore, whitekingmiddlescore, whitequeenscore,
                                                         blackpawnscore, blackknightscore, blackbishopscore, blackrookscore, blackkingmiddlescore, blackqueenscore};

            for (int i = 0; i < 12; i++)
            {
                if (i == 4 && IsEndGame())
                {
                    score += StaticPieceScore(whitekingendscore, boardList[i]);
                }
                else if (i == 10 && IsEndGame())
                {
                    score += StaticPieceScore(blackkingendscore, boardList[i]);
                }
                else
                {
                    score += StaticPieceScore(scorecharts[i], boardList[i]);
                }
            }

            if (board.InCheck(mycolor))
            {
                score -= 100;
            }
            if (board.InCheck(game.OppositeColor(mycolor)))
            {
                score += 100;
            }

            return score;
        }

        private int StaticPieceScore(int[,] scorechart, List<int> pieces)
        {
            int score = 0;
            foreach (int index in pieces)
            {
                score += scorechart[index / 8, index % 8];
            }

            if (game.Turn == mycolor)
            {
                return score;
            }
            return -score;
        }

        public bool IsEndGame()
        {
            int count = 0;
            for (int i = 0; i < 64; i++)
            {
                ulong piece = (ulong)0x1 << i;
                if ((chessboard.AllPieces & piece) != 0)
                {
                    count++;
                }
            }
            return count <= 16;
        }

        private bool Win(Game.PieceColor color)
        {
            if (color == Game.PieceColor.White)
            {
                return chessboard.BlackKing == 0;
            }
            else
            {
                return chessboard.WhiteKing == 0;
            }
        }

        private List<List<int>> ConvertBoard()
        {
            List<List<int>> boardIndexes = new List<List<int>>();
            List<ulong> boardList = chessboard.BoardList();

            foreach (ulong b in boardList)
            {
                boardIndexes.Add(Moves.ConvertBitboard(b));
            }
            return boardIndexes;
        }

        private List<int[]> GetMoves(List<List<int>> boardIndexes)
        {
            if (game.Turn == Game.PieceColor.White)
            {
                boardIndexes = boardIndexes.GetRange(0, 6);
            }
            else
            {
                boardIndexes = boardIndexes.GetRange(6, 6);
            }
            List<int[]> moves = new List<int[]>();

            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case 0:
                        moves.AddRange(GetPawnMoves(boardIndexes[i], game.Turn));
                        break;
                    case 1:
                        moves.AddRange(GetKnightMoves(boardIndexes[i], game.Turn));
                        break;
                    case 2:
                        moves.AddRange(GetBishopMoves(boardIndexes[i], game.Turn));
                        break;
                    case 3:
                        moves.AddRange(GetRookMoves(boardIndexes[i], game.Turn));
                        break;
                    case 4:
                        moves.AddRange(GetKingMoves(boardIndexes[i], game.Turn));
                        break;
                    case 5:
                        moves.AddRange(GetQueenMoves(boardIndexes[i], game.Turn));
                        break;
                }
            }
            return moves;
        }

        private List<int[]> ConvertMove(ulong moves, int selected)
        {
            List<int[]> moveList = new List<int[]>();
            for (int i = 0; i < 64; i++)
            {
                ulong piece = (ulong)0x1 << i;
                if ((moves & piece) != 0)
                {
                    moveList.Add(new int[2] { selected, i });
                }
            }
            return moveList;
        }

        private List<int[]> GetPawnMoves(List<int> indexes, Game.PieceColor color)
        {
            List<int[]> moves = new List<int[]>();
            foreach (int ind in indexes)
            {
                int rank = ind / 8;
                int file = ind % 8;

                if (color == Game.PieceColor.White)
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetWhitePawnMoves(rank, file, chessboard.AllPieces, chessboard.AllBlack, chessboard.Enpassant), ind), ind);
                    moves.AddRange(thesemoves);
                }
                else
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetBlackPawnMoves(rank, file, chessboard.AllPieces, chessboard.AllWhite, chessboard.Enpassant), ind), ind);
                    moves.AddRange(thesemoves);
                }
            }
            return moves;
        }

        private List<int[]> GetKnightMoves(List<int> indexes, Game.PieceColor color)
        {
            List<int[]> moves = new List<int[]>();
            foreach (int ind in indexes)
            {
                int rank = ind / 8;
                int file = ind % 8;

                if (color == Game.PieceColor.White)
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetKnightMoves(rank, file, chessboard.AllWhite), ind), ind);
                    moves.AddRange(thesemoves);
                }
                else
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetKnightMoves(rank, file, chessboard.AllBlack), ind), ind);
                    moves.AddRange(thesemoves);
                }
            }
            return moves;
        }

        private List<int[]> GetBishopMoves(List<int> indexes, Game.PieceColor color)
        {
            List<int[]> moves = new List<int[]>();
            foreach (int ind in indexes)
            {
                int rank = ind / 8;
                int file = ind % 8;

                if (color == Game.PieceColor.White)
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetBishopMoves(rank, file, chessboard.AllPieces, chessboard.AllWhite), ind), ind);
                    moves.AddRange(thesemoves);
                }
                else
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetBishopMoves(rank, file, chessboard.AllPieces, chessboard.AllBlack), ind), ind);
                    moves.AddRange(thesemoves);
                }
            }
            return moves;
        }

        private List<int[]> GetRookMoves(List<int> indexes, Game.PieceColor color)
        {
            List<int[]> moves = new List<int[]>();
            foreach (int ind in indexes)
            {
                int rank = ind / 8;
                int file = ind % 8;

                if (color == Game.PieceColor.White)
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetRookMoves(rank, file, chessboard.AllPieces, chessboard.AllWhite), ind), ind);
                    moves.AddRange(thesemoves);
                }
                else
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetRookMoves(rank, file, chessboard.AllPieces, chessboard.AllBlack), ind), ind);
                    moves.AddRange(thesemoves);
                }
            }
            return moves;
        }

        private List<int[]> GetQueenMoves(List<int> indexes, Game.PieceColor color)
        {
            List<int[]> moves = new List<int[]>();
            foreach (int ind in indexes)
            {
                int rank = ind / 8;
                int file = ind % 8;

                if (color == Game.PieceColor.White)
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetQueenMoves(rank, file, chessboard.AllPieces, chessboard.AllWhite), ind), ind);
                    moves.AddRange(thesemoves);
                }
                else
                {
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(Moves.GetQueenMoves(rank, file, chessboard.AllPieces, chessboard.AllBlack), ind), ind);
                    moves.AddRange(thesemoves);
                }
            }
            return moves;
        }

        private List<int[]> GetKingMoves(List<int> indexes, Game.PieceColor color)
        {
            List<int[]> moves = new List<int[]>();
            foreach (int ind in indexes)
            {
                int rank = ind / 8;
                int file = ind % 8;

                if (color == Game.PieceColor.White)
                {
                    ulong wkmoves = Moves.GetKingMoves(rank, file, chessboard.AllWhite) | Moves.GetWhiteCastleMoves(rank, file, chessboard.AllWhite, board, chessboard);
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(wkmoves, ind), ind);
                    moves.AddRange(thesemoves);
                }
                else
                {
                    ulong bkmoves = Moves.GetKingMoves(rank, file, chessboard.AllBlack) | Moves.GetBlackCastleMoves(rank, file, chessboard.AllBlack, board, chessboard);
                    List<int[]> thesemoves = ConvertMove(board.ClipCheck(bkmoves, ind), ind);
                    moves.AddRange(thesemoves);
                }
            }
            return moves;
        }



        private static readonly int[,] whitepawnscore = {{ 0, 0, 0, 0, 0, 0, 0, 0 },
                                                         { 50, 50, 50, 50, 50, 50, 50, 50 },
                                                         { 10, 10, 20, 30, 30, 20, 10, 10 },
                                                         { 5, 5, 10, 25, 25, 10, 5, 5 },
                                                         { 0, 0, 0, 20, 20, 0, 0, 0 },
                                                         { 5, -5, -10, 0, 0, -10, -5, 5 },
                                                         { 5, 10, 10, -20, -20, 10, 10, 5 },
                                                         { 0, 0, 0, 0, 0, 0, 0, 0 } };

        private static readonly int[,] blackpawnscore = {{ 0, 0, 0, 0, 0, 0, 0, 0 },
                                                          { 5, 10, 10, -20, -20, 10, 10, 5 },
                                                          { 5, -5, -10, 0, 0, -10, -5, 5 },
                                                          { 0, 0, 0, 20, 20, 0, 0, 0 },
                                                          { 5, 5, 10, 25, 25, 10, 5, 5 },
                                                          { 10, 10, 20, 30, 30, 20, 10, 10 },
                                                          { 50, 50, 50, 50, 50, 50, 50, 50 },
                                                          { 0, 0, 0, 0, 0, 0, 0, 0 }};

        private static readonly int[,] whiteknightscore = {{-50, -40, -30, -30, -30, -30, -40, -50 },
                                                            {-40,-20,  0,  0,  0,  0,-20,-40},
                                                            {-30,  0, 10, 15, 15, 10,  0,-30},
                                                            {-30,  5, 15, 20, 20, 15,  5,-30},
                                                            {-30,  0, 15, 20, 20, 15,  0,-30},
                                                            {-30,  5, 10, 15, 15, 10,  5,-30},
                                                            {-40,-20,  0,  5,  5,  0,-20,-40},
                                                            {-50,-40,-30,-30,-30,-30,-40,-50}};

        private static readonly int[,] blackknightscore = {{-50, -40, -30, -30, -30, -30, -40, -50 },
                                                            {-40,-20,  0,  5,  5,  0,-20,-40},
                                                            {-30,  5, 10, 15, 15, 10,  5,-30},
                                                            {-30,  0, 15, 20, 20, 15,  0,-30},
                                                            {-30,  5, 15, 20, 20, 15,  5,-30},
                                                            {-30,  0, 10, 15, 15, 10,  0,-30},
                                                            {-40,-20,  0,  0,  0,  0,-20,-40},
                                                            {-50,-40,-30,-30,-30,-30,-40,-50}};

        private static readonly int[,] whitebishopscore = { { -20,-10,-10,-10,-10,-10,-10,-20 },
                                                            { -10,  0,  0,  0,  0,  0,  0,-10 },
                                                            { -10,  0,  5, 10, 10,  5,  0,-10 },
                                                            { -10,  5,  5, 10, 10,  5,  5,-10 },
                                                            { -10,  0, 10, 10, 10, 10,  0,-10 },
                                                            { -10, 10, 10, 10, 10, 10, 10,-10 },
                                                            { -10,  5,  0,  0,  0,  0,  5,-10 },
                                                            { -20,-10,-10,-10,-10,-10,-10,-20 } };

        private static readonly int[,] blackbishopscore = { { -20,-10,-10,-10,-10,-10,-10,-20 },
                                                            { -10,  5,  0,  0,  0,  0,  5,-10 },
                                                            { -10, 10, 10, 10, 10, 10, 10,-10 },
                                                            { -10,  0, 10, 10, 10, 10,  0,-10 },
                                                            { -10,  5,  5, 10, 10,  5,  5,-10},
                                                            { -10,  0,  5, 10, 10,  5,  0,-10 },
                                                            { -10,  0,  0,  0,  0,  0,  0,-10 },
                                                            { -20,-10,-10,-10,-10,-10,-10,-20 } };

        private static readonly int[,] whiterookscore = { { 0, 0, 0, 0, 0, 0, 0, 0, },
                                                          {5, 10, 10, 10, 10, 10, 10,  5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                          {0,  0,  0,  5,  5,  0,  0,  0} };

        private static readonly int[,] blackrookscore = { { 0,  0,  0,  5,  5,  0,  0,  0 },
                                                          {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {-5,  0,  0,  0,  0,  0,  0, -5},
                                                         {5, 10, 10, 10, 10, 10, 10,  5},
                                                          {0,  0,  0,  0,  0,  0,  0,  0} };

        private static readonly int[,] whitequeenscore = { { -20,-10,-10, -5, -5,-10,-10,-20 },
                                                            { -10,  0,  0,  0,  0,  0,  0,-10 },
                                                            { -10,  0,  5,  5,  5,  5,  0,-10 },
                                                            { -5,  0,  5,  5,  5,  5,  0, -5 },
                                                            { 0,  0,  5,  5,  5,  5,  0, -5 },
                                                            { -10,  5,  5,  5,  5,  5,  0,-10 },
                                                            { -10,  0,  5,  0,  0,  0,  0,-10 },
                                                            { -20,-10,-10, -5, -5,-10,-10,-20} };

        private static readonly int[,] blackqueenscore = { { -20,-10,-10, -5, -5,-10,-10,-20 },
                                                            { -10,  0,  5,  0,  0,  0,  0,-10 },
                                                            { -10,  0,  5,  5,  5,  5,  5,-10 },
                                                            { -5,  0,  5,  5,  5,  5,  0, 0 },
                                                            { -5,  0,  5,  5,  5,  5,  0, -5 },
                                                            { -10,  0,  5,  5,  5,  5,  0,-10 },
                                                            { -10,  0,  0,  0,  0,  0,  0,-10 },
                                                            { -20,-10,-10, -5, -5,-10,-10,-20} };

        private static readonly int[,] whitekingmiddlescore = { { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                { -20,-30,-30,-40,-40,-30,-30,-20 },
                                                                { -10,-20,-20,-20,-20,-20,-20,-10 },
                                                                 { 20, 20,  0,  0,  0,  0, 20, 20 },
                                                                 { 20, 30, 10,  0,  0, 10, 30, 20} };

        private static readonly int[,] blackkingmiddlescore = { { 20, 30, 10,  0,  0, 10, 30, 20 },
                                                                { 20, 20,  0,  0,  0,  0, 20, 20 },
                                                                { -10,-20,-20,-20,-20,-20,-20,-10 },
                                                                { -20,-30,-30,-40,-40,-30,-30,-20 },
                                                                { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                 { -30,-40,-40,-50,-50,-40,-40,-30 },
                                                                 { -30,-40,-40,-50,-50,-40,-40,-30} };

        private static readonly int[,] whitekingendscore = { { -50,-40,-30,-20,-20,-30,-40,-50 },
                                                            { -30,-20,-10,  0,  0,-10,-20,-30 },
                                                            { -30,-10, 20, 30, 30, 20,-10,-30 },
                                                            { -30,-10, 30, 40, 40, 30,-10,-30 },
                                                            { -30,-10, 30, 40, 40, 30,-10,-30 },
                                                            { -30,-10, 20, 30, 30, 20,-10,-30 },
                                                            { -30,-30,  0,  0,  0,  0,-30,-30 },
                                                            { -50,-30,-30,-30,-30,-30,-30,-50} };

        private static readonly int[,] blackkingendscore = { { -50,-30,-30,-30,-30,-30,-30,-50 },
                                                            { -30,-30,  0,  0,  0,  0,-30,-30 },
                                                            { -30,-10, 20, 30, 30, 20,-10,-30 },
                                                            { -30,-10, 30, 40, 40, 30,-10,-30 },
                                                            { -30,-10, 30, 40, 40, 30,-10,-30 },
                                                            { -30,-10, 20, 30, 30, 20,-10,-30 },
                                                            { -30,-20,-10,  0,  0,-10,-20,-30 },
                                                            { -50,-40,-30,-20,-20,-30,-40,-50} };
    }
}
