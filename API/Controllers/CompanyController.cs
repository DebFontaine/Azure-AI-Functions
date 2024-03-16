using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace API.Controllers;


[Authorize]
public class CompanyController : BaseApiController
{
    //private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    //private readonly IPhotoService _photoService;
    private readonly ILogger<UsersController> _logger;

    private readonly ICompanyRepository _companyRepository;
    private readonly IGoogleDriveRepository _driveRepository;
    private readonly IPhotoService _photoService;

    public CompanyController(IMapper mapper, ILogger<UsersController> logger, ICompanyRepository companyRepository,
        IGoogleDriveRepository googleDriveRepository, IPhotoService photoService)
    {
        _companyRepository = companyRepository;
        _driveRepository = googleDriveRepository;
        _mapper = mapper;
        _logger = logger;
        _photoService = photoService;
    }
    [HttpGet]
    public async Task<ActionResult<CompanyDto>> GetCompanies()
    {


        var companies = await _companyRepository.GetCompaniesAsync();

        return Ok(companies);
    }
    [HttpGet("{companyId}")]
    public async Task<ActionResult<CompanyDto>> GetCompanyById(int companyId)
    {
        var companies = await _companyRepository.GetCompanyByIdAsync(companyId);

        return Ok(companies);
    }
    [HttpPost]
    public async Task<ActionResult<Company>> CreateCompany([FromBody] AddCompanyDto companyDto)
    {
        var username = User.GetUsername();

        var company = await _companyRepository.GetCompanyByNameAsync(companyDto.Name);
        if (company != null)
            return BadRequest("A company with that name already exists");
        var driveResult = await _driveRepository.CreateRootSubfolder("1nkuQGNW0fBmn429np2DaUjMm_ZdKR06U", companyDto.Name);
        if (!driveResult.IsSuccess)
            return BadRequest("Failed to create file storage folder");
        var driveFolderId = driveResult.Data;
        if (string.IsNullOrEmpty(driveFolderId))
            return BadRequest("Failed to create file storage folder");

        companyDto.Folder = driveFolderId;

        var createdCompany = await _companyRepository.CreateCompany(companyDto);
        return Ok(createdCompany);
    }
    [HttpPost("add-photo/{companyId}")]
    public async Task<ActionResult<CompanyPhotoUpdateDto>> AddPhoto(int companyId, [FromForm] IFormFile file)
    {
        if(file == null)
            return BadRequest("The file contains no content");

        var company = await _companyRepository.GetCompanyByIdAsync(companyId);
        if (company == null)
            return BadRequest($"Company with id {companyId} not found");

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        company.PhotoUrl = result.SecureUrl.AbsoluteUri;

        if (await _companyRepository.UpdateCompany() > 0) 
        {
            CompanyPhotoUpdateDto dto = new CompanyPhotoUpdateDto {
                PhotoUrl = result.SecureUrl.AbsoluteUri
            };
            
            return Ok(dto);
        }     

        return BadRequest("Error adding photo");
    }
    [HttpPut("{companyId}")]
    public async Task<ActionResult> UpdateCompany(int companyId, [FromBody] CompanyUpdateDto companyUpdateDto)
    {
        var company = await _companyRepository.GetCompanyByIdAsync(companyId);
        if (company == null)
            return BadRequest($"Company with id {companyId} not found");

        if (await _companyRepository.UpdateCompany(company, companyUpdateDto) > 0)
            return NoContent();

        return BadRequest("Error updating company");
    }
    [HttpDelete("{companyId}")]
    public async Task<ActionResult> DeleteCompany(int companyId)
    {
        var company = await _companyRepository.GetCompanyByIdAsync(companyId);
        if (company == null)
            return BadRequest($"Company with id {companyId} not found");

        if (await _companyRepository.DeleteCompany(company) > 0)
            return Ok();

        return BadRequest("Error deleting company");
    }
}
