using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.Persistence.Extensions;
using Peerly.Auth.Persistence.Repositories.EmailVerifications;
using Peerly.Auth.Persistence.Repositories.Sessions;
using Peerly.Auth.Persistence.Repositories.UserRoles;
using Peerly.Auth.Persistence.Repositories.Users;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.Persistence.UnitOfWork;

[ExcludeFromCodeCoverage]
internal sealed class UnitOfWorkInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommonUnitOfWorkFactory, CommonUnitOfWorkFactory>();
        services.AddUnitOfWorkInnerFactory<CommonUnitOfWork>();
        services
            .AddOptions<ConnectionFactoryOptions>()
            .BindConfiguration(ConnectionFactoryOptions.SectionName);

        services.AddRepositoryFactory<IUserRepository, UserRepository>();
        services.AddRepositoryFactory<IUserRoleRepository, UserRoleRepository>();
        services.AddRepositoryFactory<IEmailVerificationRepository, EmailVerificationRepository>();
        services.AddRepositoryFactory<ISessionRepository, SessionRepository>();

        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<ConnectionFactoryOptions>>().Value;

            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = opt.MasterHost,
                Port = opt.DefaultPort,
                Database = opt.Database,
                Username = opt.UserName,
                Password = opt.Password,
                Pooling = true,
            };

            return NpgsqlDataSource.Create(csb.ConnectionString);
        });

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
