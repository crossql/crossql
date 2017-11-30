namespace FutureState.AppCore.Data.Constraints
{
    public class AutoIncrementConstraint : IConstraint
    {
        private readonly IDialect _dialect;
        private readonly int _start;
        private readonly int _increment;

        public AutoIncrementConstraint(IDialect dialect, int start, int increment)
        {
            _dialect = dialect;
            _start = start;
            _increment = increment;
        }

        public override string ToString() => string.Format(_dialect.AutoIncrement, _start, _increment);
    }
}