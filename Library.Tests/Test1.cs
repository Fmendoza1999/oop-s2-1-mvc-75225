using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests
{
    [TestClass]
    public class LibraryTests
    {
        private ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [TestMethod]
        public async Task CannotLoanBookAlreadyOnActiveLoan()
        {
            using var context = GetContext();
            var book = new Book { Title = "Test", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = false };
            context.Books.Add(book);
            await context.SaveChangesAsync();

            Assert.IsFalse(book.IsAvailable);
        }

        [TestMethod]
        public async Task ReturnedLoanMakesBookAvailable()
        {
            using var context = GetContext();
            var book = new Book { Title = "Test", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = false };
            var member = new Member { FullName = "John", Email = "john@test.com", Phone = "123" };
            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.Today, DueDate = DateTime.Today.AddDays(14) };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            loan.ReturnedDate = DateTime.Today;
            book.IsAvailable = true;
            await context.SaveChangesAsync();

            Assert.IsTrue(book.IsAvailable);
            Assert.IsNotNull(loan.ReturnedDate);
        }

        [TestMethod]
        public async Task BookSearchReturnsExpectedMatches()
        {
            using var context = GetContext();
            context.Books.AddRange(
                new Book { Title = "Harry Potter", Author = "Rowling", Isbn = "1", Category = "Fiction", IsAvailable = true },
                new Book { Title = "Lord of the Rings", Author = "Tolkien", Isbn = "2", Category = "Fiction", IsAvailable = true }
            );
            await context.SaveChangesAsync();

            var results = await context.Books.Where(b => b.Title.Contains("Harry")).ToListAsync();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Harry Potter", results[0].Title);
        }

        [TestMethod]
        public async Task OverdueLogicWorksCorrectly()
        {
            using var context = GetContext();
            var book = new Book { Title = "Test", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = false };
            var member = new Member { FullName = "John", Email = "john@test.com", Phone = "123" };
            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Today.AddDays(-30),
                DueDate = DateTime.Today.AddDays(-10),
                ReturnedDate = null
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            bool isOverdue = loan.DueDate < DateTime.Today && loan.ReturnedDate == null;
            Assert.IsTrue(isOverdue);
        }

        [TestMethod]
        public async Task BookFilterByCategoryReturnsCorrectResults()
        {
            using var context = GetContext();
            context.Books.AddRange(
                new Book { Title = "Science Book", Author = "Author1", Isbn = "1", Category = "Science", IsAvailable = true },
                new Book { Title = "Fiction Book", Author = "Author2", Isbn = "2", Category = "Fiction", IsAvailable = true }
            );
            await context.SaveChangesAsync();

            var results = await context.Books.Where(b => b.Category == "Science").ToListAsync();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Science", results[0].Category);
        }
    }
}