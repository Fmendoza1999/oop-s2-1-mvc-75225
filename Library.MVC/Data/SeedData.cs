using Bogus;
using Library.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Data
{
    public static class SeedData
    {
        public static async Task InitialiseAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

            await context.Database.MigrateAsync();

            // Seed Admin role
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));

            // Seed Admin user
            if (await userManager.FindByEmailAsync("admin@library.com") == null)
            {
                var admin = new Microsoft.AspNetCore.Identity.IdentityUser
                {
                    UserName = "admin@library.com",
                    Email = "admin@library.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (await context.Books.AnyAsync()) return;

            // Seed 20 Books
            var categories = new[] { "Fiction", "Science", "History", "Technology", "Biography" };
            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
                .RuleFor(b => b.Category, f => f.PickRandom(categories))
                .RuleFor(b => b.IsAvailable, _ => true);

            var books = bookFaker.Generate(20);
            await context.Books.AddRangeAsync(books);

            // Seed 10 Members
            var memberFaker = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

            var members = memberFaker.Generate(10);
            await context.Members.AddRangeAsync(members);

            await context.SaveChangesAsync();

            // Seed 15 Loans
            var random = new Random();
            var today = DateTime.Today;
            var loans = new List<Loan>();

            for (int i = 0; i < 15; i++)
            {
                var book = books[i % books.Count];
                var member = members[random.Next(members.Count)];
                var loanDate = today.AddDays(-random.Next(1, 60));
                var dueDate = loanDate.AddDays(14);

                DateTime? returnedDate = null;

                if (i < 5) // 5 returned loans
                {
                    returnedDate = loanDate.AddDays(random.Next(1, 14));
                    book.IsAvailable = true;
                }
                else if (i < 10) // 5 active loans
                {
                    book.IsAvailable = false;
                }
                // last 5 are overdue (dueDate < today, not returned)

                loans.Add(new Loan
                {
                    Book = book,
                    Member = member,
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    ReturnedDate = returnedDate
                });
            }

            await context.Loans.AddRangeAsync(loans);
            await context.SaveChangesAsync();
        }
    }
}