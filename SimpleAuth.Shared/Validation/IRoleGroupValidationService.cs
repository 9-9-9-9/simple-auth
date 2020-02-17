using System.Collections.Generic;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Validation
{
    public interface IRoleGroupValidationService
    {
        ValidationResult IsValid(CreatePermissionGroupModel model);
    }

    public class DefaultRoleGroupValidationService : IRoleGroupValidationService
    {
        public ValidationResult IsValid(CreatePermissionGroupModel model)
        {
            return PendingFilter<FilterCorpInput, string>.New(model.Corp)
                .Chain(
                    PendingFilter<FilterAppInput, string>.New(model.App),
                    PendingFilter<FilterStringIsNormalizedAndNotNull, string>.New(model.Name),
                    PendingFilter<FilterCollectionIsNormalizedIfAny, IEnumerable<string>>.New(model.CopyFromPermissionGroups)
                ).IsValid();
        }
    }
}