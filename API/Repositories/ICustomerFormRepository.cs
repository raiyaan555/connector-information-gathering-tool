namespace API.Repositories;

using API.Models;

public interface ICustomerFormRepository
{
    CustomerFormResponse? GetByToken(string token);
    IEnumerable<CustomerFormResponse> GetByProjectId(Guid projectId);
    CustomerFormResponse Add(CustomerFormResponse response);
    bool IsSubmitted(string token);
}
