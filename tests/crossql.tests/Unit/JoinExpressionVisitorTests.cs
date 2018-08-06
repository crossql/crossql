﻿using System;
using System.Linq.Expressions;
using crossql.mssqlserver;
using crossql.tests.Helpers.Models;
using NUnit.Framework;

namespace crossql.tests.Unit
{
    [TestFixture]
    public class JoinExpressionVisitorTests
    {
        [Test]
        public void ShouldBuildInnerJoinExpression()
        {
            // Setup
            const string expectedString = "[Books].[PublisherId] = [Publishers].[Id]";
            var actualExpression = JoinExpression<BookModel, PublisherModel>((b, p) => 
                b.Publisher.Id == p.Id);

            // Execute
            var actualString = actualExpression.JoinExpression;

            // Test
            Assert.AreEqual(expectedString, actualString);
        }

        [Test]
        public void ShouldBuildLeftJoinExpression()
        {
            // Setup
            const string expectedString = "[Publishers].[Id] = [Books].[PublisherId]";
            var actualExpression = JoinExpression<BookModel, PublisherModel>((b, p) => 
                p.Id == b.Publisher.Id);

            // Execute
            var actualString = actualExpression.JoinExpression;

            // Test
            Assert.AreEqual(expectedString, actualString);
        }

        private static JoinExpressionVisitor JoinExpression<TModel, TJoinTo>(Expression<Func<TModel, TJoinTo, object>> joinExpression) where TModel : class, new() => new JoinExpressionVisitor(new SqlServerDialect()).Visit(joinExpression);
    }
}