using FluentValidation.Resources;

namespace Schedia.Web.Shared.FluentValidationCustoms;

// #RsIgnore#
public class CustomLanguageManager : LanguageManager
{
    public CustomLanguageManager()
    {
        AddTranslation("ru", "NotNullValidator", "Поле '{PropertyName}' обязательно к заполнению");
        AddTranslation("ru", "NotEmptyValidator", "Поле '{PropertyName}' обязательно к заполнению");

        AddTranslation("en", "NotNullValidator", "Field '{PropertyName}' is required");
        AddTranslation("en", "NotEmptyValidator", "Field '{PropertyName}' is required");
    }
}