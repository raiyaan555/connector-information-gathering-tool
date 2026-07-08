namespace API.Repositories;

using API.Models;

public class InMemoryCustomerFormRepository : ICustomerFormRepository
{
    private static readonly List<CustomerFormResponse> Responses =
    [
        new CustomerFormResponse
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
            ProjectId = Guid.Parse("11111111-1111-1111-1111-111111111102"),
            Token = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            FormData = new Dictionary<string, string>
            {
                ["contactName"] = "Jane Smith",
                ["contactEmail"] = "jane.smith@globaltech.com",
                ["apiEndpoint"] = "https://api.globaltech.com/v2",
                ["environment"] = "Production"
            },
            SubmittedAt = DateTime.UtcNow.AddDays(-5)
        }
    ];
    private static readonly object Lock = new();

    public CustomerFormResponse? GetByToken(string token)
    {
        lock (Lock) return Responses.FirstOrDefault(r => r.Token == token);
    }

    public IEnumerable<CustomerFormResponse> GetByProjectId(Guid projectId)
    {
        lock (Lock)
        {
            return Responses
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToList();
        }
    }

    public CustomerFormResponse Add(CustomerFormResponse response)
    {
        lock (Lock)
        {
            Responses.Add(response);
            return response;
        }
    }

    public bool IsSubmitted(string token)
    {
        lock (Lock) return Responses.Any(r => r.Token == token);
    }
}
