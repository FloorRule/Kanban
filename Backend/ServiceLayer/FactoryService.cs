using IntroSE.Kanban.Backend.BusinessLayer;
using System.Text.Json;
using System;

namespace IntroSE.Kanban.Backend.ServiceLayer
{

    public class FactoryService
    {
        internal AuthenticationFacade authenticationFacade;
        internal UserFacade userFacade;
        public UserService userService;
        internal BoardFacade boardFacade;
        public BoardService boardService;
        public TaskService taskService;

        public FactoryService()
        {
            this.authenticationFacade = new AuthenticationFacade();
            this.userFacade = new UserFacade(authenticationFacade);
            this.userService = new UserService(userFacade);
            this.boardFacade = new BoardFacade(authenticationFacade);
            this.boardService = new BoardService(boardFacade);
            this.taskService = new TaskService(boardFacade);
        }

        public string LoadData()
        {
            Response userRes;
            Response boardRes;

            try
            {
                userRes = JsonSerializer.Deserialize<Response>(userService.LoadData());
                if (userRes.ErrorOccurred) 
                    return JsonSerializer.Serialize(userRes);

                boardRes = JsonSerializer.Deserialize<Response>(boardService.LoadData());
                if (boardRes.ErrorOccurred) 
                    return JsonSerializer.Serialize(boardRes);
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response(e.Message, true));
            }

            return JsonSerializer.Serialize(new Response());
        }

        public string DeleteData()
        {
            try
            {
                boardService.DeleteData();
                userService.DeleteData();
                return JsonSerializer.Serialize(new Response());
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response(e.Message, true));
            }
        }
    }
}