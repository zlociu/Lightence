using LightenceServer.Data;
using LightenceServer.Interfaces;
using LightenceServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Services
{
    public class ProductKeyManager: IProductKeyManager
    {
        private readonly AppDbContext _appDbContext;

        private readonly int[] parameters = new int[] { 7, 5, 3, 2 };

        public ProductKeyManager(AppDbContext context)
        {
            _appDbContext = context;
        }

        private int CharToInt(char sign)
        {
            return ((int)sign - 65);
        }

        private int CheckMaxUses(string key)
        {
            return ((CharToInt(key[16]) + CharToInt(key[18])) % 26) * 26 + (CharToInt(key[17]) + CharToInt(key[19])) % 26;
        }


        /// <summary>
        /// Verify if key is valid in internal structure. Key lenght should be 25. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if producy key is valid, otherwise false</returns>
        public bool VerifyKey(string key)
        {
            // [0-4] == [20]
            int count = (CharToInt(key[0]) + CharToInt(key[1]) + parameters[0] * CharToInt(key[2]) + CharToInt(key[3]) + CharToInt(key[4])) % 26;
            if (CharToInt(key[20]) != count) return false;

            // [5-9] == [21]
            count = (CharToInt(key[5]) + parameters[1] * CharToInt(key[6]) + CharToInt(key[7]) + CharToInt(key[8]) + CharToInt(key[9])) % 26;
            if (CharToInt(key[21]) != count) return false;

            // [10-14] == [22]
            count = (CharToInt(key[10]) + CharToInt(key[11]) + CharToInt(key[12]) + CharToInt(key[13]) + parameters[2] * CharToInt(key[14])) % 26;
            if (CharToInt(key[22]) != count) return false;

            // [15-19] == [23]
            count = (parameters[3] * CharToInt(key[15]) + CharToInt(key[16]) + CharToInt(key[17]) + CharToInt(key[18]) + CharToInt(key[19])) % 26;
            if (CharToInt(key[23]) != count) return false;

            // [20-23] == [24]
            count = (CharToInt(key[20]) + CharToInt(key[21]) + CharToInt(key[22]) + CharToInt(key[23])) % 26;
            if (CharToInt(key[24]) != count) return false;

            return true;
        }

        /// <summary>
        /// Add key to database, or if already exists, check for avaiable uses.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>0 if ok, 1 if key already have max uses</returns>
        public async Task<int> AddKeyAsync(string key)
        {
            var result = await _appDbContext.ProductKeys.FindAsync(key);
            if (result == null)
            {
                _appDbContext.ProductKeys.Add(new ProductKeyModel
                {
                    Key = key,
                    Uses = 1,
                    CreateDate = DateTime.Now
                });
                _appDbContext.SaveChanges();
            }
            else
            {
                if (CheckMaxUses(key) > result.Uses)
                {
                    result.Uses += 1;
                    _appDbContext.SaveChanges();
                }
                else return 1;                
            }
            return 0;
        }
    }
}
