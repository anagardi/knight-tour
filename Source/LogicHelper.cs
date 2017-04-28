using System;
using System.Collections.Generic;
using System.Linq;

namespace Knights_Tour
{
    public class LogicHelper
    {
        // Directions the Knight can move on the chessboard
        int[] _deltax = new int[] { -1, -2, -2, -1, 1, 2, 2, 1 };
        int[] _deltay = new int[] { -2, -1, 1, 2, 2, 1, -1, -2 };

        // Chessboard
        int[,] _chessBoard = new int[8, 8];

        int[,] _positions = new int[64, 2];

        // Return true, if (x,y) cell is on board and is avaliable
        private static bool IsAvailableNeighbor(int x, int y, int[,] board)
        {
            if ((x >= 0) && (x < board.GetLength(0)) && (y >= 0) && (y < board.GetLength(1)))
            {
                return (board[x, y] == 0);
            }
            return false;
        }

        public int[,] GeneratePositions(int startx, int starty)
        {
            for (int k = 0; k < 64; k++)
            {
                // Set the position of knight on the board on step k
                _positions[k, 0] = startx;
                _positions[k, 1] = starty;

                // Set the value of current step on board
                _chessBoard[startx, starty] = k + 1;

                // Priority queue of available neighbors
                Dictionary<int, int> pq = new Dictionary<int, int>();

                for (int i = 0; i < 8; i++)
                {
                    // Get the next direction that knight can move on the board                 
                    var nextx = startx + _deltax[i];
                    var nexty = starty + _deltay[i];

                    // Check, weather the next direction that knight can move is available
                    if (IsAvailableNeighbor(nextx, nexty, _chessBoard))
                    {
                        // Count the available neighbors of the neighbor
                        int count = 0;
                        for (int j = 0; j < 8; j++)
                        {
                            // Get the the neighbor of neighbor
                            var nextneighborx = nextx + _deltax[j];
                            var nextneighbory = nexty + _deltay[j];

                            // Check, if the neighbor of neighbor is available
                            if (IsAvailableNeighbor(nextneighborx, nextneighbory, _chessBoard))
                            {
                                // Increase the count of available neighbors of the neighbor
                                count++;
                            }
                        }
                        // Store the count of available neighbors of the neighbor
                        pq.Add(i, count);
                    }
                }

                // Move to the neighbor that has min number of available neighbors
                if (pq.Count > 0)
                {
                    // Store the value of the key of the first neighbor with minimal value in pq
                    int minKey = pq.OrderBy(kvp => kvp.Value).First().Key;
                    // If there are multiple keys with the same minimal value, select the one, which is nearest to the edge of the board
                    // Get the minimal value
                    int minVal = pq.OrderBy(kvp => kvp.Value).First().Value;
                    Dictionary<int, int> minkv = new Dictionary<int, int>();
                    // Store all elements with minVal value in minkv dictionary
                    foreach (int key in pq.Keys)
                    {
                        if (pq[key] == minVal)
                        {
                            minkv.Add(key, minVal);
                        }
                    }
                    if (minkv.Count > 1)
                    {
                        // Select the square nearest the edge of the board
                        Dictionary<int, int> nextCell = new Dictionary<int, int>();
                        foreach (int key in minkv.Keys)
                        {
                            int nextx = 3 - (int)Math.Abs(3.5 - (startx + _deltax[key]));
                            int nexty = 3 - (int)Math.Abs(3.5 - (starty + _deltay[key]));

                            nextCell.Add(key, nextx * nexty);
                        }
                        minKey = nextCell.OrderBy(kvp => kvp.Value).First().Key;
                    }
                    startx += _deltax[minKey];
                    starty += _deltay[minKey];
                }
                else
                {
                    break;
                }
            }
            return _positions;
        }
    }
}
