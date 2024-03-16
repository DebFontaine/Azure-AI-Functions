using API.Dtos;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper  mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<AppUser> GetuserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.Photos)
            .Include(c => c.Company)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }
    public async Task<AppUser> GetUserWithRolesByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.UserRoles)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(p => p.Photos)
            .Include(c => c.Company)
            .ToListAsync();
    }
    public async Task<string> GetUserGender(string username)
    {
        return await _context.Users.
            Where(x => x.UserName == username).
            Select(u => u.Gender).FirstOrDefaultAsync();
    }


    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
       return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query =  _context.Users.AsQueryable();

        query = query.Where(x => x.UserName != userParams.CurrentUsername);
        if(userParams.Gender != "All")
            query = query.Where(x => x.Gender == userParams.Gender);
        if(userParams.CompanyId != -1)
            query = query.Where(x => x.CompanyId == userParams.CompanyId);

        var minDOB = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge -1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDOB && x.DateOfBirth <= maxDob);

        query = query.OrderBy(user => user.Company.Name);
        
        return await PagedList<MemberDto>.CreateAsync(
            query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider), 
            userParams.PageNumber, userParams.PageSize);
      
    }
    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

}

public class PageList<T>
{
}