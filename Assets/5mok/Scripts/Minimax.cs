using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    public class Minimax
    {
        public int maxDepth;
        public IGameLogic logic;

        public Minimax(IGameLogic logic, int maxDepth)
        {
            this.logic = logic;
            this.maxDepth = maxDepth;
        }

        public int Find(IGameBoard state, bool isPlayer)
        {
            _AlphaBeta(state, this.maxDepth, float.NegativeInfinity, float.PositiveInfinity, isPlayer, out float _, out int action);
            return action;
        }

        private void _AlphaBeta(IGameBoard state, int depth, float alpha, float beta, bool maximizingPlayer, out float value, out int action)
        {
            sbyte res = this.logic.GameResult(state, 1);
            if (depth == 0 || res != 0)
            {
                value = (float)res;
                action = -1;
                return;
            }

            if (maximizingPlayer)
            {
                value = float.NegativeInfinity;
                action = -1;

                List<int> actions = this.logic.GetAllValidActions(state, 1);
                for (int i = 0; i < actions.Count; i++)
                {
                    int a = actions[i];
                    IGameBoard nextState = this.logic.GetNextState(state, 1, a);
                    _AlphaBeta(nextState, depth - 1, alpha, beta, false, out float newValue, out int _);
                    if (value < newValue)
                    {
                        value = newValue;
                        action = a;
                    }
                    if (alpha < newValue)
                        alpha = newValue;
                    if (beta <= alpha)
                        break;
                }
            }
            else
            {
                value = float.PositiveInfinity;
                action = -1;

                List<int> actions = this.logic.GetAllValidActions(state, -1);
                for (int i = 0; i < actions.Count; i++)
                {
                    int a = actions[i];
                    IGameBoard nextState = this.logic.GetNextState(state, -1, a);
                    _AlphaBeta(nextState, depth - 1, alpha, beta, true, out float newValue, out int _);
                    if (value > newValue)
                    {
                        value = newValue;
                        action = a;
                    }
                    if (beta > newValue)
                        beta = newValue;
                    if (beta <= alpha)
                        break;
                }
            }
        }
    }
}