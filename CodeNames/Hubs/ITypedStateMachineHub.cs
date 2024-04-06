using CodeNames.Models;

namespace CodeNames.Hubs
{
    public interface ITypedStateMachineHub
    {
        Task ReceiveModel(LiveSession liveSessionModel);
    }
}
