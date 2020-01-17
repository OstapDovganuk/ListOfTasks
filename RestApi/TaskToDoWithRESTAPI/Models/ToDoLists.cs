using System.Collections.Generic;
using RestApiToDoClient;

namespace TaskToDoWithRESTAPI.Models
{
    public static class ToDoLists
    {
        public static Dictionary<string, List<ToDoTask>> TaskLists = new Dictionary<string, List<ToDoTask>>();
    }
}
