﻿using Microsoft.EntityFrameworkCore;
using SeetourAPI.Data.Context;
using SeetourAPI.Data.Models.Users;

namespace SeetourAPI.DAL.Repos
{
    public class TourGuideRepo : ITourGuideRepo
    {
        private readonly SeetourContext _context;

        public TourGuideRepo(SeetourContext context)
        {
            _context = context;
        }

        public TourGuide? GetTourGuide(string id)
        {
            return _context.TourGuides
                .Include(a => a.Tours)
                .ThenInclude(a => a.Photos)
                .Include(a => a.Tours)
                .ThenInclude(a => a.Likes)
                .Include(a => a.Tours)
                .ThenInclude(a => a.Bookings)
                .Include(a => a.Tours)
                .ThenInclude(a => a.Questions)
                .ThenInclude(a => a.TourAnswer)
                .Include(a => a.Tours)
                .ThenInclude(a => a.Bookings)
                .ThenInclude(a => a.Review)
                .Include(a => a.User).FirstOrDefault(t => t.Id == id);
        }
    }
}