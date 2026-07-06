using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fcs.Donations.WebApi.Swagger;

[ExcludeFromCodeCoverage]
public sealed class SwaggerEndpointDocumentationOperationFilter : IOperationFilter
{
    private static readonly IReadOnlyDictionary<string, EndpointDocumentation> EndpointDocumentationByOperationId =
        new Dictionary<string, EndpointDocumentation>
        {
            [nameof(Controllers.v1.DonationsController.Create)] = new(
                "Criar intencao de doacao",
                "Registra uma intencao de doacao para uma campanha ativa. Requer Bearer token valido com role Doador. A API valida a elegibilidade da campanha na fcs-campaigns, persiste a doacao como Pending e enfileira o evento donation-received via outbox.",
                SwaggerExamples.CreateDonationRequest,
                new Dictionary<string, ResponseDocumentation>
                {
                    ["202"] = new("Intencao de doacao aceita para processamento assincrono.", SwaggerExamples.CreateDonationAccepted),
                    ["400"] = new("Payload invalido, valor menor ou igual a zero, campanha inelegivel ou requisicao rejeitada.", SwaggerExamples.ValidationError),
                    ["401"] = new("Bearer token ausente, invalido ou expirado."),
                    ["403"] = new("Usuario autenticado sem role Doador."),
                    ["404"] = new("Campanha nao encontrada.", SwaggerExamples.CampaignNotFoundError),
                    ["409"] = new("Conflito ao aceitar a intencao de doacao.", SwaggerExamples.CampaignNotEligibleError),
                    ["503"] = new("Servico de campanhas temporariamente indisponivel.", SwaggerExamples.CampaignServiceUnavailableError)
                }),
            [nameof(Controllers.v1.DonationsController.Get)] = new(
                "Listar doacoes",
                "Retorna doacoes paginadas. Doador visualiza apenas as proprias doacoes; GestorONG visualiza doacoes de todos os doadores. Permite filtrar por status e ordenar por campos suportados pela API.",
                null,
                new Dictionary<string, ResponseDocumentation>
                {
                    ["200"] = new("Doacoes encontradas.", SwaggerExamples.DonationsPageSuccess),
                    ["401"] = new("Bearer token ausente, invalido ou expirado."),
                    ["403"] = new("Usuario autenticado sem role Doador ou GestorONG.")
                }),
            [nameof(Controllers.v1.DonationsController.GetById)] = new(
                "Consultar doacao",
                "Busca uma doacao pelo identificador. Doador so pode consultar doacao propria; caso a doacao pertenca a outro Doador, a API retorna 404. GestorONG pode consultar qualquer doacao.",
                null,
                new Dictionary<string, ResponseDocumentation>
                {
                    ["200"] = new("Doacao encontrada.", SwaggerExamples.DonationSuccess),
                    ["401"] = new("Bearer token ausente, invalido ou expirado."),
                    ["403"] = new("Usuario autenticado sem role Doador ou GestorONG."),
                    ["404"] = new("Doacao nao encontrada.", SwaggerExamples.DonationNotFoundError)
                })
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!EndpointDocumentationByOperationId.TryGetValue(context.MethodInfo.Name, out var documentation))
        {
            return;
        }

        operation.Summary = documentation.Summary;
        operation.Description = documentation.Description;
        operation.OperationId = context.MethodInfo.Name;

        ApplyRequestExample(operation, documentation.RequestExample);
        ApplyResponseDocumentation(operation, documentation.Responses);
    }

    private static void ApplyRequestExample(OpenApiOperation operation, object? example)
    {
        if (example is null || operation.RequestBody?.Content is null)
        {
            return;
        }

        if (!operation.RequestBody.Content.TryGetValue(SwaggerConstants.JsonContentType, out var mediaType))
        {
            return;
        }

        mediaType.Example = OpenApiExampleFactory.Create(example);
    }

    private static void ApplyResponseDocumentation(
        OpenApiOperation operation,
        IReadOnlyDictionary<string, ResponseDocumentation> responses)
    {
        if (operation.Responses is null)
        {
            return;
        }

        foreach (var (statusCode, documentation) in responses)
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                continue;
            }

            response.Description = documentation.Description;

            if (documentation.Example is not null &&
                response.Content is not null &&
                response.Content.TryGetValue(SwaggerConstants.JsonContentType, out var mediaType))
            {
                mediaType.Example = OpenApiExampleFactory.Create(documentation.Example);
            }
        }
    }
}
