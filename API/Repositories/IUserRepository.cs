namespace API.Repositories;

using API.Models;

public interface IUserRepository
{
    User? GetByEmail(string email);
    User Add(User user);
    bool Update(string email, Action<User> update);
}
