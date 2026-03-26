using FluentValidation;

namespace AprovaFlow.Api.DTOs.Comments;

public record CreateCommentRequest(string Content);
public record UpdateCommentRequest(string Content);

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("O comentário não pode estar vazio.")
            .MaximumLength(5000);
    }
}

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
