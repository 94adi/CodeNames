namespace CodeNames.Services.PlayerSubmitStrategy;

public interface IPlayerSubmitHandler
{
    Task PlayerSubmit(LiveSession session, SessionData sessionData);
}
