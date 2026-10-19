using System;
using System.IO;
using System.Threading.Tasks;
using DockerCompose;
using Marten;
using Marten.Events.Projections;
using Npgsql;
using Weasel.Core;
using Xunit;
using static WaitForPort.Ports;

namespace MartenDockerCompose.Tests;

public class UnitTest1
{
  [Fact]
  public async Task Test1()
  {
    var directory = new DirectoryInfo(
      Path.Combine(
        Directory.GetCurrentDirectory(),
        "Resources/"
      )
    );

    var compose = new Compose(directory);

    var result = await compose.Up();
    Console.WriteLine(result.StdErr);
    Console.WriteLine(result.StdOut);

    WaitForTcpPort(
      5432,
      10000
    );


    var PgTestConnectionString =
      "PORT=5432; HOST=localhost; TIMEOUT=15; POOLING=false; COMMANDTIMEOUT=20; DATABASE='marten'; PASSWORD='123456'; USER ID='marten'";
    using (var store = DocumentStore.For(
             options =>
             {
               options.Connection(PgTestConnectionString);
               options.AutoCreateSchemaObjects = AutoCreate.All;
               options.Projections.SelfAggregate<User>(ProjectionLifecycle.Inline);
             }
           ))

    {
      var id = Guid.NewGuid();
      var username = "jane.doe@acme.inc";
      var userRegistered = new UserRegistered(
        id,
        username
      );

      await using (var session = store.OpenSession())
      {
        session.Events.StartStream(
          id,
          userRegistered
        );

        await session.SaveChangesAsync();


        var user = await session.LoadAsync<User>(id);

        Assert.Equal(
          username,
          user?.Username
        );

        await session.Connection.CloseAsync();
        await session.Connection.DisposeAsync();
        store.Dispose();
        NpgsqlConnection.ClearAllPools();
      }
    }

    var composeResult = await compose.Down();
    Console.WriteLine(composeResult.StdErr);
    Console.WriteLine(composeResult.StdOut);
  }

  [Fact]
  public async Task Test2()
  {
    var directory = new DirectoryInfo(
      Path.Combine(
        Directory.GetCurrentDirectory(),
        "Resources/"
      )
    );

    var compose = new Compose(directory);

    var result = await compose.Up();
    Console.WriteLine(result.StdErr);
    Console.WriteLine(result.StdOut);
    WaitForTcpPort(
      5432,
      10000
    );


    var PgTestConnectionString =
      "PORT=5432; HOST=localhost; TIMEOUT=15; POOLING=false; COMMANDTIMEOUT=20; DATABASE='marten'; PASSWORD='123456'; USER ID='marten'";
    using (var store = DocumentStore.For(
             options =>
             {
               options.Connection(PgTestConnectionString);
               options.AutoCreateSchemaObjects = AutoCreate.All;
               options.Projections.SelfAggregate<User>(ProjectionLifecycle.Inline);
             }
           ))
    {
      var id = Guid.NewGuid();
      var username = "jane.doe@acme.inc";
      var userRegistered = new UserRegistered(
        id,
        username
      );

      await using (var session = store.OpenSession())
      {
        session.Events.StartStream(
          id,
          userRegistered
        );

        await session.SaveChangesAsync();


        var user = await session.LoadAsync<User>(id);
        Assert.Equal(
          username,
          user?.Username
        );
        await session.Connection.CloseAsync();
        await session.Connection.DisposeAsync();
        store.Dispose();
        NpgsqlConnection.ClearAllPools();
      }
    }

    var composeResult = await compose.Down();
    Console.WriteLine(composeResult.StdErr);
    Console.WriteLine(composeResult.StdOut);
  }
}
