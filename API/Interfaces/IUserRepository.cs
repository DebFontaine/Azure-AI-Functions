﻿using API.Dtos;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<bool> SaveAllAsync();

    Task<IEnumerable<AppUser>> GetUsersAsync();

    Task<AppUser> GetuserByIdAsync(int id);

    Task<AppUser> GetUserByUsernameAsync(string username);
    Task<AppUser> GetUserWithRolesByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto> GetMemberAsync(string username);
    Task<string> GetUserGender(string username);
}