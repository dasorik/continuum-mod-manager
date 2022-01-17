
namespace Continuum.Common
{
	public enum ValidationSeverity
	{
		None,
		Warning,
		Error
	}

	public class ValidationResponse
	{
		public ValidationSeverity Type { get; }
		public string Message { get; }

		public ValidationResponse(ValidationSeverity type)
			: this(type, "")
		{
		}

		public ValidationResponse(ValidationSeverity type, string message)
		{
			this.Type = type;
			this.Message = message;
		}

		public bool IsError => Type == ValidationSeverity.Error || Type == ValidationSeverity.Warning;

		public static ValidationResponse Success()
		{
			return new ValidationResponse(ValidationSeverity.None);
		}

		public static ValidationResponse Warning(string message)
		{
			return new ValidationResponse(ValidationSeverity.Warning, message);
		}

		public static ValidationResponse Error(string message)
		{
			return new ValidationResponse(ValidationSeverity.Error, message);
		}
	}
}
