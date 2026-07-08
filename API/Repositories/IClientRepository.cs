namespace API.Repositories;

using API.Models;

public interface IClientRepository
{
    IReadOnlyList<Client> GetAll();
    Client? GetById(Guid id);
    Client Add(Client client);
    bool Delete(Guid id);
}

