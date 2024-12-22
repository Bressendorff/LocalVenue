using LocalVenue.Core.Enums;

namespace LocalVenue.RequestDTOs;

public class ShowRequestDTO
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public Genre Genre { get; set; }
}