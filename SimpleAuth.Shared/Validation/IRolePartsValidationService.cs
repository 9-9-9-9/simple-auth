using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Validation
{
    public interface IRolePartsValidationService
    {
        ValidationResult IsValidCorp(string corp);
        ValidationResult IsValidApp(string app);
    }
    
    public class DefaultRolePartsValidationService : IRolePartsValidationService
    {
        public ValidationResult IsValidCorp(string corp)
        {
            return new FilterCorpInput().IsValid(corp);
        }

        public ValidationResult IsValidApp(string app)
        {
            return new FilterAppInput().IsValid(app);
        }
    }
}