using CodeNames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Core.Services.LiveGameSessionService
{
    public interface ILiveGameSessionService
    {
        LiveGameSession GetByGameRoom(int roomId);

        void AddEditLiveSession(LiveGameSession session);

        void Remove(LiveGameSession session);

        void Remove(int id);

        void Remove(IEnumerable<LiveGameSession> sessions);
    }
}
