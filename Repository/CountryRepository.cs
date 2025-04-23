using Microsoft.EntityFrameworkCore;
using YemekTarifleri.Db;
using YemekTarifleri.Models;

namespace YemekTarifleri.Repository
{
    public class CountryRepository
    {

        public async Task<bool> SaveCountryAsync(CountryModel countryModel, int userId)
        {
            if (countryModel == null)
            {
                throw new ArgumentNullException(nameof(countryModel));
            }

            try
            {
                using (var context = new YemekTarifleriContext())
                {
                    bool exists = await context.Countries
                        .AnyAsync(c => c.CountryName == countryModel.CountryName &&
                                       c.City == countryModel.City &&
                                       c.Latitude == countryModel.Latitude &&
                                       c.Longitude == countryModel.Longitude &&
                                       c.UserId == userId);

                    if (exists)
                    {
                        return true; // Country already exists for this user
                    }

                    var countryEntity = new Country
                    {
                        CountryName = countryModel.CountryName,
                        City = countryModel.City,
                        Latitude = countryModel.Latitude,
                        Longitude = countryModel.Longitude,
                        UserId = userId 
                    };

                    context.Countries.Add(countryEntity);
                    await context.SaveChangesAsync();
                    return true; // Indicate success
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false; // Indicate failure
            }
        }





    }
}
