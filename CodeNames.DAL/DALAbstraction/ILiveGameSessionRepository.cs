using CodeNames.Models;
using CodeNames.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.DAL.DALAbstraction
{
    public interface ILiveGameSessionRepository : IRepository<LiveGameSession>
    {
        LiveGameSession GetByGameRoom(int roomId);

        void AddEditLiveSession(LiveGameSession session);
    }
}
