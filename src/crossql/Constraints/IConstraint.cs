namespace crossql.Constraints
{
    public interface IConstraint
    {
        /// <summary>
        /// Method used to generate the constraint string
        /// </summary>
        /// <returns><see cref="string"/> value for the constraint</returns>
        string ToString();
    }
}