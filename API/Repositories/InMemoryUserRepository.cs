namespace API.Repositories;

using API.Models;

public class InMemoryUserRepository : IUserRepository
{
    private static readonly List<User> Users =
    [
        new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@theconnector.com",
            Password = "Password123",
            FirstName = "Admin",
            LastName = "User",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        }
    ];
    private static readonly object Lock = new();

    public User? GetByEmail(string email)
    {
        lock (Lock) return Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public User Add(User user)
    {
        lock (Lock)
        {
            Users.Add(user);
            return user;
        }
    }

    public bool Update(string email, Action<User> update)
    {
        lock (Lock)
        {
            var user = Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user is null) return false;
            update(user);
            return true;
        }
    }
}
