using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;

namespace MTGCapstone.API.Services
{
    public interface ICardService
    {
        Task<Response<List<CardVMForDeck>>> GetCardsAsync(CardResourceParameters cardResourceParameters);

        Task<Card?> GetCardAsync(int id);
    }
}