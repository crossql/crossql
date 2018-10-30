﻿using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using NUnit.Framework;
using FluentAssertions;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class InflectionTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Do_Crud_On_Model_Object_With_Inflected_Pluralization(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));
            
            // Execute Create
            var firstGoose = GooseFixture.GetFirstGoose();
            var gooseToUpdate = GooseFixture.GetGooseToUpdate();
            var gooseToDelete = GooseFixture.GetGooseToDelete();

            await db.Create(firstGoose);
            await db.Create(gooseToUpdate);
            await db.Create(gooseToDelete);
            var actualGoose = await db.Query<GooseEntity>().Where(goose => goose.Id == firstGoose.Id).FirstOrDefaultAsync();
            
            // Assert Create
            actualGoose.Should().NotBeNull();
            actualGoose.Should().BeEquivalentTo(firstGoose);
            
            // Execute Find IEnumerable
            var actualGeese = await db.Query<GooseEntity>().Where(x => x.Name.Contains("irst")).Select();
            actualGeese.Should().NotBeNullOrEmpty();
            
            // Execute Find List
            var actualGeese2 = (await db.Query<GooseEntity>().Where(x => x.Name.Contains("Goose")).Select()).ToList();
            actualGeese2.Should().HaveCount(3);
            
            // Execute Update
            gooseToUpdate.Name = "Canada Goose";
            await db.Update(gooseToUpdate);
            
            var actualUpdatedGoose = (await db.Query<GooseEntity>().Where(x => x.Id == gooseToUpdate.Id).Select()).FirstOrDefault();
            actualUpdatedGoose.Should().NotBeNull();
            actualUpdatedGoose.Should().BeEquivalentTo(gooseToUpdate);
            
            // Execute Delete
            await db.Query<GooseEntity>().Where(u => u.Id == gooseToDelete.Id).Delete();
            var actualDeletedGoose = await db.Query<GooseEntity>().Where(x => x.Id == gooseToDelete.Id).FirstOrDefaultAsync();

            actualDeletedGoose.Should().BeNull();
            
            await db.Query<GooseEntity>().Truncate();
            
            var emptyResults = await db.Query<GooseEntity>().Select();
            emptyResults.Should().BeEmpty();
        }
    }
}