using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.ViewModels;

namespace home_work_66.Services;

public interface IHeadHunterService
{
    Task<Vacancy?> GetVacancyByIdAsync(int id);
    Task<VacancyIndexViewModel> GetVacanciesAsync(string? searchString, int? categoryId, VacancySortState sortOrder, int page);
    Task<List<Category>> GetCategoriesAsync();
    
    Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId, bool isOwner);
    
    Task<bool> CreateVacancyAsync(CreateVacancyViewModel model, int companyId);
    
    Task<CreateVacancyViewModel?> GetVacancyForEditAsync(int id, int companyId);
    
    Task<bool> EditVacancyAsync(int id, int companyId, CreateVacancyViewModel model);
    
    Task<bool> UpdateVacancyAsync(int id, int companyId);
    
    Task<bool> TogglePublishAsync(int id, int companyId);
    
    Task<List<CompanyResponsesViewModel>> GetCompanyResponsesAsync(int companyId);
    
    Task<User?> GetUserByIdAsync(int id);
    
    Task<EditCompanyProfileViewModel?> GetCompanyProfileForEditAsync(int userId);
    
    Task<EditCompanyProfileResult> UpdateCompanyProfileAsync(int userId, EditCompanyProfileViewModel model);
    
    Task<List<Resume>> GetResumesAsync(int userId, bool isOwner);
    
    Task<int> CreateResumeAsync(Resume resume, int userId);
    
    Task<Resume?> GetResumeForEditAsync(int id, int userId);
    
    Task<bool> EditResumeAsync(Resume model, int userId);
    
    Task<bool> UpdateResumeDateAsync(int id, int userId);

    Task AddWorkExperienceAsync(WorkExperience workExperience);
    
    Task<WorkExperience?> GetWorkExperienceAsync(int id, int userId);
    
    Task UpdateWorkExperienceAsync(WorkExperience workExperience);
    
    Task<int?> DeleteWorkExperienceAsync(int id, int userId);
    
    Task AddEducationAsync(Education education);
    
    Task<Education?> GetEducationAsync(int id, int userId);
    
    Task UpdateEducationAsync(Education education);
    
    Task<int?> DeleteEducationAsync(int id, int userId);

    Task AddCourseAsync(Course course);
    
    Task<Course?> GetCourseAsync(int id, int userId);
    
    Task UpdateCourseAsync(Course course);
    
    Task<int?> DeleteCourseAsync(int id, int userId);

    Task<Resume?> GetResumeDetailsAsync(int id, int userId);
    
    Task<bool> ToggleResumePublicationAsync(int id, int userId);
    
    Task<List<Resume>> GetPublishedResumesAsync();
    
    Task<Resume?> GetPublicResumeDetailsAsync(int id);
    
    Task<List<Category>> GetCategoriesOrderedAsync();
    
    Task<List<Resume>> GetPublishedResumesByCategoryAsync(int? categoryId);
}