using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data;

namespace WordleTracker.Svc.Tests;

public abstract class DbTests : IDisposable
{
	private const string InMemoryConnectionString = "DataSource=:memory:";

	protected readonly WordleTrackerContext DbContext;
	private readonly SqliteConnection _connection;

	protected DbTests()
	{
		_connection = new SqliteConnection(InMemoryConnectionString);
		_connection.Open();

		var options = new DbContextOptionsBuilder<WordleTrackerContext>()
			.UseSqlite(_connection)
			.Options;

		DbContext = new WordleTrackerContext(options);
		DbContext.Database.EnsureCreated();
	}

	public void Dispose()
	{
		DbContext.Database.EnsureDeleted();
		_connection.Close();
	}
}
