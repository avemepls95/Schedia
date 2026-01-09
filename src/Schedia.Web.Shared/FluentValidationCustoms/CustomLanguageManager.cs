using FluentValidation.Resources;

namespace Schedia.Web.Shared.FluentValidationCustoms;

// #PoIgnore#
public class CustomLanguageManager : LanguageManager
{
    public CustomLanguageManager()
    {
        AddTranslation("ru", "NotNullValidator", "Поле '{PropertyName}' обязательно к заполнению");
        AddTranslation("ru", "NotEmptyValidator", "Поле '{PropertyName}' обязательно к заполнению");
    }
}