using Microsoft.AspNetCore.Mvc;
using RestApiToDoClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TaskToDoWithRESTAPI.Models.ToDoLists;

namespace TaskToDoWithRESTAPI.Controllers
{
    public class TaskListsController : Controller
    {
        public async Task<IActionResult> Index()
        {
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            var tasks = await tasksClient.GetTasksAllAsync();
            //Створюємо початкові списки користувача
            foreach (var item in tasks)
            {
                TaskLists[item.TaskListId] = tasks.Where(a => a.TaskListId == item.TaskListId).ToList();
            }
            return View(TaskLists);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(string taskList)
        {
            if (taskList != null)
            {
                TaskLists.Add(taskList, null);
                return RedirectToAction(nameof(Index));
            }
            return View(taskList);
        }
        public IActionResult AddTask(string list)
        {
            ViewData["ListId"] = list;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Додаємо задачу
        public IActionResult AddTask(ToDoTask task, string id)
        {
            task.CreateDate = DateTime.Now.ToString();
            task.TaskListId = id;
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            tasksClient.PostToDoTaskAsync(task);

            var temp = TaskLists.FirstOrDefault(a => a.Key == id).Value;
            if (temp == null)
            {
                temp = new List<ToDoTask>();
                temp.Add(task);
            }
            else
            {
                temp.Add(task);
            }
            TaskLists[id] = temp;
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(string list)
        {
            if (list == null)
            {
                return NotFound();
            }
            var item = TaskLists.Where(a => a.Key == list);
            ViewBag.Key = list;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string name, string key)
        {
            var item = TaskLists.FirstOrDefault(a => a.Key == key);
            var val = item.Value;
            List<ToDoTask> newList = new List<ToDoTask>();
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            foreach (var t in val)
            {
                t.TaskListId = name;
                newList.Add(t);
                await tasksClient.PutToDoTaskAsync(t.ToDoTaskId, t);
            }                      
            TaskLists.Remove(item.Key);
            TaskLists.Add(name, newList);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> EditTask(int id)
        {
            ToDoTask task = new ToDoTask();
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            task = await tasksClient.GetTasks2Async(id);
            if (task != null)
            {
                ViewBag.ListName = task.TaskListId;
                return View(task);
            }
            return RedirectToAction("Tasks", "TaskLists", new { list = task.TaskListId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTaskAsync(int id, ToDoTask task, string listid)
        {
            task.TaskListId = listid;
            task.CreateDate = DateTime.Now.ToString();
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            await tasksClient.PutToDoTaskAsync(id, task);
            return RedirectToAction("Tasks", "TaskLists", new { list = listid });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTask(int id)
        {
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            var task = await tasksClient.GetTasks2Async(id);
            return View(task);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id, string listid)
        {
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            //Організовуємо видалення мульти задач
            var tasks = await tasksClient.GetTasksAsync("custom", listid);
            var item = tasks.FirstOrDefault(a => a.ToDoTaskId == id);
            foreach(var t in tasks)
            {
                if((t.IsMultipleTask == true && t.Title==item.Title) || item==t)
                {
                    await tasksClient.DeleteToDoTaskAsync(t.ToDoTaskId);
                }
            }
            
            return RedirectToAction("Tasks", "TaskLists", new { list = listid });
        }
        public IActionResult Delete(string key)
        {
            ViewBag.Key = key;
            return View();
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string key)
        {
            ToDoTasksClient client = new ToDoTasksClient();
            var item = TaskLists.FirstOrDefault(a => a.Key == key);
            if (item.Value != null)
            {
                foreach (var t in item.Value)
                {
                    await client.DeleteToDoTaskAsync(t.ToDoTaskId, key);
                }
            }
            TaskLists.Remove(key);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Search(string sort, string title_search)
        {
            ViewData["CurrentFilter"] = title_search;
            if (!String.IsNullOrEmpty(title_search))
            {
                ViewData["title"] = sort == "title" ? "title_desc" : "title";
                ViewData["description"] = sort == "description" ? "description_desc" : "description";
                ViewData["taskimportance"] = sort == "taskimportance" ? "taskimportance_desc" : "taskimportance";
                ViewData["date"] = sort == "date" ? "date_desc" : "date";
                ViewData["isactive"] = sort == "isactive" ? "isactive_desc" : "isactive";
                ViewData["CreateDate"] = sort == "CreateDate" ? "CreateDate_desc" : "CreateDate";
                ToDoTasksClient tasksClient = new ToDoTasksClient();
                var search_task = await tasksClient.GetTasksAsync("search", title_search);
                search_task = SortOrder(sort, search_task.ToList());
                return View(search_task.ToList());
            }
            return RedirectToAction(nameof(Index));
        }
        //Користувацькі списки
        //Метод формує список користувача
        public async Task<IActionResult> Tasks(string list, string sort)
        {
            ViewData["list"] = list;
            ViewData["title"] = sort == "title" ? "title_desc" : "title";
            ViewData["description"] = sort == "description" ? "description_desc" : "description";
            ViewData["taskimportance"] = sort == "taskimportance" ? "taskimportance_desc" : "taskimportance";
            ViewData["date"] = sort == "date" ? "date_desc" : "date";
            ViewData["isactive"] = sort == "isactive" ? "isactive_desc" : "isactive";
            ViewData["CreateDate"] = sort == "CreateDate" ? "CreateDate_desc" : "CreateDate";
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            var tasks = await tasksClient.GetTasksAsync("custom", list);
            var task = SortOrder(sort, tasks.ToList());
            return View(task);
        }
        //Метод який завершує задачу і виконує софт видалення 
        //Задача пропадає з користувацьких списків
        public async Task<IActionResult> Complet(int id)
        {
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            var tasks = await tasksClient.GetTasks2Async(id);
            tasks.IsDeleted = true;
            tasks.Isactive = true;
            await tasksClient.PutToDoTaskAsync(id, tasks);
            return RedirectToAction("Tasks", "TaskLists", new { list = tasks.TaskListId });
        }
        //Сортуємо наш список
        public List<ToDoTask> SortOrder(string sort, List<ToDoTask> toDoTasks)
        {
            switch (sort)
            {
                case "title":
                    toDoTasks = toDoTasks.OrderBy(s => s.Title).ToList();
                    break;
                case "title_desc":
                    toDoTasks = toDoTasks.OrderByDescending(s => s.Title).ToList();
                    break;
                case "description":
                    toDoTasks = toDoTasks.OrderBy(s => s.Description).ToList();
                    break;
                case "description_desc":
                    toDoTasks = toDoTasks.OrderByDescending(s => s.Description).ToList();
                    break;
                case "taskimportance":
                    toDoTasks = toDoTasks.OrderBy(s => s.Taskimportance).ToList();
                    break;
                case "taskimportance_desc":
                    toDoTasks = toDoTasks.OrderByDescending(s => s.Taskimportance).ToList();
                    break;
                case "date":
                    toDoTasks = toDoTasks.OrderBy(s => s.Date).ToList();
                    break;
                case "date_desc":
                    toDoTasks = toDoTasks.OrderByDescending(s => s.Date).ToList();
                    break;
                case "isactive":
                    toDoTasks = toDoTasks.OrderBy(s => s.Isactive).ToList();
                    break;
                case "isactive_desc":
                    toDoTasks = toDoTasks.OrderByDescending(s => s.Isactive).ToList();
                    break;
                case "CreateDate":
                    toDoTasks = toDoTasks.OrderBy(s => s.CreateDate).ToList();
                    break;
                case "CreateDate_desc":
                    toDoTasks = toDoTasks.OrderByDescending(s => s.CreateDate).ToList();
                    break;
            }
            return toDoTasks;
        }
        //Виводимо розумний список задач
        public async Task<IActionResult> SmartTasks(string smart, string hide = "show")
        {
            ViewData["HideComolet"] = hide == "hide" ? "show" : "hide";
            ViewData["SmartName"] = smart;
            ToDoTasksClient tasksClient = new ToDoTasksClient();
            var tasks = await tasksClient.GetTasksAsync(smart, hide);
            return View(tasks.ToList());
        }
    }
}