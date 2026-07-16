using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await context.Database.MigrateAsync(cancellationToken);

        await EnsureRolesAsync(roleManager, cancellationToken);
        await EnsureAdminUserAsync(userManager, logger, cancellationToken);
        await EnsureSampleDataAsync(context, logger, cancellationToken);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, CancellationToken ct)
    {
        foreach (var role in new[] { "Admin", "Engineer", "Viewer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken ct)
    {
        const string email = "admin@arconnet.com";
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = email,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Password123");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, "Admin");
        logger.LogInformation("Seeded admin user {Email}", email);
    }

    private static async Task EnsureSampleDataAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken ct)
    {
        if (await context.Clients.AnyAsync(ct))
            return;

        var now = DateTime.UtcNow;
        var clients = new List<Client>
        {
            new()
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
            new()
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
            new()
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
            new()
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
            new()
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
        };

        // Align project ClientName with seeded clients so dashboard grouping works.
        var projects = new List<Project>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "GitLab Integration", ClientId = clients[0].Id, ClientName = clients[0].CompanyName, ApplicationName = "GitLab", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-14), UpdatedAt = now.AddDays(-2), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Salesforce IAM", ClientId = clients[1].Id, ClientName = clients[1].CompanyName, ApplicationName = "Salesforce", Status = ProjectStatus.Completed, FormToken = "a1b2c3d4-e5f6-7890-abcd-ef1234567890", FormLink = "http://localhost:4200/form/a1b2c3d4-e5f6-7890-abcd-ef1234567890", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-5), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Workday Connector", ClientId = clients[2].Id, ClientName = clients[2].CompanyName, ApplicationName = "Workday", Status = ProjectStatus.PendingReview, CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-1), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "ServiceNow SSO", ClientId = clients[3].Id, ClientName = clients[3].CompanyName, ApplicationName = "ServiceNow", Status = ProjectStatus.Draft, CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Azure AD Sync", ClientId = clients[4].Id, ClientName = clients[4].CompanyName, ApplicationName = "Azure Active Directory", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-21), UpdatedAt = now.AddDays(-4), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111106"), Name = "SAP ERP Integration", ClientId = clients[0].Id, ClientName = clients[0].CompanyName, ApplicationName = "SAP ERP", Status = ProjectStatus.Completed, CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-10), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111107"), Name = "Okta Federation", ClientId = clients[1].Id, ClientName = clients[1].CompanyName, ApplicationName = "Okta", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-1), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111108"), Name = "Jira Lifecycle", ClientId = clients[2].Id, ClientName = clients[2].CompanyName, ApplicationName = "Atlassian Jira", Status = ProjectStatus.PendingReview, CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-1), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111109"), Name = "Oracle HCM", ClientId = clients[3].Id, ClientName = clients[3].CompanyName, ApplicationName = "Oracle HCM Cloud", Status = ProjectStatus.Draft, CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2), CreatedBy = "admin@arconnet.com" },
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111110"), Name = "AWS IAM Connector", ClientId = clients[4].Id, ClientName = clients[4].CompanyName, ApplicationName = "AWS IAM", Status = ProjectStatus.InProgress, CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-3), CreatedBy = "admin@arconnet.com" },
        };

        var form = new CustomerForm
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
            ProjectId = projects[1].Id,
            Token = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            FormData = new Dictionary<string, string>
            {
                ["applicationPurpose"] = "CRM and sales lifecycle management",
                ["applicationType"] = "API Based",
                ["ciPackage"] = "CI 10.05.000"
            },
            SubmittedAt = now.AddDays(-5)
        };

        var attachments = new List<Attachment>
        {
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444401"),
                ProjectId = projects[0].Id,
                FileName = "gitlab-api-docs.pdf",
                ContentType = "application/pdf",
                FileSize = 204800,
                UploadedAt = now.AddDays(-10),
                UploadedBy = "admin@arconnet.com"
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444402"),
                ProjectId = projects[0].Id,
                FileName = "gitlab-postman.json",
                ContentType = "application/json",
                FileSize = 15360,
                UploadedAt = now.AddDays(-9),
                UploadedBy = "admin@arconnet.com"
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444403"),
                ProjectId = projects[1].Id,
                FileName = "salesforce-architecture.png",
                ContentType = "image/png",
                FileSize = 512000,
                UploadedAt = now.AddDays(-6),
                UploadedBy = "admin@arconnet.com"
            },
        };

        context.Clients.AddRange(clients);
        context.Projects.AddRange(projects);
        context.CustomerForms.Add(form);
        context.Attachments.AddRange(attachments);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seeded sample clients, projects, forms, and attachments.");
    }
}
