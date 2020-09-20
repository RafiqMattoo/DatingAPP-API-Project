using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private DataContext _datacontext;
        public AuthRepository(DataContext  dataContext)
        {
            this._datacontext = dataContext;
        }

        

        public async Task<User> Login(string username, string password)
        {
            var user =await _datacontext.Users.FirstOrDefaultAsync(x => x.Username== username);
            if(user==null)
            return user;
            
            if(!VerifyPasswordHash(password, user.Passwordhash,   user.Passwordsalt))
            {
            return null; 
            }
           
            return user;
        }

private bool  VerifyPasswordHash(string password,   byte[] passwordHash,   byte[] passwordSalt)
        {
            using (var hmac= new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                
                var computedHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i=0; i<computedHash.Length;++i)
                {
                    if(computedHash[i] != passwordHash[i]) 
                    return false;
                }
                return true;
            }
             
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac= new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt=hmac.Key;
                passwordHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
             
        }

        public async Task<User> Register(User user, string Password)
        {
            byte []PasswordHash, PasswordSalt;
             CreatePasswordHash(Password, out PasswordHash, out PasswordSalt);
             user.Passwordhash=PasswordHash;
             user.Passwordsalt=PasswordSalt;
             await _datacontext.Users.AddAsync(user);
             await _datacontext.SaveChangesAsync();
             return user;
              

        }

        public async Task<bool> UserExists(string Username)
        {
            if(await _datacontext.Users.AnyAsync(x=> x.Username==Username) )
            return true;

            return false;
        }
    }
}