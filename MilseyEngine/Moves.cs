using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilseyEngine
{
    class Moves
    {
        static ulong[] rankmasks = { 0xFF, 0xFF00, 0xFF0000, 0xFF000000, 0xFF00000000, 0xFF0000000000,
                                     0xFF000000000000, 0xFF00000000000000 }; //From bottom to top

        static ulong[] filemasks = { 0x101010101010101, 0x202020202020202, 0x404040404040404, 0x808080808080808,
                                     0x1010101010101010, 0x2020202020202020, 0x4040404040404040, 0x8080808080808080 }; //From left to right

        static ulong[] diagonalmasks = { 0x100000000000000, 0x201000000000000, 0x402010000000000, 0x804020100000000,
                                         0x1008040201000000, 0x2010080402010000, 0x4020100804020100, 0x8040201008040201,
                                         0x80402010080402, 0x804020100804, 0x8040201008, 0x80402010, 0x804020, 0x8040, 0x80 }; //Top left to bottom right corner

        static ulong[] adiagonalmasks = { 0x8000000000000000, 0x4080000000000000, 0x2040800000000000, 0x1020408000000000,
                                          0x810204080000000, 0x408102040800000, 0x204081020408000, 0x102040810204080,
                                          0x1020408102040, 0x10204081020, 0x102040810, 0x1020408, 0x10204, 0x102, 0x1 }; //Top right to bottom left corner

        static ulong clearA = 0xFEFEFEFEFEFEFEFE;
        static ulong clearB = 0xFDFDFDFDFDFDFDFD;
        static ulong clearH = 0x7F7F7F7F7F7F7F7F;
        static ulong clearG = 0xBFBFBFBFBFBFBFBF;

        public static void DrawBitboard(ulong bitBoard)
        {
            string[,] chessBoard = new string[8, 8];
            for (int i = 0; i < 64; i++)
            {
                chessBoard[i / 8, i % 8] = "";
            }
            for (int i = 0; i < 64; i++)
            {
                if (((bitBoard >> i) & 1) == 1)
                {
                    chessBoard[i / 8, i % 8] = "1";
                }
                else
                {
                    chessBoard[i / 8, i % 8] = "0";
                }
            }
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    Console.WriteLine();
                }
                Console.Write(chessBoard[i / 8, i % 8]);
            }
        }

        public static ulong GetMoveBitboard(int og_index, int new_index)
        {
            ulong og_piece = (ulong)0x1 << og_index;
            ulong new_piece = (ulong)0x1 << new_index;

            return og_piece | new_piece;
        }

        public static List<int> ConvertBitboard(ulong bitboard)
        {
            List<int> move_indexes = new List<int>();

            int counter = 0;
            while (bitboard > 0)
            {
                ulong square = bitboard & 1;
                if (square == 1)
                {
                    move_indexes.Add(counter);
                }
                counter++;
                bitboard >>= 1;
            }
            return move_indexes;
        }

        public static ulong Reverse(ulong orig)
        {
            ulong rev = 0;
            for (int i = 0; i < 64; i++)
            {
                rev <<= 1;
                rev ^= (orig & 1);
                orig >>= 1;
            }
            return rev;
        }

        public static ulong SetEnPassant(int i, int og_index, int new_index)
        {
            ulong old_piece = (ulong)0x1 << og_index;
            ulong new_piece = (ulong)0x1 << new_index;

            if (i == 0) //WhitePawns
            {
                if ((rankmasks[1] & old_piece) != 0 && (rankmasks[3] & new_piece) != 0)
                {
                    return new_piece >> 8;
                }
            }
            else if (i == 6) //BlackPawns
            {
                if ((rankmasks[6] & old_piece) != 0 && (rankmasks[4] & new_piece) != 0)
                {
                    return new_piece << 8;
                }
            }
            return 0;
        }

        public static ulong GetEnPassant(int new_index, ChessBoard board)
        {
            ulong new_piece = (ulong)0x1 << new_index;
            if (board.Enpassant == new_piece)
            {
                if ((board.Enpassant & rankmasks[2]) != 0)
                {
                    return board.Enpassant << 8;
                }
                else
                {
                    return board.Enpassant >> 8;
                }
            }
            else
            {
                return 0;
            }
        }

        public static ulong GetAllBlackMoves(ChessBoard chessboard) //Needs to be fixed to use convertbitboard method
        {
            ulong kingmoves = 0;
            ulong king = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((king & chessboard.BlackKing) != 0)
                {
                    kingmoves = GetKingMoves(i / 8, i % 8, chessboard.AllBlack);
                    break;
                }
                king <<= 1;
            }

            ulong queenmoves = 0;
            ulong queen = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((queen & chessboard.BlackQueen) != 0)
                {
                    queenmoves = GetQueenMoves(i / 8, i % 8, chessboard.AllPieces, chessboard.AllBlack);
                    break;
                }
                queen <<= 1;
            }

            return GetAllBlackPawnMoves(chessboard.BlackPawns, chessboard.AllPieces, chessboard.AllWhite, chessboard.Enpassant) | GetAllBishopMoves(chessboard.BlackBishops, chessboard.AllPieces, chessboard.AllBlack)
                | GetAllRookMoves(chessboard.BlackRooks, chessboard.AllPieces, chessboard.AllBlack) | GetAllKnightMoves(chessboard.BlackKnights, chessboard.AllBlack)
                | kingmoves | queenmoves;
        }

        public static ulong GetAllWhiteMoves(ChessBoard chessboard) //Needs to be fixed to use convertbitboard method
        {
            ulong kingmoves = 0;
            ulong king = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((king & chessboard.WhiteKing) != 0)
                {
                    kingmoves = GetKingMoves(i / 8, i % 8, chessboard.AllWhite);
                    break;
                }
                king <<= 1;
            }

            ulong queenmoves = 0;
            ulong queen = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((queen & chessboard.WhiteQueen) != 0)
                {
                    queenmoves = GetQueenMoves(i / 8, i % 8, chessboard.AllPieces, chessboard.AllWhite);
                    break;
                }
                queen <<= 1;
            }

            return GetAllWhitePawnMoves(chessboard.WhitePawns, chessboard.AllPieces, chessboard.AllBlack, chessboard.Enpassant) | GetAllBishopMoves(chessboard.WhiteBishops, chessboard.AllPieces, chessboard.AllWhite)
                | GetAllRookMoves(chessboard.WhiteRooks, chessboard.AllPieces, chessboard.AllWhite) | GetAllKnightMoves(chessboard.WhiteKnights, chessboard.AllWhite)
                | kingmoves | queenmoves;
        }

        public static ulong GetAllWhitePawnMoves(ulong pawns, ulong occupied, ulong othercolor, ulong enpassant)
        {
            ulong moves = 0;
            ulong piece = 1;
            for (int i = 0; i < 64; i++) //Performance can be improved with counter
            {
                if ((piece & pawns) != 0)
                {
                    moves |= GetWhitePawnMoves(i / 8, i % 8, occupied, othercolor, enpassant);
                }
                piece <<= 1;
            }
            return moves;
        }

        public static ulong GetAllBlackPawnMoves(ulong pawns, ulong occupied, ulong othercolor, ulong enpassant)
        {
            ulong moves = 0;
            ulong piece = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((piece & pawns) != 0)
                {
                    moves |= GetBlackPawnMoves(i / 8, i % 8, occupied, othercolor, enpassant);
                }
                piece <<= 1;
            }
            return moves;
        }

        public static ulong GetAllBishopMoves(ulong bishops, ulong occupied, ulong piececolor)
        {
            ulong moves = 0;
            ulong piece = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((piece & bishops) != 0)
                {
                    moves |= GetBishopMoves(i / 8, i % 8, occupied, piececolor);
                }
                piece <<= 1;
            }
            return moves;
        }

        public static ulong GetAllKnightMoves(ulong knights, ulong piececolor)
        {
            ulong moves = 0;
            ulong piece = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((piece & knights) != 0)
                {
                    moves |= GetKnightMoves(i / 8, i % 8, piececolor);
                }
                piece <<= 1;
            }
            return moves;
        }

        public static ulong GetAllRookMoves(ulong rooks, ulong occupied, ulong piececolor)
        {
            ulong moves = 0;
            ulong piece = 1;
            for (int i = 0; i < 64; i++)
            {
                if ((piece & rooks) != 0)
                {
                    moves |= GetRookMoves(i / 8, i % 8, occupied, piececolor);
                }
                piece <<= 1;
            }
            return moves;
        }

        public static ulong SlidingMoves(int rank, int file, ulong mask, ulong occupied, ulong piececolor)
        {
            int index = rank * 8 + file; //Converts rank and file into index: A1 = 0, H8 = 63
            ulong s = (ulong)0x1 << index; //Creates bitboard consisting only of selected piece
            ulong o = (occupied & ~s) & mask; //Creates final o variable by removing piece from total occupied and focusing on mask

            ulong unfiltered = ((o - 2 * s) ^ Reverse(Reverse(o) - 2 * Reverse(s))) & mask; //hyperbola quintessance algorithm

            return unfiltered & ~piececolor; //Trims off capturing of own pieces
        }

        public static ulong GetBishopMoves(int rank, int file, ulong occupied, ulong piececolor)
        {
            int index = rank * 8 + file;
            ulong diag_moves = SlidingMoves(rank, file, diagonalmasks[(index % 8) + 7 - (index / 8)], occupied, piececolor);
            ulong adiag_moves = SlidingMoves(rank, file, adiagonalmasks[14 - ((index / 8) + (index % 8))], occupied, piececolor);

            return diag_moves | adiag_moves;
        }

        public static ulong GetRookMoves(int rank, int file, ulong occupied, ulong piececolor)
        {
            int index = rank * 8 + file;
            ulong h_moves = SlidingMoves(rank, file, rankmasks[index / 8], occupied, piececolor);
            ulong v_moves = SlidingMoves(rank, file, filemasks[index % 8], occupied, piececolor);

            return h_moves | v_moves;
        }

        public static ulong GetQueenMoves(int rank, int file, ulong occupied, ulong piececolor)
        {
            int index = rank * 8 + file;
            ulong h_moves = SlidingMoves(rank, file, rankmasks[index / 8], occupied, piececolor);
            ulong v_moves = SlidingMoves(rank, file, filemasks[index % 8], occupied, piececolor);
            ulong diag_moves = SlidingMoves(rank, file, diagonalmasks[(index % 8) + 7 - (index / 8)], occupied, piececolor);
            ulong adiag_moves = SlidingMoves(rank, file, adiagonalmasks[14 - ((index / 8) + (index % 8))], occupied, piececolor);

            return h_moves | v_moves | diag_moves | adiag_moves;
        }

        public static ulong GetKingMoves(int rank, int file, ulong piececolor)
        {
            int index = rank * 8 + file;
            ulong piece = (ulong)0x1 << index;

            ulong spot1 = (piece & clearA) << 7;
            ulong spot2 = piece << 8;
            ulong spot3 = (piece & clearH) << 9;
            ulong spot4 = (piece & clearH) << 1;
            ulong spot5 = (piece & clearH) >> 7;
            ulong spot6 = piece >> 8;
            ulong spot7 = (piece & clearA) >> 9;
            ulong spot8 = (piece & clearA) >> 1;

            ulong kingmoves = spot1 | spot2 | spot3 | spot4 | spot5 | spot6 | spot7 | spot8;
            return kingmoves & ~piececolor;
        }

        public static ulong GetWhiteCastleMoves(int rank, int file, ulong piececolor, Board board, ChessBoard chessboard)
        {
            ulong ks = 0;
            ulong qs = 0;
            int index = rank * 8 + file;
            ulong piece = (ulong)0x1 << index;

            if (board.InCheck(Game.PieceColor.White))
            {
                return 0;
            }

            if (chessboard.WhiteKingSide && ((piece << 1) & ~piececolor) != 0 && !board.IsCheck(piece << 1, index, Game.PieceColor.White))
            {
                ks = piece << 2;
            }
            if (chessboard.WhiteQueenSide && ((piece >> 1) & ~piececolor) != 0 && ((piece >> 3) & ~piececolor) != 0 && !board.IsCheck(piece >> 1, index, Game.PieceColor.White))
            {
                qs = piece >> 2;
            }

            ulong castles = ks | qs;
            return castles & ~piececolor;
        }

        public static ulong GetBlackCastleMoves(int rank, int file, ulong piececolor, Board board, ChessBoard chessboard)
        {
            ulong ks = 0;
            ulong qs = 0;
            int index = rank * 8 + file;
            ulong piece = (ulong)0x1 << index;

            if (board.InCheck(Game.PieceColor.Black))
            {
                return 0;
            }

            if (chessboard.BlackKingSide && ((piece << 1) & ~piececolor) != 0 && !board.IsCheck(piece << 1, index, Game.PieceColor.Black))
            {
                ks = piece << 2;
            }
            if (chessboard.BlackQueenSide && ((piece >> 1) & ~piececolor) != 0 && ((piece >> 3) & ~piececolor) != 0 && !board.IsCheck(piece >> 1, index, Game.PieceColor.Black))
            {
                qs = piece >> 2;
            }

            ulong castles = ks | qs;
            return castles & ~piececolor;
        }

        public static ulong[] GetCastle(int old_index, int new_index, ChessBoard board)
        {
            ulong old_piece = (ulong)0x1 << old_index;
            ulong new_piece = (ulong)0x1 << new_index;

            if (old_piece != board.WhiteKing && old_piece != board.BlackKing)
            {
                return new ulong[] { 0, 0 };
            }
            else if ((old_piece << 2) == new_piece)
            {
                return new ulong[] { old_piece << 1, old_piece << 3 };
            }
            else if (old_piece >> 2 == new_piece)
            {
                return new ulong[] { old_piece >> 1, old_piece >> 4 };
            }
            return new ulong[] { 0, 0 };
        }

        public static ulong GetKnightMoves(int rank, int file, ulong piececolor)
        {
            int index = rank * 8 + file;
            ulong piece = (ulong)0x1 << index;

            ulong spot1 = (piece & clearA & clearB) << 6;
            ulong spot2 = (piece & clearA) << 15;
            ulong spot3 = (piece & clearH) << 17;
            ulong spot4 = (piece & clearH & clearG) << 10;
            ulong spot5 = (piece & clearH & clearG) >> 6;
            ulong spot6 = (piece & clearH) >> 15;
            ulong spot7 = (piece & clearA) >> 17;
            ulong spot8 = (piece & clearA & clearB) >> 10;

            ulong knightmoves = spot1 | spot2 | spot3 | spot4 | spot5 | spot6 | spot7 | spot8;
            return knightmoves & ~piececolor;
        }

        public static ulong GetWhitePawnMoves(int rank, int file, ulong occupied, ulong othercolor, ulong enpassant)
        {
            int index = rank * 8 + file;
            ulong piece = (ulong)0x1 << index;

            ulong one_step = (piece << 8) & ~occupied;
            ulong two_step = ((one_step & rankmasks[2]) << 8) & ~occupied;
            ulong pawnmoves = one_step | two_step;

            ulong left_attack = (piece & clearA) << 7;
            ulong right_attack = (piece & clearH) << 9;
            ulong passant_left_attack = ((piece & clearA) << 7) & enpassant;
            ulong passant_right_attack = ((piece & clearH) << 9) & enpassant;
            ulong pawn_attacks = ((left_attack | right_attack) & othercolor) | passant_left_attack | passant_right_attack;

            return pawnmoves | pawn_attacks;
        }

        public static ulong GetBlackPawnMoves(int rank, int file, ulong occupied, ulong othercolor, ulong enpassant)
        {
            int index = rank * 8 + file;
            ulong piece = (ulong)0x1 << index;

            ulong one_step = (piece >> 8) & ~occupied;
            ulong two_step = ((one_step & rankmasks[5]) >> 8) & ~occupied;
            ulong pawnmoves = one_step | two_step;

            ulong left_attack = (piece & clearA) >> 9;
            ulong right_attack = (piece & clearH) >> 7;
            ulong passant_left_attack = ((piece & clearA) >> 9) & enpassant;
            ulong passant_right_attack = ((piece & clearH) >> 7) & enpassant;
            ulong pawn_attacks = ((left_attack | right_attack) & othercolor) | passant_left_attack | passant_right_attack;

            return pawnmoves | pawn_attacks;
        }
    }
}
