namespace API.Repositories;

using API.Models;

public class InMemoryProjectRepository : IProjectRepository
{
    private static readonly List<Project> Projects = SeedProjects();
    private static readonly object Lock = new();

    public IReadOnlyList<Project> GetAll()
    {
        lock (Lock) return Projects.OrderByDescending(p => p.CreatedAt).ToList();
    }

    public Project? GetById(Guid id)
    {
        lock (Lock) return Projects.FirstOrDefault(p => p.Id == id);
    }

    public Project? GetByToken(string token)
    {
        lock (Lock) return Projects.FirstOrDefault(p => p.FormToken == token);
    }

    public Project Add(Project project)
    {
        lock (Lock)
        {
            Projects.Add(project);
            return project;
        }
    }

    public Project Update(Project project)
    {
        lock (Lock)
        {
            project.UpdatedAt = DateTime.UtcNow;
            return project;
        }
    }

    public bool Delete(Guid id)
    {
        lock (Lock)
        {
            var project = Projects.FirstOrDefault(p => p.Id == id);
            if (project is null) return false;
            Projects.Remove(project);
            return true;
        }
    }

    public int DeleteByClientName(string clientName)
    {
        lock (Lock)
        {
            var toRemove = Projects
                .Where(p => string.Equals(p.ClientName, clientName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var project in toRemove)
                Projects.Remove(project);
            return toRemove.Count;
        }
    }

    private static List<Project> SeedProjects()
    {
        var now = DateTime.UtcNow;
        return
        [
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "GitLab Integration", ClientName = "Acme Corp", ApplicationName = "GitLab", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-14), UpdatedAt = now.AddDays(-2), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Salesforce IAM", ClientName = "GlobalTech Inc", ApplicationName = "Salesforce", Status = ProjectStatus.Completed, FormToken = "a1b2c3d4-e5f6-7890-abcd-ef1234567890", FormLink = "http://localhost:4200/customer-form/a1b2c3d4-e5f6-7890-abcd-ef1234567890", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-5), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Workday Connector", ClientName = "Enterprise Solutions", ApplicationName = "Workday", Status = ProjectStatus.PendingReview, CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-1), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "ServiceNow SSO", ClientName = "Nova Industries", ApplicationName = "ServiceNow", Status = ProjectStatus.Draft, CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Azure AD Sync", ClientName = "CloudFirst Ltd", ApplicationName = "Azure Active Directory", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-21), UpdatedAt = now.AddDays(-4), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111106"), Name = "SAP ERP Integration", ClientName = "Manufacturing Co", ApplicationName = "SAP ERP", Status = ProjectStatus.Completed, CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-10), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111107"), Name = "Okta Federation", ClientName = "SecureNet", ApplicationName = "Okta", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-1), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111108"), Name = "Jira Lifecycle", ClientName = "DevOps Team", ApplicationName = "Atlassian Jira", Status = ProjectStatus.PendingReview, CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-1), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111109"), Name = "Oracle HCM", ClientName = "HR Solutions", ApplicationName = "Oracle HCM Cloud", Status = ProjectStatus.Draft, CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2), CreatedBy = "admin@theconnector.com" },
            new Project { Id = Guid.Parse("11111111-1111-1111-1111-111111111110"), Name = "AWS IAM Connector", ClientName = "CloudScale", ApplicationName = "AWS IAM", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-3), CreatedBy = "admin@theconnector.com" },
        ];
    }
}
