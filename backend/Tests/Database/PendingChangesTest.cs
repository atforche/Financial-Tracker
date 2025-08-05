using Data;
using Microsoft.EntityFrameworkCore;

namespace Tests.Database;

/// <summary>
/// Test class that tests that there are no pending model changes for EF Core
/// </summary>
public class PendingChangesTest : TestClass
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    public void RunTest()
    {
        if (DatabaseFixture.ShouldUseDatabase)
        {
            Assert.False(GetService<DatabaseContext>().Database.HasPendingModelChanges());
        }
    }
}