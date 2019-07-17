using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilseyEngine
{
    class ChessBoard
    {
        public ulong WhitePawns
        {
            get; set;
        }
        public ulong WhiteKnights
        {
            get; set;
        }
        public ulong WhiteBishops
        {
            get; set;
        }
        public ulong WhiteRooks
        {
            get; set;
        }
        public ulong WhiteKing
        {
            get; set;
        }
        public ulong WhiteQueen
        {
            get; set;
        }

        public ulong BlackPawns
        {
            get; set;
        }
        public ulong BlackKnights
        {
            get; set;
        }
        public ulong BlackBishops
        {
            get; set;
        }
        public ulong BlackRooks
        {
            get; set;
        }
        public ulong BlackKing
        {
            get; set;
        }
        public ulong BlackQueen
        {
            get; set;
        }

        public ulong AllWhite
        {
            get; set;
        }
        public ulong AllBlack
        {
            get; set;
        }
        public ulong AllPieces
        {
            get; set;
        }

        public bool WhiteKingSide
        {
            get; set;
        }
        public bool WhiteQueenSide
        {
            get; set;
        }
        public bool BlackKingSide
        {
            get; set;
        }
        public bool BlackQueenSide
        {
            get; set;
        }
        public ulong Enpassant
        {
            get; set;
        }

        private ChessBoard last;

        public ChessBoard()
        {
            Initialize();

            WhiteKingSide = true;
            WhiteQueenSide = true;
            BlackKingSide = true;
            BlackQueenSide = true;
            last = null;
            Enpassant = 0;
        }

        public ChessBoard(ChessBoard board)
        {
            InternalCopy(board);
        }

        public void Initialize()
        {
            WhitePawns = 0xFF00;
            WhiteKnights = 0x42;
            WhiteBishops = 0x24;
            WhiteRooks = 0x81;
            WhiteQueen = 0x8;
            WhiteKing = 0x10;

            BlackPawns = 0xFF000000000000;
            BlackKnights = 0x4200000000000000;
            BlackBishops = 0x2400000000000000;
            BlackRooks = 0x8100000000000000;
            BlackQueen = 0x800000000000000;
            BlackKing = 0x1000000000000000;

            Update();
        }

        public void Update()
        {
            AllWhite = WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueen | WhiteKing;
            AllBlack = BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueen | BlackKing;
            AllPieces = AllWhite | AllBlack;
        }

        public List<ulong> BoardList()
        {
            return new List<ulong>() { WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteKing, WhiteQueen,
                                       BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackKing, BlackQueen };
        }

        private void UpdateCastle(ulong whiteking, ulong blackking, ulong whiterooks, ulong blackrooks)
        {
            if (whiteking != WhiteKing)
            {
                WhiteKingSide = false;
                WhiteQueenSide = false;
            }
            if (blackking != BlackKing)
            {
                BlackKingSide = false;
                BlackQueenSide = false;
            }

            ulong whiteks = (ulong)0x1 << 7;
            ulong whiteqs = (ulong)0x1;
            if ((whiterooks ^ whiteks) > whiterooks)
            {
                WhiteKingSide = false;
            }
            if ((whiterooks ^ whiteqs) > whiterooks)
            {
                WhiteQueenSide = false;
            }

            ulong blackks = (ulong)0x1 << 63;
            ulong blackqs = (ulong)0x1 << 56;
            if ((blackrooks ^ blackks) > blackrooks)
            {
                BlackKingSide = false;
            }
            if ((blackrooks ^ blackqs) > blackrooks)
            {
                BlackQueenSide = false;
            }
        }

        public void MakeMove(List<ulong> boardList)
        {
            last = new ChessBoard(this);

            UpdateCastle(boardList[4], boardList[10], boardList[3], boardList[9]);

            WhitePawns = boardList[0];
            WhiteKnights = boardList[1];
            WhiteBishops = boardList[2];
            WhiteRooks = boardList[3];
            WhiteKing = boardList[4];
            WhiteQueen = boardList[5];

            BlackPawns = boardList[6];
            BlackKnights = boardList[7];
            BlackBishops = boardList[8];
            BlackRooks = boardList[9];
            BlackKing = boardList[10];
            BlackQueen = boardList[11];

            Enpassant = boardList[12];

            Update();
        }

        public void UndoLastMove()
        {
            InternalCopy(last);
        }

        private void InternalCopy(ChessBoard board)
        {
            WhitePawns = board.WhitePawns;
            WhiteKnights = board.WhiteKnights;
            WhiteBishops = board.WhiteBishops;
            WhiteRooks = board.WhiteRooks;
            WhiteQueen = board.WhiteQueen;
            WhiteKing = board.WhiteKing;

            BlackPawns = board.BlackPawns;
            BlackKnights = board.BlackKnights;
            BlackBishops = board.BlackBishops;
            BlackRooks = board.BlackRooks;
            BlackQueen = board.BlackQueen;
            BlackKing = board.BlackKing;

            Enpassant = board.Enpassant;
            WhiteKingSide = board.WhiteKingSide;
            WhiteQueenSide = board.WhiteQueenSide;
            BlackKingSide = board.BlackKingSide;
            BlackQueenSide = board.BlackQueenSide;
            last = board.last;

            Update();
        }
    }
}
