namespace CodeNames.Services.PlayerSubmitStrategy.Factory;

public class PlayerSubmitFactory : IPlayerSubmitFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PlayerSubmitFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IPlayerSubmitHandler Create(PlayerCardSubmit playerCardSubmit)
    {
        return playerCardSubmit switch
        {
            PlayerCardSubmit.Black =>
                _serviceProvider.GetRequiredService<PlayerSubmitBlackCardHandler>(),
            PlayerCardSubmit.Neutral =>
                _serviceProvider.GetRequiredService<PlayerSubmitNeutralCardHandler>(),
            PlayerCardSubmit.Team =>
                _serviceProvider.GetRequiredService<PlayerSubmitTeamCardHandler>(),
            PlayerCardSubmit.OppositeTeam =>
                _serviceProvider.GetRequiredService<PlayerSubmitOppositeTeamCardHandler>(),
            _ => throw new Exception("Invalid input")
        };

    }
}
