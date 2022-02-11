using System;
using System.Threading;
using System.Threading.Tasks;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class GetGroupByIdTests : DbTests
{
	[Fact]
	public async Task InvalidIdThrowsAnException()
	{
		var group = new Group
		{
			Name = "Group Name"
		};

		DbContext.Groups.Add(group);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			svc.GetGroupById(group.Id + 1, new CancellationToken()));
	}

	[Fact]
	public async Task ValidIdReturnsGroup()
	{
		var name = "Group Name";
		var group = new Group
		{
			Name = name
		};

		DbContext.Groups.Add(group);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.GetGroupById(group.Id, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(name, result.Name);
	}
}
