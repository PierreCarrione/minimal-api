using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Mocks
{
    public class AdministratorServiceMock : IAdministratorService
    {
        private static List<Administrator> Administrators = new List<Administrator>(){
            new Administrator{
                Id = 1,
                Email = "adm@teste.com",
                Password = "123456",
                Profile = "Adm"
            },
            new Administrator{
                Id = 2,
                Email = "editor@teste.com",
                Password = "123456",
                Profile = "Editor"
            }
        };

        public Administrator? Login(LoginDTO loginDTO)
        {
            return Administrators.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault();
        }

        public Administrator Add(Administrator administrator)
        {
            Administrators.Add(administrator);
            return administrator;
        }

        public string Delete(Administrator administrator)
        {
            string message;

            try
            {
                Administrators.Remove(administrator);
                message = "Administrator deleted successfully.";
            }
            catch (Exception ex)
            {
                message = "Something wrong happened : " + ex.Message;
            }

            return message;
        }

        public List<Administrator> GetAll(int page = 1)
        {
            int pageSize = 10;
            var query = Administrators.AsQueryable();
            var result = query.ToList();

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public Administrator? GetById(int id)
        {
            return Administrators.Where(x => x.Id == id).FirstOrDefault();
        }

        public Administrator Update(Administrator administrator)
        {
            var admIndex = Administrators.IndexOf(administrator);
            Administrators[admIndex] = administrator;

            return administrator;
        }
    }
}
