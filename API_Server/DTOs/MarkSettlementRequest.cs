using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

public class MarkSettlementRequest
{
    [Range(1, int.MaxValue)]
    public int FromUserId { get; set; }

    [Range(1, int.MaxValue)]
    public int ToUserId { get; set; }
}
