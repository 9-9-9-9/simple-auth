using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Validation
{
    public interface IUserValidationService
    {
        ValidationResult IsValidUserId(string userId);
        ValidationResult IsValidPassword(string password);
        ValidationResult IsValidEmail(string email);
    }

    public class DefaultUserValidationService : IUserValidationService
    {
        private readonly IFilter<string> _filterUid = new FilterGoodUserId();
        private readonly IFilter<string> _filterPwd = new FilterGoodPassword();
        private readonly IFilter<string> _filterEml = new FilterEmail();

        public ValidationResult IsValidUserId(string userId)
        {
            return _filterUid.IsValid(userId);
        }

        public ValidationResult IsValidPassword(string password)
        {
            return _filterPwd.IsValid(password);
        }

        public ValidationResult IsValidEmail(string email)
        {
            return _filterEml.IsValid(email);
        }
    }
}