namespace WordleTracker.Core.Utilities;

public static class Functional
{
	public static Func<T, bool> Complement<T>(Func<T, bool> f) => x => !f(x);

	public static Func<T, bool> All<T>(params Func<T, bool>[] functions) =>
		x => functions.All(f => f(x));

	public static Func<T, bool> AnyFail<T>(params Func<T, bool>[] functions) =>
		x => functions.Any(f => !f(x));
}
