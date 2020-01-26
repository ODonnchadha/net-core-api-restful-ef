using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UpdateBook : ManipulationBook
    {
        [Required(ErrorMessage = "Description is required.")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
