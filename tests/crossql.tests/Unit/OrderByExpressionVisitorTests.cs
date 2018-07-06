using System;
using System.Linq.Expressions;
using crossql.mssqlserver;
using crossql.tests.Helpers.Models;
using NUnit.Framework;

namespace crossql.tests.Unit
{
    [TestFixture]
    public class OrderByExpressionVisitorTests
    {
        public static OrderByExpressionVisitor OrderByExpression<TModel>(Expression<Func<TModel, object>> expression)
            where TModel : class, new()
        {
            var visitor = new OrderByExpressionVisitor(new SqlServerDialect()).Visit(expression);
            return visitor;
        }

        [Test]
        public void ShouldBuildTheOrderExpressionFeildList()
        {
            // Setup
            const string expectedString = "[Authors].[Email]";
            var actualExpression = OrderByExpression<AuthorModel>(u => u.Email);

            // Execute
            var actualString = actualExpression.OrderByExpression;

            // Test
            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void ShouldBuildTheOrderExpressionFeildListForDateTime()
        {
            // Setup
            const string expectedString = "[Authors].[CreatedDate]";
            var actualExpression = OrderByExpression<AuthorModel>(u => u.CreatedDate);

            // Execute
            var actualString = actualExpression.OrderByExpression;

            // Test
            Assert.AreEqual(expectedString, actualString);
        }
    }
}