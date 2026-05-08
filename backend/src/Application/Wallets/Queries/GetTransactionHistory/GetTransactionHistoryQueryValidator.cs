using System;
using FluentValidation;

namespace Application.Wallets.Queries.GetTransactionHistory;

public class GetTransactionHistoryQueryValidator : AbstractValidator<GetTransactionHistoryQuery>
{
    public GetTransactionHistoryQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.TransactionType).IsInEnum().When(x => x.TransactionType.HasValue);
    }
}
