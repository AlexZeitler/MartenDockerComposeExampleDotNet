using System;
using System.IO;
using System.Threading.Tasks;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Common;
using Marten;
using Marten.Events.Projections;
using Npgsql;
using Weasel.Core;
using Xunit;

namespace MartenDockerCompose.Tests;

public class UserProjectionTests
{
  [Fact]
  public async Task ShouldProjectUserRegistration()
  {
    var file = Path.Combine(
      Directory.GetCurrentDirectory(),
      (TemplateString)"Resources/docker-compose.yml"
    );

    var service = new Builder()
      .UseContainer()
      .UseCompose()
      .FromFile(file)
      .RemoveOrphans()
      .ForceRecreate()
      .WaitForPort(
        "database",
        "5432/tcp",
        30000 /*30s*/
      )
      .Build();
    var container = service.Start();

    var PgTestConnectionString =
      "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; COMMANDTIMEOUT = 20; DATABASE = 'marten'; PASSWORD = '123456'; USER ID = 'marten'";
    using var store = DocumentStore.For(
      options =>
      {
        options.Connection(PgTestConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.All;
        options.Projections.SelfAggregate<User>(ProjectionLifecycle.Inline);
      }
    );

    var id = Guid.NewGuid();
    var username = "jane.doe@acme.inc";
    var userRegistered = new UserRegistered(
      id,
      username
    );

    User user;
    await using (var session = store.OpenSession())
    {
      session.Events.StartStream(
        id,
        userRegistered
      );

      await session.SaveChangesAsync();


      user = session.Load<User>(id);
      Assert.Equal(
        username,
        user?.Username
      );
      session.Connection.Close();
      await session.Connection.DisposeAsync();
      NpgsqlConnection.ClearAllPools();
    }

    service.Stop();
    service.Remove();
  }

  [Fact]
  public async Task ShouldProjectUserRegistrationSecond()
  {
    var file = Path.Combine(
      Directory.GetCurrentDirectory(),
      (TemplateString)"Resources/docker-compose.yml"
    );

    var service = new Builder()
      .UseContainer()
      .UseCompose()
      .FromFile(file)
      .RemoveOrphans()
      .ForceRecreate()
      .WaitForPort(
        "database",
        "5432/tcp",
        30000 /*30s*/
      )
      .Build();
    var container = service.Start();

    var PgTestConnectionString =
      "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; COMMANDTIMEOUT = 20; DATABASE = 'marten'; PASSWORD = '123456'; USER ID = 'marten'";
    using var store = DocumentStore.For(
      options =>
      {
        options.Connection(PgTestConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.All;
        options.Projections.SelfAggregate<User>(ProjectionLifecycle.Inline);
      }
    );

    var id = Guid.NewGuid();
    var username = "jane.doe@acme.inc";
    var userRegistered = new UserRegistered(
      id,
      username
    );

    User user = null;
    await using (var session = store.OpenSession())
    {
      session.Events.StartStream(
        id,
        userRegistered
      );

      await session.SaveChangesAsync();


      user = session.Load<User>(id);
      Assert.Equal(
        username,
        user?.Username
      );
      await session.Connection.DisposeAsync();
      NpgsqlConnection.ClearAllPools();
    }

    service.Stop();
    service.Remove();
  }
}
