namespace Kultura.Application.Dto.RestaurntDtos
{
    public record SlotDto
    {
        public string tableId { get; set; } = null!;

        public TimeSpan startTime {get;set;}
        public TimeSpan endTime { get; set; }
    }
}
