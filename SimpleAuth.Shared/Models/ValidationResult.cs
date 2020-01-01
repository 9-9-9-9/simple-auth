namespace SimpleAuth.Shared.Models
{
    public struct ValidationResult
    {
        public bool IsValid;
        public string Message;

        public static ValidationResult Valid()
        {
            return new ValidationResult
            {
                IsValid = true,
            };
        }

        public static ValidationResult Invalid(string message)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = message
            };
        }
    }
}