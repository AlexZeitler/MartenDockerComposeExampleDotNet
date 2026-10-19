using System;

namespace MartenDockerCompose.Tests;

public class UserRegistered
{
  public UserRegistered(
    Guid id,
    string username
  )
  {
    Id = id;
    Username = username;
  }

  public Guid Id { get; private set; }
  public string Username { get; private set; }
}
