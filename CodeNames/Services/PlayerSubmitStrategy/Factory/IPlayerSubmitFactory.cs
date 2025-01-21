namespace CodeNames.Services.PlayerSubmitStrategy.Factory;

public interface IPlayerSubmitFactory
{
    IPlayerSubmitHandler Create(PlayerCardSubmit playerCardSubmit);
}
