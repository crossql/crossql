namespace crossql.Constraints
{
    public class OnDeleteNoActionConstraint : IConstraint
    {
        private static IDialect _dialect;

        public OnDeleteNoActionConstraint(IDialect dialect)
        {
            _dialect = dialect;
        }

        public override string ToString() => _dialect.OnDeleteNoActionConstraint;
    }
}