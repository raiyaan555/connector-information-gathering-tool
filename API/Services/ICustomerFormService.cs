using API.DTOs;
using API.Models;

namespace API.Services;

public interface ICustomerFormService
{
    ApiResponse<CustomerFormDto> GetFormByToken(string token);
    ApiResponse<CustomerFormResponseDto> SubmitForm(string token, SubmitCustomerFormRequest request);
    ApiResponse<IEnumerable<CustomerFormResponseDto>> GetResponsesByProjectId(Guid projectId);
}
