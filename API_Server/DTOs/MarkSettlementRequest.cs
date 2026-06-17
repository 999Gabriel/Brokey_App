using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

// Request-Body für POST /api/groups/{id}/settlement/mark-settled – das User-Paar (von/an), dessen offene Splits als bezahlt markiert werden.
public class MarkSettlementRequest
{
    [Range(1, int.MaxValue)]
    public int FromUserId { get; set; }

    [Range(1, int.MaxValue)]
    public int ToUserId { get; set; }
}
