using System.Linq.Expressions;
using API.Data;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class CompanyRepository : ICompanyRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyRepository> _logger;

    public CompanyRepository(DataContext context, IMapper mapper, ILogger<CompanyRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<IEnumerable<Company>> GetCompaniesAsync()
    {
        return await _context.Companies.ToListAsync();
    }

    public async Task<Company> GetCompanyByNameAsync(string companyName)
    {
        return await _context.Companies
            .SingleOrDefaultAsync(x => x.Name.ToLower() == companyName.ToLower());
    }
    public async Task<Company> GetCompanyByIdAsync(int companyId)
    {
        return await _context.Companies
            .SingleOrDefaultAsync(x => x.CompanyId == companyId);
    }
    public async Task<Company> CreateCompany(AddCompanyDto companyDto)
    {
        try
        {
            var companyToAdd = new Company
            {
                Name = companyDto.Name,
                Folder = companyDto.Folder,
                PhotoUrl = companyDto.PhotoUrl,
                City = companyDto.City,
                Country = companyDto.Country,
            };

            var company = _context.Companies.Add(companyToAdd);

            await _context.SaveChangesAsync();

            return company.Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error creating company");
        }
        return null;

    }
    public async Task<int> UpdateCompany(Company company, CompanyUpdateDto companyUpdateDto)
    {
        try
        {
            company.City = companyUpdateDto.City;
            company.Country = companyUpdateDto.Country;

            var result = await _context.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Company");
            return 0;
        }

    }
    public async Task<int> UpdateCompany()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Company");
            return 0;
        }

    }
    public async Task<int> DeleteCompany(Company company)
    {
        try
        {
            _context.Companies.Remove(company);
            var result = await _context.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Deleting Company");
            return 0;
        }
    }

}
