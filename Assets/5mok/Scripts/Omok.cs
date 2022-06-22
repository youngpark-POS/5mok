using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MyProject
{
    public class OmokGame
    {
        public sbyte player;
        public OmokBoard board;
        public OmokLogic logic;
        public sbyte result;

        public event System.Action<sbyte, RowCol, RowCol> onGameEnd;

        public OmokGame(int row = 15, int col = 15, int length = 5)
        {
            this.player = 1;
            this.board = new OmokBoard(row, col);
            this.logic = new OmokLogic(row, col, length);
        }

        public bool TakeAction(int r, int c)
        {
            if (this.result != 0)
                return false;

            int action = r * this.board.Column + c;
            this.board = this.logic.GetNextState(this.board, this.player, action) as OmokBoard;
            this.player = (sbyte)-this.player;

            sbyte winner = this.logic.GameResult(this.board, this.player, out RowCol p1, out RowCol p2);
            if (winner != 0)
            {
                this.result = winner;
                this.onGameEnd?.Invoke(winner, p1, p2);
            }

            return true;
        }

        public bool TakeAction(int action)
        {
            RowCol rc = ActionToRC(action);
            return TakeAction(rc.r, rc.c);
        }

        public RowCol ActionToRC(int action)
        {
            return new RowCol(action / this.board.Column, action % this.board.Column);
        }

        public int RCToAction(RowCol rc)
        {
            return rc.r * this.board.Column + rc.c;
        }
    }

    public interface IGameLogic
    {
        int GetActionSize();
        bool IsValidAction(IGameBoard board, int action);
        List<int> GetAllValidActions(IGameBoard board, sbyte player);
        IGameBoard GetNextState(IGameBoard board, sbyte player, int action);
        IGameBoard POV(IGameBoard board, sbyte player);
        sbyte GameResult(IGameBoard board, sbyte player);
    }

    public class OmokLogic : IGameLogic
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Length { get; private set; }

        public OmokLogic(int row, int col, int length)
        {
            this.Row = row;
            this.Column = col;
            this.Length = length;
        }

        public int GetActionSize()
        {
            return this.Row * this.Column;
        }

        public bool IsValidAction(IGameBoard board, int action)
        {
            int c = action % this.Column;
            int r = action / this.Column;

            return board.Get(r, c) == 0;
        }

        public List<int> GetAllValidActions(IGameBoard board, sbyte player)
        {
            List<int> actions = new List<int>();

            for (int r = 0; r < this.Row; r++)
                for (int c = 0; c < this.Column; c++)
                {
                    if (board.Get(r, c) == 0)
                        actions.Add(r * this.Column + c);
                }
            return actions;
        }

        public IGameBoard GetNextState(IGameBoard board, sbyte player, int action)
        {
            OmokBoard nextBoard = board.Copy() as OmokBoard;
            if (nextBoard == null)
                return null;

            int c = action % this.Column;
            int r = action / this.Column;

            nextBoard.Place(r, c, player);
            return nextBoard;
        }

        public IGameBoard POV(IGameBoard board, sbyte player)
        {
            IGameBoard povBoard = board.Copy();
            if (player == -1)
            {
                for (int r = 0; r < this.Row; r++)
                    for (int c = 0; c < this.Column; c++)
                    {
                        povBoard.Set(r, c, (sbyte)-povBoard.Get(r, c));
                    }
            }
            return povBoard;
        }

        public sbyte GameResult(IGameBoard board, sbyte player, out RowCol p1, out RowCol p2)
        {
            p1 = new RowCol(-1, -1);
            p2 = new RowCol(-1, -1);

            sbyte winner = 0;

            // Brute-force algorithm (not good)
            sbyte color;
            int i = 0;
            int r2 = 0, c2 = 0;
            for (int r1 = 0; r1 < this.Row; r1++)
                for (int c1 = 0; c1 < this.Column; c1++)
                {
                    color = board.Get(r1, c1);
                    if (color == 0)
                        continue;

                    // (+1, 0)
                    for (i = 1; i < this.Length; i++)
                    {
                        r2 = r1 + i;
                        if (r2 == this.Row || color != board.Get(r2, c1))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r2, c1);
                        break;
                    }

                    // (-1, 0)
                    for (i = 1; i < this.Length; i++)
                    {
                        r2 = r1 - i;
                        if (r2 == -1 || color != board.Get(r2, c1))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r2, c1);
                        break;
                    }

                    // (0, +1)
                    for (i = 1; i < this.Length; i++)
                    {
                        c2 = c1 + i;
                        if (c2 == this.Column || color != board.Get(r1, c2))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r1, c2);
                        break;
                    }

                    // (0, -1)
                    for (i = 1; i < this.Length; i++)
                    {
                        c2 = c1 - i;
                        if (c2 == -1 || color != board.Get(r1, c2))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r1, c2);
                        break;
                    }

                    // (+1, +1)
                    for (i = 1; i < this.Length; i++)
                    {
                        r2 = r1 + i;
                        c2 = c1 + i;
                        if (r2 == this.Row || c2 == this.Column || color != board.Get(r2, c2))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r2, c2);
                        break;
                    }

                    // (-1, -1)
                    for (i = 1; i < this.Length; i++)
                    {
                        r2 = r1 - i;
                        c2 = c1 - i;
                        if (r2 == -1 || c2 == -1 || color != board.Get(r2, c2))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r2, c2);
                        break;
                    }

                    // (+1, -1)
                    for (i = 1; i < this.Length; i++)
                    {
                        r2 = r1 + i;
                        c2 = c1 - i;
                        if (r2 == this.Row || c2 == -1 || color != board.Get(r2, c2))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r2, c2);
                        break;
                    }

                    // (-1, +1)
                    for (i = 1; i < this.Length; i++)
                    {
                        r2 = r1 - i;
                        c2 = c1 + i;
                        if (r2 == -1 || c2 == this.Column || color != board.Get(r2, c2))
                            break;
                    }
                    if (i == this.Length)
                    {
                        winner = color;
                        p1 = new RowCol(r1, c1); p2 = new RowCol(r2, c2);
                        break;
                    }
                }

            if (p1.r > p2.r)
            {
                var temp = p1; p1 = p2; p2 = temp;
            }

            if (winner == player)
                return 1;
            else if (winner == -player)
                return -1;
            return 0;
        }

        public sbyte GameResult(IGameBoard board, sbyte player)
        {
            return GameResult(board, player, out RowCol p1, out RowCol p2);
        }

    }

    public interface IGameBoard
    {
        sbyte Get(int r, int c);
        void Set(int r, int c, sbyte color);
        IGameBoard Copy();
    }

    public class OmokBoard : IGameBoard
    {
        public const sbyte Black = 1;
        public const sbyte White = -1;

        public int Row { get; private set; }
        public int Column { get; private set; }
        private sbyte[,] board;

        public OmokBoard(int row, int col)
        {
            this.Row = row;
            this.Column = col;

            this.board = new sbyte[this.Row, this.Column];
            for (int i = 0; i < this.Row; i++)
                for (int j = 0; j < this.Column; j++)
                    this.board[i, j] = 0;
        }

        public OmokBoard(OmokBoard src)
        {
            this.Row = src.Row;
            this.Column = src.Column;

            this.board = new sbyte[this.Row, this.Column];
            for (int i = 0; i < this.Row; i++)
                for (int j = 0; j < this.Column; j++)
                    this.board[i, j] = src.board[i, j];
        }

        public sbyte Get(int r, int c) => this.board[r, c];
        public void Set(int r, int c, sbyte color) => this.board[r, c] = color;

        public bool Place(int r, int c, sbyte player)
        {
            if (this.board[r, c] != 0)
                return false;
            if (player == 1 || player == -1)
            {
                this.board[r, c] = player;
                return true;
            }
            return false;
        }

        public IGameBoard Copy()
        {
            return new OmokBoard(this);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int r = 0; r < this.Row; r++)
            {
                for (int c = 0; c < this.Column; c++)
                {
                    sbyte color = this.board[r, c];
                    builder.Append(color == 1 ? "●" : (color == -1 ? "○" : "--"));
                }
                builder.AppendLine("");
            }
            return builder.ToString();
        }
    }
}