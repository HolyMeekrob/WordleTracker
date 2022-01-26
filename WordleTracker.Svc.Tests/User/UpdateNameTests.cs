using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class UpdateNameTests : DbTests
{
	[Fact]
	public async Task NonExistentIdThrowsAnException()
	{
		var svc = new UserSvc(DbContext);

		await Assert.ThrowsAsync<KeyNotFoundException>(() =>
			svc.UpdateName(" ", "New name", new CancellationToken()));
	}

	[Fact]
	public async Task EmptyNameThrowsAnException()
	{
		var svc = new UserSvc(DbContext);

		var user = new User()
		{
			Id = "User Id",
			Name = "User Name",
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		};
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.UpdateName(user.Id, string.Empty, new CancellationToken()));
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceNameThrowsAnException(string name)
	{
		var svc = new UserSvc(DbContext);

		var user = new User()
		{
			Id = "User Id",
			Name = "User Name",
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		};
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.UpdateName(user.Id, name, new CancellationToken()));
	}

	[Fact]
	public async Task ReturnsTheUpdatedUser()
	{
		var newName = "New name";
		var svc = new UserSvc(DbContext);

		var user = new User()
		{
			Id = "User Id",
			Name = "User Name",
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		};
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		var result = await svc.UpdateName(user.Id, newName, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(newName, result.Name);
	}

	[Fact]
	public async Task UpdatesTheUserWithTheGivenName()
	{
		var newName = "New name";
		var svc = new UserSvc(DbContext);

		var user = new User()
		{
			Id = "User Id",
			Name = "User Name",
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		};
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		await svc.UpdateName(user.Id, newName, new CancellationToken());

		var dbUser = await DbContext.FindAsync<User>(user.Id);
		Assert.NotNull(dbUser);
		Assert.Equal(newName, dbUser!.Name);
	}
}
