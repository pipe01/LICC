using LICC.Internal;
using System;

namespace LICC.Exceptions
{
    internal sealed class ParameterMismatchException : Exception
    {
        public int ExpectedMin { get; }
        public int ExpectedMax { get; }
        public int ParamsCount { get; }
        public Command Command { get; }

        public override string Message => "Expected " + (ExpectedMin != ExpectedMax ? $"between {ExpectedMin} and {ExpectedMax}" : ExpectedMax.ToString()) + $" arguments, got {(ParamsCount == 0 ? "none" : ParamsCount.ToString())}";

        public ParameterMismatchException(int expectedMin, int expectedMax, int paramsCount, Command command)
        {
            this.ExpectedMin = expectedMin;
            this.ExpectedMax = expectedMax;
            this.ParamsCount = paramsCount;
            this.Command = command;
        }
    }
}
