namespace API.Repositories;

using API.Models;

public class InMemoryClientRepository : IClientRepository
{
    private static readonly List<Client> Clients = SeedClients();
    private static readonly object Lock = new();

    public IReadOnlyList<Client> GetAll()
    {
        lock (Lock)
            return Clients.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Client? GetById(Guid id)
    {
        lock (Lock)
            return Clients.FirstOrDefault(c => c.Id == id);
    }

    public Client Add(Client client)
    {
        lock (Lock)
        {
            Clients.Add(client);
            return client;
        }
    }

    public bool Delete(Guid id)
    {
        lock (Lock)
        {
            var client = Clients.FirstOrDefault(c => c.Id == id);
            if (client is null) return false;
            Clients.Remove(client);
            return true;
        }
    }

    private static List<Client> SeedClients()
    {
        var now = DateTime.UtcNow;
        return
        [
            new Client
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                CompanyName = "Axis Bank",
                Industry = "Banking & Financial Services",
                PrimaryContact = "Rahul Sharma",
                Email = "axisbank-contacts@sample.com",
                Phone = "+91-98765-43210",
                Country = "India",
                Address = "Mumbai, India",
                Notes = "Preferred onboarding window: 2nd week of every month.",
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-10)
            },
            new Client
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                CompanyName = "ICICI Bank",
                Industry = "Banking & Financial Services",
                PrimaryContact = "Priya Singh",
                Email = "icici-contacts@sample.com",
                Phone = "+91-91234-56789",
                Country = "India",
                Address = "Mumbai, India",
                Notes = "Requires SAML SSO integration for all identity flows.",
                CreatedAt = now.AddDays(-40),
                UpdatedAt = now.AddDays(-15)
            },
            new Client
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222203"),
                CompanyName = "HDFC Bank",
                Industry = "Banking & Financial Services",
                PrimaryContact = "Amit Verma",
                Email = "hdfc-contacts@sample.com",
                Phone = "+91-99887-66554",
                Country = "India",
                Address = "Delhi, India",
                Notes = "Legacy system; coordinate with infra teams for UAT access.",
                CreatedAt = now.AddDays(-25),
                UpdatedAt = now.AddDays(-8)
            },
            new Client
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222204"),
                CompanyName = "Kotak Mahindra",
                Industry = "Banking & Financial Services",
                PrimaryContact = "Sneha Patel",
                Email = "kotak-contacts@sample.com",
                Phone = "+91-90011-22334",
                Country = "India",
                Address = "Bengaluru, India",
                Notes = "Connector type: API based. Expected completion: within 6 weeks.",
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-20)
            },
            new Client
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222205"),
                CompanyName = "Reliance",
                Industry = "Conglomerate",
                PrimaryContact = "Karan Shah",
                Email = "reliance-contacts@sample.com",
                Phone = "+91-95555-44444",
                Country = "India",
                Address = "Gurugram, India",
                Notes = "SSO required; confirm SAML endpoint details before UAT.",
                CreatedAt = now.AddDays(-35),
                UpdatedAt = now.AddDays(-14)
            },
        ];
    }
}

