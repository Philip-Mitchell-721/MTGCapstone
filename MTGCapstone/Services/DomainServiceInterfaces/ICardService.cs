using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Services
{
    public interface ICardService
    {
        Task<List<Card>?> GetCardsAsync(CardResourceParameters cardResourceParameters);

        Task<Card?> GetCardAsync(int id);
    }
}