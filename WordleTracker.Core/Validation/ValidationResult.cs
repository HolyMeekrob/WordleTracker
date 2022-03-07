namespace WordleTracker.Core.Validation;

public class ValidationResult
{
	public bool IsValid { get; }
	public string Message { get; }

	private ValidationResult(bool isValid, string message)
	{
		IsValid = isValid;
		Message = message;
	}

	public static ValidationResult Success() =>
		new(true, string.Empty);

	public static ValidationResult Failure(string message) =>
		new(false, message);
}
public class ValidationResult<T>
{
	public bool IsValid { get; }

	public T Value { get; }

	public string Message { get; }

	private ValidationResult(bool isValid, T value, string message)
	{
		IsValid = isValid;
		Value = value;
		Message = message;
	}

	public static ValidationResult<T> Success(T value) =>
		new(true, value, string.Empty);

	public static ValidationResult<T> Failure(string message) =>
		new(false, default!, message);
}

