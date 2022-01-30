using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WordleTracker.Data.Models;

namespace WordleTracker.Data;

public class WordleTrackerContext : DbContext
{
	public DbSet<User> Users => Set<User>();
	public DbSet<Day> Days => Set<Day>();
	public DbSet<Result> Results => Set<Result>();
	public DbSet<Group> Groups => Set<Group>();
	public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

	public WordleTrackerContext(DbContextOptions<WordleTrackerContext> options) : base(options)
	{
	}

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		configurationBuilder
			.Properties<DateTimeOffset>()
			.HaveConversion<UniversalSortableTimeConverter>();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(WordleTrackerContext).Assembly);
	}

	private class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
	{
		// TODO: How to automate UpdatedDate
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder
				.HasMany(user => user.Groups)
				.WithMany(group => group.Members)
				.UsingEntity<GroupMember>(
					entity => entity
						.HasOne(gm => gm.Group)
						.WithMany(group => group.Memberships)
						.HasForeignKey(gm => gm.GroupId),
					entity => entity
						.HasOne(gm => gm.User)
						.WithMany(user => user.GroupMemberships)
						.HasForeignKey(gm => gm.UserId));
		}
	}

	private class ResultEntityTypeConfiguration : IEntityTypeConfiguration<Result>
	{
		public void Configure(EntityTypeBuilder<Result> builder)
		{
			builder.HasKey(result => new { result.UserId, result.DayId });
			builder.Property(result => result.Guesses).HasConversion(
				guessList => JsonSerializer.Serialize(guessList, default(JsonSerializerOptions)),
				guessList => JsonSerializer.Deserialize<ImmutableList<WordGuess>>(guessList, default(JsonSerializerOptions))!,
				new ValueComparer<ImmutableList<WordGuess>>(
					(a, b) => a!.SequenceEqual(b!),
					guesses => guesses.Aggregate(0, (hashes, value) => HashCode.Combine(hashes, value.GetHashCode())),
					guesses => guesses.ToImmutableList()
				)
			);
		}
	}

	private class GroupEntityTypeConfiguration : IEntityTypeConfiguration<Group>
	{
		public void Configure(EntityTypeBuilder<Group> builder)
		{
			builder.HasKey("_id");
			builder.Property("_id").HasColumnName("Id");
		}
	}

	// TODO: How to set UpdatedDate automatically
	private class GroupMemberEntityTypeConfiguration : IEntityTypeConfiguration<GroupMember>
	{
		public void Configure(EntityTypeBuilder<GroupMember> builder)
		{
			builder.HasKey(gm => new { gm.GroupId, gm.UserId });
			builder.Property(groupMember => groupMember.Role).HasConversion<string>();
		}
	}

	/// <summary>
	/// Converts DateTimeOffsets to/from ISO 8601 format when reading/writing to the database.
	/// </summary>
	private class UniversalSortableTimeConverter : ValueConverter<DateTimeOffset, string>
	{
		public UniversalSortableTimeConverter() : base(
				date => date.ToString("u"),
				dateStr => DateTimeOffset.Parse(dateStr)
			)
		{
		}
	}
}
