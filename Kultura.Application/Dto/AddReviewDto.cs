using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto
{
    public class AddReviewDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RestaurantId { get; set; } = string.Empty;

        [Required]
        [StringLength(500, ErrorMessage = "Comment length can't exceed 500 characters.")]
        public string Comment { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
    }
}
