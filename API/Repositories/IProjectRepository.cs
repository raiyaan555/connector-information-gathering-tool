namespace API.Repositories;

using API.Models;

public interface IProjectRepository
{
    IReadOnlyList<Project> GetAll();
    Project? GetById(Guid id);
    Project? GetByToken(string token);
    Project Add(Project project);
    Project Update(Project project);
    bool Delete(Guid id);
    int DeleteByClientName(string clientName);
}
