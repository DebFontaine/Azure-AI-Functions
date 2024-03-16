using API.Entities;

namespace API.Interfaces;

public interface ICompanyRepository
{
    Task<IEnumerable<Company>> GetCompaniesAsync();

    Task<Company> GetCompanyByNameAsync(string companyName);
    Task<Company> GetCompanyByIdAsync(int companyId);
    Task<Company> CreateCompany(AddCompanyDto companyDto);
    Task<int> UpdateCompany();
    Task<int> UpdateCompany(Company company, CompanyUpdateDto companyUpdateDto);
    Task<int> DeleteCompany(Company company);
}
