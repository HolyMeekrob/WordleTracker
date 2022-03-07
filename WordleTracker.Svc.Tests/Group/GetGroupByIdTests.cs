using System.Threading;
using System.Threading.Tasks;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class GetGroupByIdTests : DbTests
{
	[Fact]
	public async Task InvalidIdReturnsNull()
	{
		var group = new Group
		{
			Name = "Group Name"
		};

		DbContext.Groups.Add(group);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.GetGroupById(group.Id + 1, new CancellationToken());

		Assert.Null(result);
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
		Assert.Equal(name, result!.Name);
	}
}
