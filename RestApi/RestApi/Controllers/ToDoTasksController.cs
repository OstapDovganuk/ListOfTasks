using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoTasksController : ControllerBase
    {
        private readonly TaskContext _context;

        public ToDoTasksController(TaskContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoTask>>> GetTasks()
        {
            return await _context.Tasks.ToListAsync();
        }
        [HttpGet("{smart}&{hide}")]
        public async Task<ActionResult<IEnumerable<ToDoTask>>> GetTasks(string smart, string hide)
        {
            List<ToDoTask> all_task;
            List<ToDoTask> planned_task;
            List<ToDoTask> important_task;
            List<ToDoTask> today_task;
            if (smart == "custom")
            {
                all_task = await _context.Tasks.Where(a => a.TaskListId == hide).ToListAsync();
                return all_task;
            }
            if (smart == "search")
            {
                all_task = await _context.Tasks.Where(a => a.title.Contains(hide)).ToListAsync();
                return all_task;
            }
            if (hide != "hide")
            {
                all_task = await _context.Tasks.IgnoreQueryFilters().ToListAsync();
                planned_task = await _context.Tasks.IgnoreQueryFilters().Where(a => a.date != null).ToListAsync();
                important_task = await _context.Tasks.IgnoreQueryFilters().Where(a => a.taskimportance == importance.hight).ToListAsync();
                today_task = await _context.Tasks.IgnoreQueryFilters().Where(a => a.date == DateTime.Now.Date.ToString()).ToListAsync();
            }
            else
            {
                all_task = await _context.Tasks.ToListAsync();
                planned_task = await _context.Tasks.Where(a => a.date != null).ToListAsync();
                important_task = await _context.Tasks.Where(a => a.taskimportance == importance.hight).ToListAsync();
                today_task = await _context.Tasks.Where(a => a.date == DateTime.Now.Date.ToString()).ToListAsync();
            }
            switch (smart)
            {
                case "all_task":
                    return all_task;
                case "planned_task":
                    return planned_task;
                case "important_task":
                    return important_task;
                case "today_task":
                    return today_task;
            }
            return await _context.Tasks.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoTask>> GetTasks(int id)
        {
            var toDoTask = await _context.Tasks.FindAsync(id);

            if (toDoTask == null)
            {
                return NotFound();
            }

            return toDoTask;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToDoTask(int id, ToDoTask toDoTask)
        {
            if (id != toDoTask.ToDoTaskId)
            {
                return BadRequest();
            }

            _context.Entry(toDoTask).State = EntityState.Modified;

            try
            {
                _context.Update(toDoTask);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoTaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpPost]
        public async Task<ActionResult<ToDoTask>> PostToDoTask(ToDoTask toDoTask)
        {
            var item = _context.Tasks.Where(a => a.title == toDoTask.title && a.description == toDoTask.description && a.TaskListId == toDoTask.TaskListId && toDoTask.IsMultipleTask == false).FirstOrDefault();
            if (item == null)
            {
                _context.Tasks.Add(toDoTask);
                await _context.SaveChangesAsync();
            }
            return CreatedAtAction("GetToDoTask", new { id = toDoTask.ToDoTaskId }, toDoTask);
        }
        [HttpDelete("{id}&{list}")]
        public async Task<ActionResult<ToDoTask>> DeleteToDoTask(int id, string list)
        {
            if (list != null)
            {
                var toDoTask = await _context.Tasks.Where(a => a.TaskListId == list).ToListAsync();
                if (toDoTask == null)
                {
                    return NotFound();
                }
                foreach (var item in toDoTask)
                {
                    _context.Tasks.Remove(item);
                }
                await _context.SaveChangesAsync();
                return toDoTask.LastOrDefault();
            }
            else
            {
                var toDoTask = await _context.Tasks.FindAsync(id);
                if (toDoTask == null)
                {
                    return NotFound();
                }
                var multi_task = await _context.Tasks.Where(a => a.IsMultipleTask == true && a.title == toDoTask.title && a.TaskListId == toDoTask.TaskListId).ToListAsync();
                if (multi_task != null)
                {
                    foreach (var item in multi_task)
                    {
                        _context.Tasks.Remove(item);
                    }
                }
                _context.Tasks.Remove(toDoTask);
                await _context.SaveChangesAsync();
                return toDoTask;
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ToDoTask>> DeleteToDoTask(int id)
        {
            var toDoTask = await _context.Tasks.FindAsync(id);
            _context.Remove(toDoTask);
            _context.SaveChanges();
            return toDoTask;
        }

        private bool ToDoTaskExists(int id)
        {
            return _context.Tasks.Any(e => e.ToDoTaskId == id);
        }
    }
}
