using System.ComponentModel.DataAnnotations;

namespace RestApi.Models
{
    public class ToDoTask
    {
        [Key]
        public int ToDoTaskId { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
        public importance taskimportance { get; set; } = importance.normal;
        [DataType(DataType.Date)]
        public string date { get; set; }
        public bool isactive { get; set; } = false;
        [Required]
        public string TaskListId { get; set; }
        public string CreateDate { get; set; }
        [Required]
        public bool IsMultipleTask { get; set; } = false;
        public bool IsDeleted { get; set; }
    }
}
