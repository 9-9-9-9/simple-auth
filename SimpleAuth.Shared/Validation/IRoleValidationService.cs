using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Validation
{
    public interface IRoleValidationService
    {
        ValidationResult IsValid(CreateRoleModel model);
    }

    public class DefaultRoleValidationService : IRoleValidationService
    {
        public ValidationResult IsValid(CreateRoleModel model)
        {
            return PendingFilter<FilterCorpInput, string>.New(model.Corp)
                .Chain(
                    PendingFilter<FilterAppInput, string>.New(model.App),
                    PendingFilter<FilterEnvInput, string>.New(model.Env),
                    PendingFilter<FilterTenantInput, string>.New(model.Tenant),
                    PendingFilter<FilterModuleInput, string>.New(model.Module),
                    PendingFilter<FilterArrSubModulesInput, string[]>.New(model.SubModules),
                    PendingFilter<FilterRoleExistingPartsAreCorrect, IRolePart>.New(model)
                )
                .IsValid();
        }
    }
}