﻿using System.Collections;

namespace Yale.Parser.Internal
{
    internal class ShortCircuitInfo
    {
        public Stack Operands;
        public Stack Operators;

        public BranchManager Branches;

        public ShortCircuitInfo()
        {
            Operands = new Stack();
            Operators = new Stack();
            Branches = new BranchManager();
        }

        public void ClearTempState()
        {
            Operands.Clear();
            Operators.Clear();
        }
    }
}
