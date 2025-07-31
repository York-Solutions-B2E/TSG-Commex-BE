using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;

namespace TSG_Commex_BE.Repositories.Implementations;

public class CommunicationRepository : ICommunicationRepository
{
    private readonly ApplicationDbContext _context;

    public CommunicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // TODO: Implement all interface methods
}