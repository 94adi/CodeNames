using CodeNames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Core.Services.StateMachineService
{
    public class StateMachineService : IStateMachineService
    {

        public SessionState NextState(SessionState currentState, StateTransition transition)
        {
            SessionState nextState = SessionState.UNKNOWN;

            switch (transition)
            {
                case StateTransition.NONE:
                    {
                        if (currentState == SessionState.PENDING)
                        {
                            nextState = SessionState.INIT;
                        }
                        else if (currentState == SessionState.INIT)
                        {
                            nextState = SessionState.START;
                        }
                        else if (currentState == SessionState.SPYMASTER_BLUE)
                        {
                            nextState = SessionState.GUESS_BLUE;
                        }
                        else if (currentState == SessionState.GUESS_BLUE)
                        {
                            nextState = SessionState.SPYMASTER_RED;
                        }
                        else if (currentState == SessionState.SPYMASTER_RED)
                        {
                            nextState = SessionState.GUESS_RED;
                        }
                        else if (currentState == SessionState.GUESS_RED)
                        {
                            nextState = SessionState.SPYMASTER_BLUE;
                        }
                        break;
                    }
                case StateTransition.GAME_START:
                    {
                        if (currentState == SessionState.START)
                        {
                            nextState = SessionState.SPYMASTER_BLUE;
                        }
                        break;
                    }
                case StateTransition.TEAM_GUESSED_ALL_CARDS:
                    {
                        if (currentState == SessionState.GUESS_RED)
                        {
                            nextState = SessionState.RED_WON;
                        }
                        else if (currentState == SessionState.GUESS_BLUE)
                        {
                            nextState = SessionState.BLUE_WON;
                        }
                        break;
                    }
                case StateTransition.TEAM_CHOSE_BLACK_CARD:
                case StateTransition.TEAM_GUESSED_ALL_OPPONENT_CARDS:
                    {
                        if(currentState == SessionState.GUESS_RED)
                        {
                            nextState = SessionState.BLUE_WON;
                        }
                        else if(currentState == SessionState.GUESS_BLUE)
                        {
                            nextState = SessionState.RED_WON;
                        }
                        break;
                    }
            }

            return nextState;
        }
    }
}
