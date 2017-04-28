using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Knights_Tour.Tests
{
    [TestClass()]
    public class LogicHelperTests
    {
        private LogicHelper _logicHelper;
        private int[,] _positions;

        private TestContext _testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod()]
        public void GeneratePositionsTest()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    _logicHelper = new LogicHelper();
                    _positions = _logicHelper.GeneratePositions(i, j);

                    Assert.IsTrue(_positions.GetLength(0) == 64);
                    Assert.IsTrue(_positions.GetLength(1) == 2);

                    int[,] board = new int[8, 8];
                    const int emptyCell = -1;

                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            board[k, l] = emptyCell;
                        }
                    }

                    for (int step = 0; step < _positions.GetLength(0); step++)
                    {
                        int x = _positions[step, 0];
                        int y = _positions[step, 1];

                        Assert.AreEqual(board[x, y], emptyCell);

                        board[x, y] = step;
                    }

                    int index = 0;
                    int[] copyOfBoard = new int[64];
                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            copyOfBoard[index++] = board[k, l];
                        }
                    }

                    Array.Sort(copyOfBoard);
                    for (int k = 0; k < 64; k++)
                    {
                         Assert.AreEqual(copyOfBoard[k], k);
                    }
                }
            }
        }
    }
}