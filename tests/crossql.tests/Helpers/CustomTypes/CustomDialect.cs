namespace crossql.tests.Helpers.CustomTypes
{
    public class CustomDialect : ICustomDialect
    {
        public string LatLong => "double(9, 6)";
    }

    public interface ICustomDialect
    {
        string LatLong { get; }
    }
}