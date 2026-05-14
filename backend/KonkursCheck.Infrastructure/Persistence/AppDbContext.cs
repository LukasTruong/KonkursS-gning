using KonkursCheck.Domain.Entities;
using KonkursCheck.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KonkursCheck.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<PersonCompanyRole> PersonCompanyRoles => Set<PersonCompanyRole>();
    public DbSet<BankruptcySummary> BankruptcySummaries => Set<BankruptcySummary>();

    private static CompanyStatus ParseCompanyStatus(string v) =>
        Enum.TryParse<CompanyStatus>(v, true, out var r) ? r : CompanyStatus.Unknown;

    private static RoleType ParseRoleType(string v) =>
        Enum.TryParse<RoleType>(v, true, out var r) ? r : RoleType.Other;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<Person>(e =>
        {
            e.ToTable("persons");
            e.HasKey(p => p.PersonCvrId);
            e.Property(p => p.PersonCvrId).HasColumnName("person_cvr_id").HasMaxLength(20);
            e.Property(p => p.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            e.Property(p => p.LastUpdated).HasColumnName("last_updated");
            e.HasIndex(p => p.FullName)
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops")
                .HasDatabaseName("idx_persons_name");
        });

        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies");
            e.HasKey(c => c.CvrNumber);
            e.Property(c => c.CvrNumber).HasColumnName("cvr_number").HasMaxLength(8);
            e.Property(c => c.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            e.Property(c => c.Status).HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToUpperInvariant(),
                    v => ParseCompanyStatus(v));
            e.Property(c => c.FoundedDate).HasColumnName("founded_date");
            e.Property(c => c.BankruptcyDate).HasColumnName("bankruptcy_date");
            e.Property(c => c.DissolutionDate).HasColumnName("dissolution_date");
            e.Property(c => c.IndustryCode).HasColumnName("industry_code").HasMaxLength(10);
            e.Property(c => c.LastUpdated).HasColumnName("last_updated");
            e.HasIndex(c => c.Name)
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops")
                .HasDatabaseName("idx_companies_name");
        });

        modelBuilder.Entity<PersonCompanyRole>(e =>
        {
            e.ToTable("person_company_roles");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            e.Property(r => r.PersonCvrId).HasColumnName("person_cvr_id").HasMaxLength(20);
            e.Property(r => r.CvrNumber).HasColumnName("cvr_number").HasMaxLength(8);
            e.Property(r => r.Role).HasColumnName("role")
                .HasConversion(
                    v => v.ToString().ToUpperInvariant(),
                    v => ParseRoleType(v));
            e.Property(r => r.StartDate).HasColumnName("start_date");
            e.Property(r => r.EndDate).HasColumnName("end_date");
            e.HasOne(r => r.Person).WithMany(p => p.Roles).HasForeignKey(r => r.PersonCvrId);
            e.HasOne(r => r.Company).WithMany(c => c.PersonRoles).HasForeignKey(r => r.CvrNumber);
            e.HasIndex(r => r.PersonCvrId).HasDatabaseName("idx_roles_person");
            e.HasIndex(r => r.CvrNumber).HasDatabaseName("idx_roles_company");
        });

        modelBuilder.Entity<BankruptcySummary>(e =>
        {
            e.ToTable("bankruptcy_summaries");
            e.HasKey(s => s.PersonCvrId);
            e.Property(s => s.PersonCvrId).HasColumnName("person_cvr_id").HasMaxLength(20);
            e.Property(s => s.TotalBankruptcies).HasColumnName("total_bankruptcies").HasDefaultValue(0);
            e.Property(s => s.MostRecentDate).HasColumnName("most_recent_date");
            e.Property(s => s.CompanyNames).HasColumnName("company_names").HasColumnType("text[]");
            e.Property(s => s.LastCalculated).HasColumnName("last_calculated");
            e.HasOne(s => s.Person).WithOne(p => p.BankruptcySummary).HasForeignKey<BankruptcySummary>(s => s.PersonCvrId);
            e.HasIndex(s => s.TotalBankruptcies).IsDescending().HasDatabaseName("idx_summary_bankruptcies");
        });
    }
}
