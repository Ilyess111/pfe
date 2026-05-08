using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    public interface IEmpAttendanceService
    {
        // En-têtes
        Task<IEnumerable<EmpAttendanceDto>> GetAllHeadersAsync(string projectNo);
        Task<EmpAttendanceDto?> GetHeaderByIdAsync(Guid id, string projectNo);
        Task<EmpAttendanceDto> CreateHeaderAsync(EmpAttendanceDto dto, string projectNo);
        Task<bool> PatchHeaderAsync(Guid id, EmpAttendanceDto dto, string projectNo);
        Task<bool> DeleteHeaderAsync(Guid id, string projectNo);

        // Lignes
        Task<bool> CreateLinesAsync(List<EmpAttendanceLineDto> lines, string projectNo);
        Task<bool> PatchLineAsync(Guid lineId, EmpAttendanceLineDto lineDto, string projectNo);
        Task<bool> DeleteLineAsync(Guid lineId, string projectNo);

        // Dans IEmpAttendanceService.cs
        Task<bool> MarkPresenceAsync(Guid headerId, string employeeNo, int day, string projectNo);
    
    }
}