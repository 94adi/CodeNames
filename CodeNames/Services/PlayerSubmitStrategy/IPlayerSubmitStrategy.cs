namespace CodeNames.Services.PlayerSubmitStrategy;

public interface IPlayerSubmitStrategy
{
    Task PlayerSubmit(LiveSession session, SessionData sessionData);
}
