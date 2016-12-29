using Librarian.Core.Data;
using Xunit;

namespace Librarian.Core.Tests.RepositoryUnitTests
{
    public class EssentialTests : TestsBase
    {
        [Fact(DisplayName = "Repository construction")]
        public void RepositoryConstructionTest()
        {
            using (var context = CreateTestContext())
            {
                // ReSharper disable once UnusedVariable
                var repository = new LibrarianRepository(context);
            }
        }

        [Fact(DisplayName = "RepositoryOperationException construction")]
        public void RepositoryOperationExceptionConstructionTest()
        {
            var key = "key";
            var message = "message";

            var ex = new RepositoryOperationException(key, message);

            Assert.Equal(key, ex.Key);
            Assert.Equal(message, ex.Message);
        }

        [Fact(DisplayName = "LibrarianContext initialization")]
        public void ContextInitializationTest()
        {
            using (var context = CreateTestContext())
            {
                // first
                LibrarianContextInitializer.Initialize(context);

                // subsequent (actually will be skipped)
                LibrarianContextInitializer.Initialize(context);
            }
        }
    }
}
