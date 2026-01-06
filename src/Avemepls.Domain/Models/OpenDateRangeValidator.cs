using Avemepls.Core.Models;

using FluentValidation;

namespace Avemepls.Domain.Models;

public class OpenDateRangeValidator : AbstractValidator<OpenDateRange>
{
    public OpenDateRangeValidator()
    {
        RuleFor(q => q.To)
            .GreaterThanOrEqualTo(q => q.From)
            .When(e => e is { From: not null, To: not null });
    }
}