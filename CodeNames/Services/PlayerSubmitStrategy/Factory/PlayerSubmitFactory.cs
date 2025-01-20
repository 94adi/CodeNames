namespace CodeNames.Services.PlayerSubmitStrategy.Factory;

public class PlayerSubmitFactory : IPlayerSubmitFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PlayerSubmitFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IPlayerSubmitStrategy Create(PlayerCardSubmit playerCardSubmit)
    {
        using var scope = _serviceProvider.CreateScope();

        var scopedServiceProvider = scope.ServiceProvider;

        return playerCardSubmit switch
        {
            PlayerCardSubmit.Black => scopedServiceProvider.GetRequiredService<PlayerSubmitBlackCard>(),
            _ => throw new Exception("Invalid input")
        };

    }
}
