using CodeNames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Core.Services.StateMachineService
{
    public interface IStateMachineService
    {
        public SessionState NextState(SessionState currentState, StateTransition transition);
    }
}
