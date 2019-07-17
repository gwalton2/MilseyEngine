using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilseyEngine
{
    class Game
    {
        public enum PieceColor { White, Black };

        public bool isAI { get; }
        public PieceColor AIColor { get; }
        public PieceColor Turn { get; set; }
        public bool GameOver { get; set; }

        public Game()
        {
            Turn = PieceColor.White;
            GameOver = false;
            isAI = false;
        }

        public Game(PieceColor AI)
        {
            isAI = true;
            AIColor = AI;
            Turn = PieceColor.White;
            GameOver = false;
        }

        public void NextTurn()
        {
            Turn = OppositeColor(Turn);
        }

        public PieceColor OppositeColor(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return PieceColor.Black;
            }
            else
            {
                return PieceColor.White;
            }
        }
    }
}
