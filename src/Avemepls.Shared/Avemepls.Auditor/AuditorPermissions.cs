using System.ComponentModel.DataAnnotations;

using Avemepls.Security;
using Avemepls.Security.Attributes;

namespace Avemepls.Auditor;

[PermissionsData("Логи")]
[RequireRoles(BuiltInRoles.Admin)]
public static class AuditorPermissions
{
    [Display(Name = "Аудит")]
    public static class Auditor
    {
        /// <summary>
        /// Просмотр
        /// </summary>
        [Display(Name = "Просмотр")]
        public const string View = "Logs.Auditor.View";
    }
}