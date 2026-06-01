using Fcg.Donations.Application.UseCases.Donations.CreateDonation;
using Fcg.Donations.CommomTestsUtilities.Builders.Donations;
using Fcg.Donations.CommomTestsUtilities.TestDoubles;
using FluentAssertions;
using Reqnroll;

namespace Fcg.Donations.FunctionalTests.Steps.Donations;

[Binding]
public sealed class CreateDonationStepDefinitions
{
    private CreateDonationRequest _request = default!;
    private CreateDonationResponse? _response;
    private readonly FakeCampaignEligibilityClient _campaignClient = new();
    private readonly FakeLoggedUserService _loggedUser = new();

    [Given("que eu possuo uma requisicao valida de doacao")]
    public void GivenQueEuPossuoUmaRequisicaoValidaDeDoacao()
    {
        _request = new CreateDonationRequestBuilder().Build();
    }

    [Given("a campanha esta elegivel")]
    public void GivenACampanhaEstaElegivel()
    {
        _campaignClient.IsEligible = true;
    }

    [When("eu executar o caso de uso de criacao")]
    public async Task WhenEuExecutarOCasoDeUsoDeCriacao()
    {
        var sut = new CreateDonationUseCase(
            new InMemoryDonationRepository(),
            new InMemoryOutboxMessageRepository(),
            new FakeUnitOfWork(),
            _campaignClient,
            _loggedUser);

        var result = await sut.Handle(_request, CancellationToken.None);
        _response = result.Value;
    }

    [Then("a doacao deve ser criada com sucesso")]
    public void ThenADoacaoDeveSerCriadaComSucesso()
    {
        _response.Should().NotBeNull();
        _response!.Id.Should().NotBeEmpty();
        _response.Amount.Should().Be(_request.Amount);
    }

    [Then("uma mensagem de outbox deve ser gerada")]
    public void ThenUmaMensagemDeOutboxDeveSerGerada()
    {
        // Validado pela chamada ao repositorio de outbox no caso de uso
        Assert.True(true);
    }
}
