using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MilseyEngine
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Page
    {
        AI myAI;
        Board board;
        Button selected;
        int selected_index;
        Game game;
        ChessBoard chessboard;

        Dictionary<int, Button> buttons;
        Dictionary<char, string> piece_icons = new Dictionary<char, string>()
        {
            {'b', "b_bishop"}, {'k', "b_king"}, {'n', "b_knight"}, {'p', "b_pawn"}, {'q', "b_queen"}, {'r', "b_rook"},
            {'B', "w_bishop"}, {'K', "w_king"}, {'N', "w_knight"}, {'P', "w_pawn"}, {'Q', "w_queen"}, {'R', "w_rook"}, {'-', null}
        };

        public GameWindow(bool AI, bool white)
        {
            InitializeComponent();

            chessboard = new ChessBoard();

            if (AI)
            {
                if (white)
                {
                    game = new Game(Game.PieceColor.White);
                    board = new Board(game, chessboard);
                    myAI = new AI(chessboard, board, game, 5, game.AIColor);
                }
                else
                {
                    game = new Game(Game.PieceColor.Black);
                    board = new Board(game, chessboard);
                    myAI = new AI(chessboard, board, game, 5, game.AIColor);
                }
            }
            else
            {
                game = new Game();
                board = new Board(game, chessboard);
            }

            buttons = new Dictionary<int, Button>
            {
                {0, _00}, {1, _01}, {2, _02}, {3, _03}, {4, _04}, {5, _05}, {6, _06}, {7, _07},
                {8, _10}, {9, _11}, {10, _12}, {11, _13}, {12, _14}, {13, _15}, {14, _16}, {15, _17},
                {16, _20}, {17, _21}, {18, _22}, {19, _23}, {20, _24}, {21, _25}, {22, _26}, {23, _27},
                {24, _30}, {25, _31}, {26, _32}, {27, _33}, {28, _34}, {29, _35}, {30, _36}, {31, _37},
                {32, _40}, {33, _41}, {34, _42}, {35, _43}, {36, _44}, {37, _45}, {38, _46}, {39, _47},
                {40, _50}, {41, _51}, {42, _52}, {43, _53}, {44, _54}, {45, _55}, {46, _56}, {47, _57},
                {48, _60}, {49, _61}, {50, _62}, {51, _63}, {52, _64}, {53, _65}, {54, _66}, {55, _67},
                {56, _70}, {57, _71}, {58, _72}, {59, _73}, {60, _74}, {61, _75}, {62, _76}, {63, _77}
            };

            CopyBoard();

            if (white)
            {
                HandleAI();
            }
        }

        private void Move(Button new_square)
        {
            ResetColor();
            ChangeTurn();
            SetLastMove();

            CopyBoard();
            CheckGameOver();

            HandleAI();
        }

        private void CheckGameOver()
        {
            if (game.GameOver)
            {
                game.NextTurn();
                winLabel.Content = game.Turn + " wins";
                game.NextTurn();
            }
        }

        private void HandleAI()
        {
            if (game.isAI && !game.GameOver && game.AIColor == game.Turn)
            {
                int[] AImove = myAI.GetMove();
                board.MoveBitBoard(AImove[0], AImove[1]);
                board.MoveCharBoard(AImove[0], AImove[1]);
                Move(_00);
            }
        }

        private void SetLastMove()
        {
            Dictionary<int, char> file = new Dictionary<int, char>()
            {
                {0, 'a'}, {1, 'b'}, {2, 'c'}, {3, 'd'}, {4, 'e'}, {5, 'f'}, {6, 'g'}, {7, 'h'}
            };

            int og_rank = board.Lastmove[0] + 1;
            int og_file = board.Lastmove[1];
            int new_rank = board.Lastmove[2] + 1;
            int new_file = board.Lastmove[3];

            string move = file[og_file] + og_rank.ToString() + file[new_file] + new_rank.ToString();
            if (board.Captured)
            {
                move += " (capture)";
            }
            moveLabel.Content = move;
        }

        private void ChangeTurn()
        {
            if (game.Turn == Game.PieceColor.White)
            {
                colorLabel.Content = "White";
                colorLabel.Foreground = Brushes.Black;
                colorLabel.Background = Brushes.White;
            }
            else
            {
                colorLabel.Content = "Black";
                colorLabel.Foreground = Brushes.White;
                colorLabel.Background = Brushes.Black;
            }
        }

        private void ResetColor()
        {
            foreach (int i in buttons.Keys)
            {
                int rank = i / 8;
                int file = i % 8;

                if ((rank + file) % 2 == 0)
                {
                    buttons[i].Background = Brushes.Black;
                }
                else
                {
                    buttons[i].Background = Brushes.White;
                }
            }
        }

        private void DisplayMoves(ulong moves)
        {
            ResetColor();

            List<int> valid_moves = Moves.ConvertBitboard(moves);
            foreach (int i in valid_moves)
            {
                buttons[i].Background = Brushes.Green;
            }

            if (board.DisplayCheck)
            {
                DisplayCheck();
            }
        }

        private void DisplayCheck()
        {
            ulong king;
            if (game.Turn == Game.PieceColor.White)
            {
                king = chessboard.WhiteKing;
            }
            else
            {
                king = chessboard.BlackKing;
            }

            int index = 0;
            for (int i = 0; i < 64; i++)
            {
                ulong piece = (ulong)0x1 << i;
                if ((king & piece) != 0)
                {
                    index = i;
                    break;
                }
            }
            buttons[index].Background = Brushes.Red;
        }

        private void CopyBoard()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int index = rank * 8 + file;
                    char piece = board.MyBoard[rank, file];
                    string icon = piece_icons[piece];
                    if (icon != null)
                    {
                        buttons[index].Content = FindResource(piece_icons[piece]);
                    }
                    else
                    {
                        buttons[index].Content = null;
                    }
                }
            }
        }

        private void _00_Click(object sender, RoutedEventArgs e)
        {
            if (_00.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 0);
                board.MoveCharBoard(selected_index, 0);
                Move(_00);
            }
            else if (board.IsOccupied(0, 0))
            {
                selected = _00;
                selected_index = 0;

                ulong moves = board.GetMoves(0, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _01_Click(object sender, RoutedEventArgs e)
        {
            if (_01.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 1);
                board.MoveCharBoard(selected_index, 1);
                Move(_01);
            }
            else if (board.IsOccupied(0, 1))
            {
                selected = _01;
                selected_index = 1;

                ulong moves = board.GetMoves(0, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _02_Click(object sender, RoutedEventArgs e)
        {
            if (_02.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 2);
                board.MoveCharBoard(selected_index, 2);
                Move(_02);
            }
            else if (board.IsOccupied(0, 2))
            {
                selected = _02;
                selected_index = 2;

                ulong moves = board.GetMoves(0, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _03_Click(object sender, RoutedEventArgs e)
        {
            if (_03.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 3);
                board.MoveCharBoard(selected_index, 3);
                Move(_03);
            }
            else if (board.IsOccupied(0, 3))
            {
                selected = _03;
                selected_index = 3;

                ulong moves = board.GetMoves(0, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _04_Click(object sender, RoutedEventArgs e)
        {
            if (_04.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 4);
                board.MoveCharBoard(selected_index, 4);
                Move(_04);
            }
            else if (board.IsOccupied(0, 4))
            {
                selected = _04;
                selected_index = 4;

                ulong moves = board.GetMoves(0, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _05_Click(object sender, RoutedEventArgs e)
        {
            if (_05.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 5);
                board.MoveCharBoard(selected_index, 5);
                Move(_05);
            }
            else if (board.IsOccupied(0, 5))
            {
                selected = _05;
                selected_index = 5;

                ulong moves = board.GetMoves(0, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _06_Click(object sender, RoutedEventArgs e)
        {
            if (_06.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 6);
                board.MoveCharBoard(selected_index, 6);
                Move(_06);
            }
            else if (board.IsOccupied(0, 6))
            {
                selected = _06;
                selected_index = 6;

                ulong moves = board.GetMoves(0, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _07_Click(object sender, RoutedEventArgs e)
        {
            if (_07.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 7);
                board.MoveCharBoard(selected_index, 7);
                Move(_07);
            }
            else if (board.IsOccupied(0, 7))
            {
                selected = _07;
                selected_index = 7;

                ulong moves = board.GetMoves(0, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _10_Click(object sender, RoutedEventArgs e)
        {
            if (_10.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 8);
                board.MoveCharBoard(selected_index, 8);
                Move(_10);
            }
            else if (board.IsOccupied(1, 0))
            {
                selected = _10;
                selected_index = 8;

                ulong moves = board.GetMoves(1, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _11_Click(object sender, RoutedEventArgs e)
        {
            if (_11.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 9);
                board.MoveCharBoard(selected_index, 9);
                Move(_11);
            }
            else if (board.IsOccupied(1, 1))
            {
                selected = _11;
                selected_index = 9;

                ulong moves = board.GetMoves(1, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _12_Click(object sender, RoutedEventArgs e)
        {
            if (_12.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 10);
                board.MoveCharBoard(selected_index, 10);
                Move(_12);
            }
            else if (board.IsOccupied(1, 2))
            {
                selected = _12;
                selected_index = 10;

                ulong moves = board.GetMoves(1, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _13_Click(object sender, RoutedEventArgs e)
        {
            if (_13.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 11);
                board.MoveCharBoard(selected_index, 11);
                Move(_13);
            }
            else if (board.IsOccupied(1, 3))
            {
                selected = _13;
                selected_index = 11;

                ulong moves = board.GetMoves(1, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _14_Click(object sender, RoutedEventArgs e)
        {
            if (_14.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 12);
                board.MoveCharBoard(selected_index, 12);
                Move(_14);
            }
            else if (board.IsOccupied(1, 4))
            {
                selected = _14;
                selected_index = 12;

                ulong moves = board.GetMoves(1, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _15_Click(object sender, RoutedEventArgs e)
        {
            if (_15.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 13);
                board.MoveCharBoard(selected_index, 13);
                Move(_15);
            }
            else if (board.IsOccupied(1, 5))
            {
                selected = _15;
                selected_index = 13;

                ulong moves = board.GetMoves(1, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _16_Click(object sender, RoutedEventArgs e)
        {
            if (_16.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 14);
                board.MoveCharBoard(selected_index, 14);
                Move(_16);
            }
            else if (board.IsOccupied(1, 6))
            {
                selected = _16;
                selected_index = 14;

                ulong moves = board.GetMoves(1, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _17_Click(object sender, RoutedEventArgs e)
        {
            if (_17.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 15);
                board.MoveCharBoard(selected_index, 15);
                Move(_17);
            }
            else if (board.IsOccupied(1, 7))
            {
                selected = _17;
                selected_index = 15;

                ulong moves = board.GetMoves(1, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _20_Click(object sender, RoutedEventArgs e)
        {
            if (_20.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 16);
                board.MoveCharBoard(selected_index, 16);
                Move(_20);
            }
            else if (board.IsOccupied(2, 0))
            {
                selected = _20;
                selected_index = 16;

                ulong moves = board.GetMoves(2, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _21_Click(object sender, RoutedEventArgs e)
        {
            if (_21.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 17);
                board.MoveCharBoard(selected_index, 17);
                Move(_21);
            }
            else if (board.IsOccupied(2, 1))
            {
                selected = _21;
                selected_index = 17;

                ulong moves = board.GetMoves(2, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _22_Click(object sender, RoutedEventArgs e)
        {
            if (_22.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 18);
                board.MoveCharBoard(selected_index, 18);
                Move(_22);
            }
            else if (board.IsOccupied(2, 2))
            {
                selected = _22;
                selected_index = 18;

                ulong moves = board.GetMoves(2, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _23_Click(object sender, RoutedEventArgs e)
        {
            if (_23.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 19);
                board.MoveCharBoard(selected_index, 19);
                Move(_23);
            }
            else if (board.IsOccupied(2, 3))
            {
                selected = _23;
                selected_index = 19;

                ulong moves = board.GetMoves(2, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _24_Click(object sender, RoutedEventArgs e)
        {
            if (_24.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 20);
                board.MoveCharBoard(selected_index, 20);
                Move(_24);
            }
            else if (board.IsOccupied(2, 4))
            {
                selected = _24;
                selected_index = 20;

                ulong moves = board.GetMoves(2, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _25_Click(object sender, RoutedEventArgs e)
        {
            if (_25.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 21);
                board.MoveCharBoard(selected_index, 21);
                Move(_25);
            }
            else if (board.IsOccupied(2, 5))
            {
                selected = _25;
                selected_index = 21;

                ulong moves = board.GetMoves(2, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _26_Click(object sender, RoutedEventArgs e)
        {
            if (_26.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 22);
                board.MoveCharBoard(selected_index, 22);
                Move(_26);
            }
            else if (board.IsOccupied(2, 6))
            {
                selected = _26;
                selected_index = 22;

                ulong moves = board.GetMoves(2, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _27_Click(object sender, RoutedEventArgs e)
        {
            if (_27.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 23);
                board.MoveCharBoard(selected_index, 23);
                Move(_27);
            }
            else if (board.IsOccupied(2, 7))
            {
                selected = _27;
                selected_index = 23;

                ulong moves = board.GetMoves(2, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _30_Click(object sender, RoutedEventArgs e)
        {
            if (_30.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 24);
                board.MoveCharBoard(selected_index, 24);
                Move(_30);
            }
            else if (board.IsOccupied(3, 0))
            {
                selected = _30;
                selected_index = 24;

                ulong moves = board.GetMoves(3, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _31_Click(object sender, RoutedEventArgs e)
        {
            if (_31.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 25);
                board.MoveCharBoard(selected_index, 25);
                Move(_31);
            }
            else if (board.IsOccupied(3, 1))
            {
                selected = _31;
                selected_index = 25;

                ulong moves = board.GetMoves(3, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _32_Click(object sender, RoutedEventArgs e)
        {
            if (_32.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 26);
                board.MoveCharBoard(selected_index, 26);
                Move(_32);
            }
            else if (board.IsOccupied(3, 2))
            {
                selected = _32;
                selected_index = 26;

                ulong moves = board.GetMoves(3, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _33_Click(object sender, RoutedEventArgs e)
        {
            if (_33.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 27);
                board.MoveCharBoard(selected_index, 27);
                Move(_33);
            }
            else if (board.IsOccupied(3, 3))
            {
                selected = _33;
                selected_index = 27;

                ulong moves = board.GetMoves(3, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _34_Click(object sender, RoutedEventArgs e)
        {
            if (_34.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 28);
                board.MoveCharBoard(selected_index, 28);
                Move(_34);
            }
            else if (board.IsOccupied(3, 4))
            {
                selected = _34;
                selected_index = 28;

                ulong moves = board.GetMoves(3, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _35_Click(object sender, RoutedEventArgs e)
        {
            if (_35.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 29);
                board.MoveCharBoard(selected_index, 29);
                Move(_35);
            }
            else if (board.IsOccupied(3, 5))
            {
                selected = _35;
                selected_index = 29;

                ulong moves = board.GetMoves(3, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _36_Click(object sender, RoutedEventArgs e)
        {
            if (_36.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 30);
                board.MoveCharBoard(selected_index, 30);
                Move(_36);
            }
            else if (board.IsOccupied(3, 6))
            {
                selected = _36;
                selected_index = 30;

                ulong moves = board.GetMoves(3, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _37_Click(object sender, RoutedEventArgs e)
        {
            if (_37.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 31);
                board.MoveCharBoard(selected_index, 31);
                Move(_37);
            }
            else if (board.IsOccupied(3, 7))
            {
                selected = _37;
                selected_index = 31;

                ulong moves = board.GetMoves(3, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _40_Click(object sender, RoutedEventArgs e)
        {
            if (_40.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 32);
                board.MoveCharBoard(selected_index, 32);
                Move(_40);
            }
            else if (board.IsOccupied(4, 0))
            {
                selected = _40;
                selected_index = 32;

                ulong moves = board.GetMoves(4, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _41_Click(object sender, RoutedEventArgs e)
        {
            if (_41.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 33);
                board.MoveCharBoard(selected_index, 33);
                Move(_41);
            }
            else if (board.IsOccupied(4, 1))
            {
                selected = _41;
                selected_index = 33;

                ulong moves = board.GetMoves(4, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _42_Click(object sender, RoutedEventArgs e)
        {
            if (_42.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 34);
                board.MoveCharBoard(selected_index, 34);
                Move(_42);
            }
            else if (board.IsOccupied(4, 2))
            {
                selected = _42;
                selected_index = 34;

                ulong moves = board.GetMoves(4, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _43_Click(object sender, RoutedEventArgs e)
        {
            if (_43.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 35);
                board.MoveCharBoard(selected_index, 35);
                Move(_43);
            }
            else if (board.IsOccupied(4, 3))
            {
                selected = _43;
                selected_index = 35;

                ulong moves = board.GetMoves(4, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _44_Click(object sender, RoutedEventArgs e)
        {
            if (_44.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 36);
                board.MoveCharBoard(selected_index, 36);
                Move(_44);
            }
            else if (board.IsOccupied(4, 4))
            {
                selected = _44;
                selected_index = 36;

                ulong moves = board.GetMoves(4, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _45_Click(object sender, RoutedEventArgs e)
        {
            if (_45.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 37);
                board.MoveCharBoard(selected_index, 37);
                Move(_45);
            }
            else if (board.IsOccupied(4, 5))
            {
                selected = _45;
                selected_index = 37;

                ulong moves = board.GetMoves(4, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _46_Click(object sender, RoutedEventArgs e)
        {
            if (_46.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 38);
                board.MoveCharBoard(selected_index, 38);
                Move(_46);
            }
            else if (board.IsOccupied(4, 6))
            {
                selected = _46;
                selected_index = 38;

                ulong moves = board.GetMoves(4, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _47_Click(object sender, RoutedEventArgs e)
        {
            if (_47.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 39);
                board.MoveCharBoard(selected_index, 39);
                Move(_47);
            }
            else if (board.IsOccupied(4, 7))
            {
                selected = _47;
                selected_index = 39;

                ulong moves = board.GetMoves(4, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _50_Click(object sender, RoutedEventArgs e)
        {
            if (_50.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 40);
                board.MoveCharBoard(selected_index, 40);
                Move(_50);
            }
            else if (board.IsOccupied(5, 0))
            {
                selected = _50;
                selected_index = 40;

                ulong moves = board.GetMoves(5, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _51_Click(object sender, RoutedEventArgs e)
        {
            if (_51.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 41);
                board.MoveCharBoard(selected_index, 41);
                Move(_51);
            }
            else if (board.IsOccupied(5, 1))
            {
                selected = _51;
                selected_index = 41;

                ulong moves = board.GetMoves(5, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _52_Click(object sender, RoutedEventArgs e)
        {
            if (_52.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 42);
                board.MoveCharBoard(selected_index, 42);
                Move(_52);
            }
            else if (board.IsOccupied(5, 2))
            {
                selected = _52;
                selected_index = 42;

                ulong moves = board.GetMoves(5, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _53_Click(object sender, RoutedEventArgs e)
        {
            if (_53.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 43);
                board.MoveCharBoard(selected_index, 43);
                Move(_53);
            }
            else if (board.IsOccupied(5, 3))
            {
                selected = _53;
                selected_index = 43;

                ulong moves = board.GetMoves(5, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _54_Click(object sender, RoutedEventArgs e)
        {
            if (_54.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 44);
                board.MoveCharBoard(selected_index, 44);
                Move(_54);
            }
            else if (board.IsOccupied(5, 4))
            {
                selected = _54;
                selected_index = 44;

                ulong moves = board.GetMoves(5, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _55_Click(object sender, RoutedEventArgs e)
        {
            if (_55.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 45);
                board.MoveCharBoard(selected_index, 45);
                Move(_55);
            }
            else if (board.IsOccupied(5, 5))
            {
                selected = _55;
                selected_index = 45;

                ulong moves = board.GetMoves(5, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _56_Click(object sender, RoutedEventArgs e)
        {
            if (_56.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 46);
                board.MoveCharBoard(selected_index, 46);
                Move(_56);
            }
            else if (board.IsOccupied(5, 6))
            {
                selected = _56;
                selected_index = 46;

                ulong moves = board.GetMoves(5, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _57_Click(object sender, RoutedEventArgs e)
        {
            if (_57.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 47);
                board.MoveCharBoard(selected_index, 47);
                Move(_57);
            }
            else if (board.IsOccupied(5, 7))
            {
                selected = _57;
                selected_index = 47;

                ulong moves = board.GetMoves(5, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _60_Click(object sender, RoutedEventArgs e)
        {
            if (_60.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 48);
                board.MoveCharBoard(selected_index, 48);
                Move(_60);
            }
            else if (board.IsOccupied(6, 0))
            {
                selected = _60;
                selected_index = 48;

                ulong moves = board.GetMoves(6, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _61_Click(object sender, RoutedEventArgs e)
        {
            if (_61.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 49);
                board.MoveCharBoard(selected_index, 49);
                Move(_61);
            }
            else if (board.IsOccupied(6, 1))
            {
                selected = _61;
                selected_index = 49;

                ulong moves = board.GetMoves(6, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _62_Click(object sender, RoutedEventArgs e)
        {
            if (_62.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 50);
                board.MoveCharBoard(selected_index, 50);
                Move(_62);
            }
            else if (board.IsOccupied(6, 2))
            {
                selected = _62;
                selected_index = 50;

                ulong moves = board.GetMoves(6, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _63_Click(object sender, RoutedEventArgs e)
        {
            if (_63.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 51);
                board.MoveCharBoard(selected_index, 51);
                Move(_63);
            }
            else if (board.IsOccupied(6, 3))
            {
                selected = _63;
                selected_index = 51;

                ulong moves = board.GetMoves(6, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _64_Click(object sender, RoutedEventArgs e)
        {
            if (_64.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 52);
                board.MoveCharBoard(selected_index, 52);
                Move(_64);
            }
            else if (board.IsOccupied(6, 4))
            {
                selected = _64;
                selected_index = 52;

                ulong moves = board.GetMoves(6, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _65_Click(object sender, RoutedEventArgs e)
        {
            if (_65.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 53);
                board.MoveCharBoard(selected_index, 53);
                Move(_65);
            }
            else if (board.IsOccupied(6, 5))
            {
                selected = _65;
                selected_index = 53;

                ulong moves = board.GetMoves(6, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _66_Click(object sender, RoutedEventArgs e)
        {
            if (_66.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 54);
                board.MoveCharBoard(selected_index, 54);
                Move(_66);
            }
            else if (board.IsOccupied(6, 6))
            {
                selected = _66;
                selected_index = 54;

                ulong moves = board.GetMoves(6, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _67_Click(object sender, RoutedEventArgs e)
        {
            if (_67.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 55);
                board.MoveCharBoard(selected_index, 55);
                Move(_67);
            }
            else if (board.IsOccupied(6, 7))
            {
                selected = _67;
                selected_index = 55;

                ulong moves = board.GetMoves(6, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _70_Click(object sender, RoutedEventArgs e)
        {
            if (_70.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 56);
                board.MoveCharBoard(selected_index, 56);
                Move(_70);
            }
            else if (board.IsOccupied(7, 0))
            {
                selected = _70;
                selected_index = 56;

                ulong moves = board.GetMoves(7, 0);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _71_Click(object sender, RoutedEventArgs e)
        {
            if (_71.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 57);
                board.MoveCharBoard(selected_index, 57);
                Move(_71);
            }
            else if (board.IsOccupied(7, 1))
            {
                selected = _71;
                selected_index = 57;

                ulong moves = board.GetMoves(7, 1);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _72_Click(object sender, RoutedEventArgs e)
        {
            if (_72.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 58);
                board.MoveCharBoard(selected_index, 58);
                Move(_72);
            }
            else if (board.IsOccupied(7, 2))
            {
                selected = _72;
                selected_index = 58;

                ulong moves = board.GetMoves(7, 2);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _73_Click(object sender, RoutedEventArgs e)
        {
            if (_73.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 59);
                board.MoveCharBoard(selected_index, 59);
                Move(_73);
            }
            else if (board.IsOccupied(7, 3))
            {
                selected = _73;
                selected_index = 59;

                ulong moves = board.GetMoves(7, 3);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _74_Click(object sender, RoutedEventArgs e)
        {
            if (_74.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 60);
                board.MoveCharBoard(selected_index, 60);
                Move(_74);
            }
            else if (board.IsOccupied(7, 4))
            {
                selected = _74;
                selected_index = 60;

                ulong moves = board.GetMoves(7, 4);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _75_Click(object sender, RoutedEventArgs e)
        {
            if (_75.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 61);
                board.MoveCharBoard(selected_index, 61);
                Move(_75);
            }
            else if (board.IsOccupied(7, 5))
            {
                selected = _75;
                selected_index = 61;

                ulong moves = board.GetMoves(7, 5);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _76_Click(object sender, RoutedEventArgs e)
        {
            if (_76.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 62);
                board.MoveCharBoard(selected_index, 62);
                Move(_76);
            }
            else if (board.IsOccupied(7, 6))
            {
                selected = _76;
                selected_index = 62;

                ulong moves = board.GetMoves(7, 6);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void _77_Click(object sender, RoutedEventArgs e)
        {
            if (_77.Background.Equals(Brushes.Green))
            {
                board.MoveBitBoard(selected_index, 63);
                board.MoveCharBoard(selected_index, 63);
                Move(_77);
            }
            else if (board.IsOccupied(7, 7))
            {
                selected = _77;
                selected_index = 63;

                ulong moves = board.GetMoves(7, 7);
                DisplayMoves(moves);
            }
            else
            {
                ResetColor();
            }
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            TitleWindow tw = new TitleWindow();
            this.NavigationService.Navigate(tw);
        }
    }
}
