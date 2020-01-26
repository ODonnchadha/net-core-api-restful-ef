using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public abstract class ManipulationBook
    {
        [Required(ErrorMessage = "Title is required."), MaxLength(100, ErrorMessage = "Title cannot have more than 100 characters.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot have more than 500 characters.")]
        public virtual string Description { get; set; }
    }
}
