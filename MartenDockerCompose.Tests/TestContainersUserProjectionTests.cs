using System;
using System.IO;
using System.Threading.Tasks;
using Baseline.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Common;
using Marten;
using Marten.Events.Projections;
using Npgsql;
using Weasel.Core;
using Xunit;

namespace MartenDockerCompose.Tests;

public class TestContainersUserProjectionTests
{
  [Fact]
  public async Task ShouldProjectUserRegistration()
  {
    var service = new TestcontainersBuilder<TestcontainersContainer>()
      .WithImage("postgres:14")
      .WithEnvironment(
        "POSTGRES_USER",
        "marten"
      )
      .WithEnvironment(
        "POSTGRES_PASSWORD",
        "123456"
      )
      .WithEnvironment(
        "POSTGRES_DB",
        "marten"
      )
      .WithPortBinding(
        5432,
        5432
      )
      .Build();

    await service.StartAsync();


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

    await service.StopAsync();
    await service.DisposeAsync();
  }

  [Fact]
  public async Task ShouldProjectUserRegistrationSecond()
  {
    var service = new TestcontainersBuilder<TestcontainersContainer>()
      .WithImage("postgres:14")
      .WithEnvironment(
        "POSTGRES_USER",
        "marten"
      )
      .WithEnvironment(
        "POSTGRES_PASSWORD",
        "123456"
      )
      .WithEnvironment(
        "POSTGRES_DB",
        "marten"
      )
      .WithPortBinding(
        5432,
        5432
      )
      .Build();

    await service.StartAsync();

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

    await service.StopAsync();
    await service.DisposeAsync();
  }
}
