using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class CreateUserTests : DbTests
{
	[Fact]
	public async Task EmptyIdThrowsAnException()
	{
		var svc = new UserSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateUser(string.Empty, "Name", new CancellationToken())
		);
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceIdThrowsAnException(string id)
	{
		var svc = new UserSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateUser(id, "Name", new CancellationToken())
		);
	}

	[Fact]
	public async Task IsCreatedWithTheGivenId()
	{
		var svc = new UserSvc(DbContext);
		var id = "User Id";

		var returnedUser = await svc.CreateUser(id, string.Empty, new CancellationToken());
		var fetchedUser = await DbContext.Users.FindAsync(id);

		Assert.NotNull(fetchedUser);
		Assert.Equal(returnedUser.Id, fetchedUser!.Id);
	}

	[Fact]
	public async Task EmptyNameUsesId()
	{
		var svc = new UserSvc(DbContext);
		var id = "User Id";

		var returnedUser = await svc.CreateUser(id, string.Empty, new CancellationToken());
		var fetchedUser = await DbContext.Users.FindAsync(id);

		Assert.NotNull(fetchedUser);
		Assert.Equal(id, returnedUser.Name);
		Assert.Equal(id, fetchedUser!.Name);
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceNameUsesId(string name)
	{
		var svc = new UserSvc(DbContext);
		var id = "User Id";

		var returnedUser = await svc.CreateUser(id, name, new CancellationToken());
		var fetchedUser = await DbContext.Users.FindAsync(id);

		Assert.NotNull(fetchedUser);
		Assert.Equal(id, returnedUser.Name);
		Assert.Equal(id, fetchedUser!.Name);
	}

	[Fact]
	public async Task IsCreatedWithTheGivenName()
	{
		var svc = new UserSvc(DbContext);
		var id = "User Id";
		var name = "User Name";

		var returnedUser = await svc.CreateUser(id, name, new CancellationToken());
		var fetchedUser = await DbContext.Users.FindAsync(id);

		Assert.NotNull(fetchedUser);
		Assert.Equal(name, returnedUser.Name);
		Assert.Equal(returnedUser.Name, fetchedUser!.Name);
	}

	[Fact]
	public async Task HasMatchingCreatedDateAndUpdatedDate()
	{
		var svc = new UserSvc(DbContext);
		var id = "User Id";

		var returnedUser = await svc.CreateUser(id, string.Empty, new CancellationToken());
		var fetchedUser = await DbContext.Users.FindAsync(id);

		Assert.NotNull(fetchedUser);
		Assert.Equal(returnedUser.CreatedDate, returnedUser.UpdatedDate);
		Assert.Equal(fetchedUser!.CreatedDate, fetchedUser.UpdatedDate);
	}
}

