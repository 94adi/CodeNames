namespace CodeNames.Services.PlayerSubmitStrategy.Factory;

public interface IPlayerSubmitFactory
{
    IPlayerSubmitStrategy Create(PlayerCardSubmit playerCardSubmit);
}
