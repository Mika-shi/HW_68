using home_work_66.BLL.Models;

namespace home_work_66.BLL.Interfaces;

public interface IHeadHunterRepository
{
    Task<User?> GetUserByIdAsync(int id);

    Task<List<Category>> GetCategoriesAsync();
    Task<List<Category>> GetCategoriesOrderedAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<bool> CategoryExistsAsync(string name);
    Task<bool> CategoryHasResumesAsync(int id);
    Task<bool> CategoryHasVacanciesAsync(int id);
    Task AddCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
    Task SaveChangesAsync();
    Task<Vacancy?> GetVacancyByIdAsync(int id);
    Task<List<Vacancy>> GetVacanciesAsync();
    Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId);
    Task AddVacancyAsync(Vacancy vacancy);
    Task<Vacancy?> GetVacancyForEditAsync(int id, int companyId);
    Task<List<Vacancy>> GetPublishedVacanciesAsync();
    Task<Vacancy?> GetVacancyByIdAndCompanyAsync(int id, int companyId);
    Task<List<Chat>> GetCompanyChatsAsync(int companyId);
    
    Task<List<Resume>> GetResumesByUserAsync(int userId);
    
    Task<Resume?> GetResumeByIdAndUserAsync(int id, int userId);
    
    Task AddResumeAsync(Resume resume);
    
    Task<Resume?> GetResumeDetailsAsync(int id, int userId);
    
    Task<List<Resume>> GetPublishedResumesAsync();
    
    Task<Resume?> GetPublicResumeDetailsAsync(int id);
    
    Task<List<Resume>> GetPublishedResumesByCategoryAsync(int? categoryId);
    
    Task AddWorkExperienceAsync(WorkExperience workExperience);
    
    Task<WorkExperience?> GetWorkExperienceAsync(int id, int userId);
    
    Task UpdateWorkExperienceAsync(WorkExperience workExperience);
    
    Task DeleteWorkExperienceAsync(WorkExperience workExperience);
    
    Task AddEducationAsync(Education education);
    
    Task<Education?> GetEducationAsync(int id, int userId);
    
    Task UpdateEducationAsync(Education education);
    
    Task DeleteEducationAsync(Education education);
    
    Task AddCourseAsync(Course course);
    
    Task<Course?> GetCourseAsync(int id, int userId);
    
    Task UpdateCourseAsync(Course course);
    
    Task DeleteCourseAsync(Course course);
    
    Task<Chat?> GetExistingChatAsync(int userId, int vacancyId);
    
    Task AddChatAsync(Chat chat);
    
    Task<Chat?> GetChatByIdAsync(int chatId);
    
    Task<List<Message>> GetMessagesByChatIdAsync(int? chatId);
    
    Task AddMessageAsync(Message message);
    
    Task<Resume?> GetPublishedResumeByIdAsync(int resumeId);
    
    Task<Chat?> GetExistingChatForCompanyAsync(int userId, int companyId, int vacancyId);
    
    Task<List<Resume>> GetUserResumesAsync(int userId);
    
    Task<List<Chat>> GetUserResponsesAsync(int userId);
    
    Task<bool> IsEmailAvailableAsync(string email, int? id);
}