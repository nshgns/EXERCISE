using BenchmarkDotNet.Attributes;
using EXERCISE_MVC.Data;
using EXERCISE_MVC.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VSDiagnostics;

namespace EXERCISE_MVC.Benchmarks
{
    [CPUUsageDiagnoser]
    public class PerformanceBenchmark
    {
        private AppDbContext _context;
        [GlobalSetup]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExerciseDb;Trusted_Connection=true;").Options;
            _context = new AppDbContext(options);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [Benchmark]
        public async Task ProductListingWithSearch()
        {
            var search = "ürün";
            var urunSorgusu = _context.Urunler.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                urunSorgusu = urunSorgusu.Where(x => x.Ad.Contains(search));
            }

            var result = await urunSorgusu.ToListAsync();
        }

        [Benchmark]
        public async Task ProductListingWithCategory()
        {
            var kategori = "Elektronik";
            var urunSorgusu = _context.Urunler.AsQueryable();
            if (!string.IsNullOrEmpty(kategori))
            {
                urunSorgusu = urunSorgusu.Where(x => x.Kategori == kategori);
            }

            var result = await urunSorgusu.ToListAsync();
        }

        [Benchmark]
        public async Task GetProductById()
        {
            var result = await _context.Urunler.FirstOrDefaultAsync(x => x.Id == 1);
        }

        [Benchmark]
        public async Task UserLoginCheck()
        {
            var result = _context.Users.FirstOrDefault(u => u.Email == "admin@test.com");
        }
    }
}